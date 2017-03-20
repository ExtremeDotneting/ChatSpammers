using System;
using System.Collections.Generic;

namespace ChatSpammers
{
    public class TwoChatsHandlerCallbackArgs : EventArgs
    {
        public ChatHandlerCallbackArgs ChatHandlerCallbackArgs1 { get; } = new ChatHandlerCallbackArgs();
        public ChatHandlerCallbackArgs ChatHandlerCallbackArgs2 { get; } = new ChatHandlerCallbackArgs();
        public bool? IsCompleted { get; set; } = null;
    }
    public class ChatHandlerCallbackArgs
    {
        public List<ChatMessage> NewMessages { get;  set; }
        public List<ChatMessage> MessagesToSend { get; set; }
        public bool IsRealPerson { get; set; } = true;
    }
}
