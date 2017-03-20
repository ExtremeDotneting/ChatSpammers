using System;
using System.Collections.Generic;

namespace Proxyes
{
    public class ProxyDispatcher
    {
        public void TryFindProxyes()
        {
            throw new NotImplementedException();
        }

        public string GetFreeProxy()
        {
            throw new NotImplementedException();
        }

        public void SetProxyStatus(string proxy, ProxyStatus proxyStatus)
        {
            throw new NotImplementedException();
        }

        public List<string> ProxyListDead { get; private set; }

        public List<string> ProxyListFree { get; private set; }

        public List<string> ProxyListUsedNow { get; private set; }

        public IProxySearcher ProxySearcher { get; private set; }

        public ProxySearchConfigs ProxySearchConfigsProperty { get; private set; }
    }
}
