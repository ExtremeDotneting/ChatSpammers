namespace ChatSpammers
{
    public interface IChatHandler:IHasFreeMethod
    {
        void StartConversation(ChatHandlerSpecialSettings settings);
        void FinishConversation();
        void UpdateMessagesList();
        void SendMessage(ChatMessage msg);
        ChatMessage GetMessageAt(int msgNum);
        int GetMessagesCount();
        bool IsSearchingCompanion();
        bool IsStillTalking();
        bool IsRealPerson { get; }
    }
}
