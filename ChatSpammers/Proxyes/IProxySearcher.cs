using System.Collections.Generic;
using CustomWebBrowsers;

namespace Proxyes
{
    public interface IProxySearcher
    {
        List<string> TryFindProxyes(ProxySearchConfigs configs);

        ICustomBrowser CustomBrowser
        {
            get;
            set;
        }
    }
}
