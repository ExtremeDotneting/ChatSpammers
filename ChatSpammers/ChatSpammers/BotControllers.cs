using System;
using System.Collections.Generic;
using Helpers;
using System.Threading.Tasks;

namespace ChatSpammers
{

    public class BotController_Conversation
    {
        bool? isCompleted = null;

        public BotController_Chat Chat1 { get; }
        public BotController_Chat Chat2 { get; }
        public bool IsConversationFinished { get; set; } = false;

        public BotController_Conversation(TwoChatsHandlerCallbackArgs args)
        {
            Chat1 = new BotController_Chat(this);
            Chat2 = new BotController_Chat(this);
            OnCallback(args);
        }
        public void OnCallback(TwoChatsHandlerCallbackArgs args)
        {
            Chat1.OnCallback(args.ChatHandlerCallbackArgs1);
            Chat2.OnCallback(args.ChatHandlerCallbackArgs2);
            args.IsCompleted = isCompleted;
        }
        public void FinishConversation(bool isCompleted)
        {
            this.isCompleted = isCompleted;
            while(!IsConversationFinished)
                SynchronizationHelper.Pause(200);
        }
        public void WaitMiliseconds(int ms)
        {
            while (ms > 0 )
            {
                DoEvents();
                ms -= 200;
            }
        }
        public void WaitMessagesCountInOneOfChats(int count)
        {
            while (Chat1.GetMessagesCount() < count && Chat2.GetMessagesCount() < count)
            {
                DoEvents();
            }
        }
        public void WaitAnySubstringInMessageInAnyChat(string[] substrings)
        {
            Task task1= Task.Run(() =>
            {
                Chat1.WaitAnySubstringInMessage(substrings);
            });
            Task task2 = Task.Run(() =>
            {
                Chat2.WaitAnySubstringInMessage(substrings);
            });
            Task.WaitAny(new Task[] { task1, task2 });
        }
        public void WaitSubstringInMessageInAnyChat(string substring)
        {
            Task task1 = Task.Run(() =>
             {
                 Chat1.WaitSubstringInMessage(substring);
             });
            Task task2 = Task.Run(() =>
            {
                Chat2.WaitSubstringInMessage(substring);
            });
            Task.WaitAny(new Task[] { task1, task2 });
        }
        /// <summary>
        /// Break scenario thread if conversation finished.
        /// </summary>
        public void ThrowStopExceptionIfNeed()
        {
            if (IsConversationFinished)
                throw new BreakThreadException("Conversation was finished before scenario. It`s not exception, just used to break scenario thread.");
        }
        public void DoEvents()
        {
            ThrowStopExceptionIfNeed();
            SynchronizationHelper.Pause(100);
        }
    }
    
    public class BotController_Chat
    {
        BotController_Conversation owner;
        List<ChatMessage> messagesList = new List<ChatMessage>();
        List<ChatMessage> messagesToSendList = new List<ChatMessage>();

        public bool IsRealPerson { get; private set; } = false;

        public BotController_Chat(BotController_Conversation owner)
        {
            this.owner =owner;
        }
        public void OnCallback(ChatHandlerCallbackArgs args)
        {
            IsRealPerson = args.IsRealPerson;
            if (args.NewMessages != null)
                messagesList.AddRange(args.NewMessages);
            args.MessagesToSend = messagesToSendList;
            messagesToSendList = new List<ChatMessage>();
        }
        public int GetMessagesCount()
        {
            owner.ThrowStopExceptionIfNeed();
            return messagesList.Count;
        }
        public ChatMessage GetMessageAt(int index)
        {
            owner.ThrowStopExceptionIfNeed();
            try
            {
                return messagesList[index];
            }
            catch
            {
                return null;
            }
        }
        public void SendMessage(ChatMessage msg)
        {
            owner.ThrowStopExceptionIfNeed();
            messagesToSendList.Add(msg);
        }
        public void WaitMessagesCount(int count)
        {
            while (GetMessagesCount() < count)
            {
                owner.DoEvents();
            }
        }
        public void WaitSubstringInMessage(string substring)
        {
            substring = substring.ToLower();
            int msgNum = 0;
            while (true)
            {
                for (int i = msgNum; i < messagesList.Count; i++)
                {
                    if (messagesList[i].Text.ToLower().IndexOf(substring) < 0)
                        return;
                    msgNum++;
                    if (msgNum % 10 == 9)
                        owner.DoEvents();
                }
                owner.DoEvents();
            }
        }
        public void WaitAnySubstringInMessage(string[] substrings)
        {
            for (int i = 0; i < substrings.Length; i++)
                substrings[i] = substrings[i].ToLower();
            int msgNum = 0;
            while (true)
            {
                for (int i = msgNum; i < messagesList.Count; i++)
                {
                    string msgText = messagesList[i].Text.ToLower();
                    for (int j = 0; j < substrings.Length; j++)
                    {
                        if (messagesList[i].Text.ToLower().IndexOf(substrings[j]) < 0)
                            return;
                    }
                    msgNum++;
                    if (msgNum % 10 == 9)
                        owner.DoEvents();
                }
                owner.DoEvents();
            }
        }
    }
    public class BreakThreadException:Exception
    {
        public BreakThreadException() : base() { }
        public BreakThreadException(string msg) : base(msg) { }
    }
    
}
