using System.Collections.Generic;
using System.Threading;
using Helpers;

namespace ChatSpammers
{
    //Ничего не делает. Просто mock-объект для исполнения вашего сценария.
    public class ChatHandler_SilentBot : IChatHandler
    {
        static int botsCount = 0;
        bool isStillTalking = true;
        List<ChatMessage> chatMessages = new List<ChatMessage>();
        Window_CustomConsole consoleForMessages;
        bool showConsole;
        Thread consoleReadThread;

        public ChatHandler_SilentBot(bool showConsole)
        {
            this.showConsole = showConsole;
            if (showConsole)
            {
                consoleForMessages = Window_CustomConsole.Create(string.Format("SilentBot{0} Log", ++botsCount));
                StartReadMessageLoop();
            }
        }

        public bool IsFree { get; private set; } = false;
        public bool IsRealPerson { get;} = false;

        public void FinishConversation()
        {
            isStillTalking = false;
            chatMessages = new List<ChatMessage>();
            WriteLineToConsole("Finish conversation!\n");
        }
        public void Free()
        {
            FinishConversation();
            StopReadMessageLoop();
            IsFree = true;
            
        }
        public ChatMessage GetMessageAt(int msgNum)
        {
            return chatMessages[msgNum];
        }
        public int GetMessagesCount()
        {
            return chatMessages.Count;
        }
        public bool IsSearchingCompanion()
        {
            return false;
        }
        public bool IsStillTalking()
        {
            return isStillTalking;
        }
        public void SendMessage(ChatMessage msg)
        {
            //Инвертируем тип отправителя
            msg = new ChatMessage(
                msg.Text,
                !msg.IsCompanionsMessage
                );
            msg.CanResendMessage = false;

            chatMessages.Add(msg);
            string msgStr = string.Format(
                "{0}-> \" {1} \";",
                msg.IsCompanionsMessage ? "Nekto" : "Current",
                msg.Text.Replace("\n", "\n\t")
                );
            WriteLineToConsole(msgStr);
        }
        public void StartConversation(ChatHandlerSpecialSettings settings)
        {
            WriteLineToConsole("Start conversation!");
        }
        public void UpdateMessagesList()
        {
        }
        public void WriteLineToConsole(string text)
        {
            if (showConsole)
                consoleForMessages.WriteLine(text);
        }
        void StartReadMessageLoop()
        {
            consoleReadThread=new Thread(() =>
            {
                while (consoleReadThread == Thread.CurrentThread)
                {
                    string text=consoleForMessages.Read().Trim();
                    var msg = new ChatMessage(
                        text,
                        true
                        );
                    chatMessages.Add(msg);
                }
            });
            consoleReadThread.Priority = ThreadPriority.BelowNormal;
            consoleReadThread.Start();
        }
        void StopReadMessageLoop()
        {
            if (consoleReadThread == null)
                return;
            Thread thr = consoleReadThread;
            consoleReadThread = null;
            thr.Join(10000);
            if (thr.IsAlive)
            {
                thr.Abort();
            }
        }
    }
}
