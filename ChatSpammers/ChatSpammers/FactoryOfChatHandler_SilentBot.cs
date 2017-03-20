namespace ChatSpammers
{
    public class FactoryOfChatHandler_SilentBot:IFactoryOfChatHandler
    {
        public bool ShowMessages { get; set; } = true;

        public IChatHandler CreateChat1(ChatSpammerSettings settings)
        {
            return new ChatHandler_SilentBot(ShowMessages);
        }
        public IChatHandler CreateChat2(ChatSpammerSettings settings)
        {
            return new ChatHandler_SilentBot(ShowMessages);
        }
        public void OnChatsDisposed(IChatHandler chatHandler1, IChatHandler chatHandler2, TwoChatsHandlerStatus twoChatsHandlerStatus)
        {
        }
    }
}
