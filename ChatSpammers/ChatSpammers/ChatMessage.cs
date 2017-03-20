namespace ChatSpammers
{
    public class ChatMessage
    {
        public ChatMessage(string text, bool isCompanionsMessage)
        {
            Text = text;
            IsCompanionsMessage = isCompanionsMessage;
        }
        public string Text
        {
            get;
            private set;
        }
        public bool IsCompanionsMessage
        {
            get;
            private set;
        }
        /// <summary>
        /// ≈сли истина, то TwoChatsHandler не обрабатывает эти сообщени€.
        /// </summary>
        public bool CanResendMessage { get; set; } = true;
    }
}
