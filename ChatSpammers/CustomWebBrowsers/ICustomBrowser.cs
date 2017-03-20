
using System.Collections.Generic;

namespace CustomWebBrowsers
{
    public interface ICustomBrowser:IHasFreeMethod
    {
        void LoadPage(string urlStr);
        void LoadPageAsync(string urlStr);
        void ExJs(string script);
        string ExJsWithResult(string script);
        void ExJsAsync(string script);

        CustomBrowserStatus BrowserStatus
        {
            get;
        }

        /// <summary>
        /// Return false if result isn`t exist in enum.
        /// </summary>
        bool CheckJsResult_WhiteList(string jsResult, IEnumerable<string> resultsWhiteList);

        /// <summary>
        /// Return false if result is exist in enum.
        /// </summary>
        bool CheckJsResult_BlackList(string jsResult, IEnumerable<string> resultsBlackList);
        bool CheckJsResult_IsUndefined(object jsResult);

        /// <summary>
        /// —брос статуса.
        /// </summary>
        void ResetStatus();

        void WriteToLog(string text);
    }
}
