

namespace CustomWebBrowsers
{
    public interface ICustomBrowserFactory : IHasFreeMethod
    {
        /// <summary>
        /// Not use proxy. Sharing cache.
        /// </summary>
        ICustomBrowser CreateCustomBrowser();
        
        /// <summary>
        /// Not use proxy.
        /// </summary>
        ICustomBrowser CreateCustomBrowser(string cahcePath);
        ICustomBrowser CreateCustomBrowser(string cahcePath, string proxyConfigs);

        
    }
}
