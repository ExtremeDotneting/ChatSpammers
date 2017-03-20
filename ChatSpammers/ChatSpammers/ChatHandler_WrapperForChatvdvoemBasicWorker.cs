using CustomWebBrowsers;

namespace ChatSpammers
{
    class ChatHandler_WrapperForChatvdvoemBasicWorker:IChatHandler
    {
        public ChatvdvoemBasicWorker _ChatvdvoemBasicWorker { get; private set; }
        public ChatHandler_WrapperForChatvdvoemBasicWorker(ICustomBrowser browser)
        {
            _ChatvdvoemBasicWorker = new ChatvdvoemBasicWorker(browser);
        }
        public bool IsFree{ get{return _ChatvdvoemBasicWorker.IsFree;}}
        public void FinishConversation()
        {
            _ChatvdvoemBasicWorker.FinishConversation();
        }
        public void Free()
        {
            _ChatvdvoemBasicWorker.Free();
        }
        public ChatMessage GetMessageAt(int msgNum)
        {
            return _ChatvdvoemBasicWorker.GetSaved_MessageAt(msgNum);
        }
        public int GetMessagesCount()
        {
            return _ChatvdvoemBasicWorker.GetSaved_MessagesCount();
        }
        public bool IsSearchingCompanion()
        {
            return _ChatvdvoemBasicWorker.GetWithUpdate_IsSearchingCompanion();
        }
        public bool IsStillTalking()
        {
            return _ChatvdvoemBasicWorker.GetWithUpdate_IsStillTalking();
        }
        public void SendMessage(ChatMessage msg)
        {
            _ChatvdvoemBasicWorker.SendMessage(msg);
        }
        public void StartConversation(ChatHandlerSpecialSettings settings)
        {
            _ChatvdvoemBasicWorker.FindCompanion();
        }
        public void UpdateMessagesList()
        {
            _ChatvdvoemBasicWorker.Update_MessagesList();
        }
        public string ProxyStr { get; set; }
        public bool IsRealPerson { get; set; } = true;
        public string CacheFolder { get; set; }
    }
}
