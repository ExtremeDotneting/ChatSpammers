namespace ChatSpammers
{
    class FactoryOfChatHandler_FromTwoFactories:IFactoryOfChatHandler
    {
        public IFactoryOfChatHandler Factory1 { get; private set; }
        public IFactoryOfChatHandler Factory2 { get; private set; }

        public FactoryOfChatHandler_FromTwoFactories(IFactoryOfChatHandler factory1, IFactoryOfChatHandler factory2)
        {
            Factory1 = factory1;
            Factory2 = factory2;
        }
        public IChatHandler CreateChat1(ChatSpammerSettings settings)
        {
            return Factory1.CreateChat1(settings);
        }
        public IChatHandler CreateChat2(ChatSpammerSettings settings)
        {
            return Factory2.CreateChat1(settings);
        }
        public void OnChatsDisposed(IChatHandler chatHandler1, IChatHandler chatHandler2, TwoChatsHandlerStatus twoChatsHandlerStatus)
        {
            Factory1.OnChatsDisposed(chatHandler1, null, twoChatsHandlerStatus);
            Factory2.OnChatsDisposed(chatHandler2, null, twoChatsHandlerStatus);
        }
    }
}
