using Proxyes;
using CustomWebBrowsers;

namespace ChatSpammers
{
    public class FactoryOfChatHandler_2Chatvdvoem : IFactoryOfChatHandler
    {
        ICustomBrowserFactory CustomBrowserFactory;
        ProxyDispatcher _ProxyDispatcher;
        CacheDirManager cacheDirManager = new CacheDirManager(ResourcesAndConsts.Instance().FolderForCache + "\\Chatvdvoem");


        public FactoryOfChatHandler_2Chatvdvoem(ICustomBrowserFactory customBrowserFactory)
        {

            CustomBrowserFactory = customBrowserFactory; 
            //_ProxyDispatcher = new ProxyDispatcher();
        }
        public IChatHandler CreateChat1(ChatSpammerSettings settings)
        {
            string cacheFolder = cacheDirManager.GetFreeDir();
            string proxy = null;
            if (settings.UseProxy)
            {
                proxy = _ProxyDispatcher.GetFreeProxy();
                _ProxyDispatcher.SetProxyStatus(proxy, ProxyStatus.UsedNow);
            }
            ICustomBrowser browser = CustomBrowserFactory.CreateCustomBrowser(
                cacheFolder,
                proxy
                );

            ChatHandler_WrapperForChatvdvoemBasicWorker res = new ChatHandler_WrapperForChatvdvoemBasicWorker(browser);
            res.CacheFolder = cacheFolder;
            res.ProxyStr = proxy;
            return res;
        }
        public IChatHandler CreateChat2(ChatSpammerSettings settings)
        {
            return CreateChat1(settings);
        }
        public void OnChatsDisposed(IChatHandler chatHandler1, IChatHandler chatHandler2, TwoChatsHandlerStatus twoChatsHandlerStatus)
        {
            ProxyStatus proxyStatus = (twoChatsHandlerStatus == TwoChatsHandlerStatus.FatalError) ? ProxyStatus.Dead : ProxyStatus.Free;
            if (chatHandler1 != null)
            {
                var ch1 = chatHandler1 as ChatHandler_WrapperForChatvdvoemBasicWorker;
                CacheHandle(ch1.CacheFolder, twoChatsHandlerStatus == TwoChatsHandlerStatus.FatalError);
                if (ch1.ProxyStr != null)
                {
                    _ProxyDispatcher.SetProxyStatus(ch1.ProxyStr, proxyStatus);
                }
            }
            if (chatHandler2 != null)
            {
                var ch2 = chatHandler2 as ChatHandler_WrapperForChatvdvoemBasicWorker;
                CacheHandle(ch2.CacheFolder, twoChatsHandlerStatus == TwoChatsHandlerStatus.FatalError);
                if (ch2.ProxyStr != null)
                {
                    _ProxyDispatcher.SetProxyStatus(ch2.ProxyStr, proxyStatus);
                }
            }

        }

        void CacheHandle(string cacheFolder, bool isFatal)
        {
            if (!string.IsNullOrWhiteSpace(cacheFolder))
            {
                cacheDirManager.SetDirAsFree(cacheFolder);
            }
        }
    }
    
}
