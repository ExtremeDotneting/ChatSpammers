namespace ChatSpammers
{
    public interface IFactoryOfChatHandler
    {
        IChatHandler CreateChat1(ChatSpammerSettings settings);
        IChatHandler CreateChat2(ChatSpammerSettings settings);
        void OnChatsDisposed(IChatHandler chatHandler1, IChatHandler chatHandler2, TwoChatsHandlerStatus twoChatsHandlerStatus);
    }
}
