using System;
using System.Threading.Tasks;
using System.Threading;

namespace ChatSpammers
{
    public class BotScenario_ByDelegate : IBotScenario
    {
        public Task WorkTask { get; private set; }
        Thread workThread;
        Action<BotController_Conversation> workAct;

        public Exception LastException { get; private set; }
        public bool IsFree { get; private set; } = false;
        public bool IsExecuting { get; private set; } = false;

        public BotScenario_ByDelegate(Action<BotController_Conversation> act)
        {
            workAct = act;
        }
        public object Clone()
        {
            return new BotScenario_ByDelegate(workAct);
        }
        public void ExecuteAsync(BotController_Conversation conversationController)
        {
            WorkTask = Task.Run(() =>
            {
                workThread = Thread.CurrentThread;
                IsExecuting = true;
                try
                {
                    workAct(conversationController);
                }
                catch (Exception ex)
                {
                    LastException = ex;
#if DEBUG
                    if (!(ex is BreakThreadException))
                        throw;
#endif
                }
                IsExecuting = false;
            });
        }
        public void FinishExecution(int timeoutMS)
        {
            if (!IsExecuting)
            {
                return;
            }
            if (!WorkTask.Wait(timeoutMS))
            {
                workThread.Abort();
                throw new Exception(string.Format("Bot wasn`t finished by {0} ms!", timeoutMS));
            }
            IsExecuting = false;
        }
        public void Free()
        {
            IsFree = true;
            FinishExecution(30000);
        }
    }
}
