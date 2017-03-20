using System;
using System.Collections.Generic;
using Awesomium.Core;
using Helpers;
using System.Threading.Tasks;
using System.Threading;

namespace CustomWebBrowsers
{
    public class AwesomiumCustomBrowserException : Exception
    {
        public AwesomiumCustomBrowserException() : base() { }
        public AwesomiumCustomBrowserException(string message) : base(message) { }
    }

    public class AwesomiumCustomBrowser : ICustomBrowser
    {
        class ActionAndBool
        {
            public Action Action;
            public bool IsExecuted=false;

            public ActionAndBool() { }
            public ActionAndBool(Action action)
            {
                Action = action;
            }
        }

        int timeoutForWaitingMS = 30000;
        CustomBrowserStatus customBrowserStatus = CustomBrowserStatus.Completed;
        int browserTasks = 0;
        Task WebViewUpdateTask { get; set; }
        List<ActionAndBool> actionToInvoke = new List<ActionAndBool>();
            
        public bool IsFree{get;private set;} = false;
        /// <summary>
        /// Еще не проверял правильно ли работает это свойство.
        /// </summary>
        public CustomBrowserStatus BrowserStatus
        {
            get
            {
                return customBrowserStatus;
            }
            private set
            {
                if (customBrowserStatus != CustomBrowserStatus.HasError)
                    customBrowserStatus = value;
            }
        }
        AwesomiumCustomBrowserFactory.IWebViewAndAnother WebViewAndAnother { get;set;}
        public WebView WebViewProperty { get { return WebViewAndAnother.WebView; } }

        /// <summary>
        /// Лучше используйте фабрику.
        /// </summary>
        public AwesomiumCustomBrowser(AwesomiumCustomBrowserFactory.IWebViewAndAnother webViewAndAnother)
        {
            WebViewAndAnother = webViewAndAnother;
            WebViewUpdateTask = Task.Run(new Action(WebViewUpdateMethod));
        }
        public void Free()
        {
            if (IsFree)
                return;
            IsFree = true;
            WebViewUpdateTask?.Wait();
            WebViewUpdateTask = null;
            WebViewAndAnother?.Free();
            WebViewAndAnother = null;
        }
        public void WebViewUpdateMethod()
        {
            Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
            while (!IsFree)
            {
                Thread.Sleep(3);
                if (actionToInvoke.Count > 0)
                {
                    var actionToInvokeBuf = actionToInvoke;
                    actionToInvoke = new List<ActionAndBool>();
                    for (int i=0; i< actionToInvokeBuf.Count;i++)
                    {
                        if (IsFree)
                            break;
                        var item = actionToInvokeBuf[i];
                        if (item == null)
                            continue;
                        StartedOneTask();
                        InvokeToWebCoreThread(item.Action);
                        item.IsExecuted = true;
                        CompletedOneTask();
                    }
                }
            }
        }

        /// <summary>
        /// Return false if result isn`t exist in enum.
        /// </summary>
        public bool CheckJsResult_WhiteList(string jsResult, IEnumerable<string> resultsWhiteList)
        {
            foreach (string item in resultsWhiteList)
            {
                if (jsResult == item)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Return false if result is exist in enum.
        /// </summary>
        public bool CheckJsResult_BlackList(string jsResult, IEnumerable<string> resultsBlackList)
        {
            foreach (string item in resultsBlackList)
            {
                if (jsResult == item)
                    return false;
            }
            return true;
        }
        public bool CheckJsResult_IsUndefined(object jsResult)
        {
            try
            {
                return (jsResult as JSValue).IsUndefined;
            }
            catch { }

            return (jsResult as string).Trim() == "undefined";
        }

        public void LoadPage(string urlStr)
        {
            InvokeToWebViewThread(() =>
            {
                WebViewProperty.Source = urlStr.ToUri();
                WaitWhileLoading();
            });
        }
        public void LoadPageAsync(string urlStr)
        {
            Task.Run(() =>
            {
                LoadPage(urlStr);
            });
        }
        public void ExJs(string script)
        {
            InvokeToWebViewThread(() =>
            {
                WaitWhileLoading();
                WebViewProperty.ExecuteJavascriptWithResult(script);
                WaitWhileLoading();
            });
        }
        public string ExJsWithResult(string script)
        {
            string res = null;
            InvokeToWebViewThread(() =>
            {
                WaitWhileLoading();
                res =WebViewProperty.ExecuteJavascriptWithResult(script);
                WaitWhileLoading();
            });
            return res;
        }
        public void ExJsAsync(string script)
        {
            Task.Run(() =>
            {
                ExJs(script);
            });
        }
        public void WriteToLog(string text)
        {
            WebViewAndAnother?.WriteToLog(text);
        }
        
        /// <summary>
        /// Сброс статуса.
        /// </summary>
        public void ResetStatus()
        {
            browserTasks = 0;
            customBrowserStatus = CustomBrowserStatus.Completed;
        }
        /// <summary>
        /// Если хотите добавлять новые методы по работе с WebView - добавляйте их через єтот метод.
        /// </summary>
        void InvokeToWebViewThread(Action method)
        {
            if (IsFree)
                return;
            var actAndBool = new ActionAndBool(method);
            actionToInvoke.Add(actAndBool);
            while(!IsFree && !actAndBool.IsExecuted)
            {
                SynchronizationHelper.Pause(10);
            }

        }
        void InvokeToWebCoreThread(Action method)
        {
            if (IsFree)
                return;
            WebViewAndAnother.Invoker(method);
        }
        void WaitWhileLoading()
        {
            while (!IsFree && CheckIfWebViewIsLoading())
            {
                SynchronizationHelper.Pause(3);
            }
        }
        bool CheckIfWebViewIsNormal()
        {
            bool res = true;
            if (!WebViewProperty.IsLive || WebViewProperty.IsCrashed || WebViewProperty.IsDisposed || !WebViewProperty.IsResponsive)
            {
                BrowserStatus = CustomBrowserStatus.HasError;
                res = false;
            }
            return res;
        }
        bool CheckIfWebViewIsLoading()
        {
            bool res = CheckIfWebViewIsNormal() && (WebViewProperty.IsLoading || WebViewProperty.IsNavigating || !WebViewProperty.IsDocumentReady);
            return res;
        }
        void StartedOneTask()
        {
            browserTasks++;
            BrowserStatus = CustomBrowserStatus.Working;
        }
        void CompletedOneTask()
        {
            browserTasks--;
            if(browserTasks<=0)
                BrowserStatus = CustomBrowserStatus.Completed;
        }

    }
}
