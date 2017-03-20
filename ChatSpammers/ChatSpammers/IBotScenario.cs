using System;
using System.Threading.Tasks;

namespace ChatSpammers
{
    public interface IBotScenario:ICloneable,IHasFreeMethod
    {
        Task WorkTask { get; }
        Exception LastException { get; }
        bool IsExecuting { get; }
        void ExecuteAsync(BotController_Conversation conversationController);
        void FinishExecution(int timeoutMS);
    }
}
