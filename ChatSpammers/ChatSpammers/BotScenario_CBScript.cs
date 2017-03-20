using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatSpammers
{
    public class BotScenario_GFScript : IBotScenario
    {
        List<Action<BotController_Conversation>> actionsToBeExecuted;

        public Task WorkTask { get; private set; }
        Thread workThread;
        

        public Exception LastException { get; private set; }
        public bool IsFree { get; private set; } = false;
        public bool IsExecuting { get; private set; } = false;

        public BotScenario_GFScript(string GFScript)
        {
            //ParseScript(GFScript);
        }
        public object Clone()
        {
            throw new NotImplementedException();
        }
        public void ExecuteAsync(BotController_Conversation conversationController)
        {
            WorkTask = Task.Run(() =>
            {
                workThread = Thread.CurrentThread;
                IsExecuting = true;
                try
                {
                    BotFunc(conversationController);
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
            actionsToBeExecuted = null;
        }
        void BotFunc(BotController_Conversation botController)
        {
            for (int i =0; i< actionsToBeExecuted.Count; i++)
            {
                if (IsFree)
                    return;
                actionsToBeExecuted[i](botController);
            }
        }
        

    }

    
}
