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
        /// ���� ������, �� TwoChatsHandler �� ������������ ��� ���������.
        /// </summary>
        public bool CanResendMessage { get; set; } = true;
    }
}
