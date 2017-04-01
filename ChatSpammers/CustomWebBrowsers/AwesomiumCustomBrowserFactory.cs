using System;
using Awesomium.Core;
using Awesomium.Windows.Controls;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using System.IO;
using Helpers;

namespace CustomWebBrowsers
{
    /// <summary>
    /// ������� ���������. ��� �������� ��������� �� ��������� ��� � �������� ������.
    /// </summary>
    public class AwesomiumCustomBrowserFactory : ICustomBrowserFactory
    {
        public interface IWebViewAndAnother : IHasFreeMethod
        {
            WebView WebView { get; }
            Action<Action> Invoker { get; }
            Action<string> WriteToLog { get; }
        }
        /// <summary>
        /// ����� ���������� � AwesomiumCustomBrowser ��� ��������.
        /// </summary>
        class WebViewAndAnother : IWebViewAndAnother
        {

            public WebView WebView { set; get; }
            /// <summary>
            /// ����� ��� ��������� ������� ������ � ��������� � �����, � ������� ��� ����� ����������.
            /// </summary>
            public Action<Action> Invoker { set; get; }
            public Action<string> WriteToLog { set; get; } = delegate { };
            public bool IsFree { get; private set; } = false;
            public void Free()
            {
                if (IsFree)
                    return;
                IsFree = true;
                WriteToLog = delegate { };
                Invoker(() =>
                {
                    if (BrowsersTabItem != null)
                        WindowedBrowsersPresenter.RemoveTabItem(BrowsersTabItem);
                    BrowsersTabItem = null;
                    WindowedBrowsersPresenter = null;
                    WebView?.Dispose();
                    WebView = null;
                    WebViewHost?.Dispose();
                    WebViewHost = null;
                    //���������������� ������� ����� �� ���� ��� ��� WebView, ������������ ������ ����������, � ��������� � 10 ���.
                    SynchronizationHelper.WaitFor(() =>
                    {
                        bool stillWait = true;
                        Invoker(() => { stillWait = WebSession.HasViews; });
                        return stillWait;
                    }, 15000);
                    if (!WebSession.HasViews)
                        WebSession.Dispose();
                    WebSession = null;
                    WebCore.ReleaseMemory();
                });
                //Invoker = null;
                GC.Collect();
            }
            public WebViewHost WebViewHost { set; get; }
            public Window_CustomBrowsersPresenter WindowedBrowsersPresenter { set; get; }
            public TabItem BrowsersTabItem { set; get; }
            public WebSession WebSession { set; get; }
        }
        static AwesomiumCustomBrowserFactory instance;
        public static AwesomiumCustomBrowserFactory Instance()
        {
            if (instance == null)
                instance = new AwesomiumCustomBrowserFactory();
            return instance;
        }

        Thread WebCoreThread { get; set; }
        Dispatcher WebCoreDispatcher { get; set; }
        void InvokeToWebcoreThread(Action method)
        {
            //������ ��� ���������� ������ �� ������ �������� � �������� �� � ��������� �����.
            Exception exFromInvoke = null;
            WebCoreDispatcher.Invoke(() =>
            {
                try
                {
                    if(!IsFree)
                        method?.Invoke();
                }
                catch (Exception ex)
                {
#if DEBUG
                    throw;
#endif
                    exFromInvoke = ex;
                }
            });
            if (exFromInvoke != null)
                throw exFromInvoke;
        }

        AwesomiumCustomBrowserFactory()
        {
            Initialize();
            //ClearCacheGarbage();
        }
        /// <summary>
        /// ��������� ���� ������ ���.
        /// </summary>
        void Initialize()
        {
            Action initAction = () =>
            {
                WebCoreDispatcher = Dispatcher.CurrentDispatcher;
                WebConfig webConfig = new WebConfig();
                webConfig.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36";
                //webConfig.UserAgent = "Mozilla/5.0 (Linux; Android 4.4; Nexus 5 Build/_BuildID_) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/30.0.0.0 Mobile Safari/537.36";
                webConfig.RemoteDebuggingPort = 9997;
                WebCore.Initialize(webConfig);

                Action noAct = new Action(delegate { });
                while (!IsFree)
                {
                    Thread.Sleep(5);
                    Dispatcher.CurrentDispatcher.Invoke(
                        DispatcherPriority.Background,
                        noAct
                    );
                }
                WebCore.Shutdown();
            };
            WebCoreThread = new Thread(new ThreadStart(initAction));
            WebCoreThread.SetApartmentState(ApartmentState.STA);
            WebCoreThread.Priority = ThreadPriority.Highest;
            WebCoreThread.Start();

            AppDomain.CurrentDomain.ProcessExit += delegate
            {
                WebCore.Shutdown();
            };
        }
        Window_CustomBrowsersPresenter WindowedBrowsersPresenter;
        /// <summary>
        /// ������� ��������� WebView �����.
        /// if you send null value, the proxy or cache will be established by default.
        /// ���� �������� ������� ����� - ������������� ��� �������� �������� ����������� ����� ���� � ���.
        /// </summary>
        IWebViewAndAnother CreateWebViewAndAnother(bool isWindowed, string proxyConfigs,string cachePath)
        {
            WebViewType viewType;
            if (isWindowed)
                viewType = WebViewType.Window;
            else
                viewType = WebViewType.Offscreen;
            /*
             * ��� ��� ������� � �����:
             * - ������ �������� ������;
             * - ������ �������� IntPtr ��������, ��� ���������� ���������. Upd: � wpf ������ ���� ������ ������������ WebViewHost;
             * - ������ �������� ��� ����������� ViewType;
             * - ����� ��������� ���������;
             * - ����� �������� ������;
             * - ����� ������ ��� ��������, �� ������ �������� ���� � ���� � ������.
             */

            WebViewAndAnother res = new WebViewAndAnother();
            WebView wv = null;
            InvokeToWebcoreThread(() =>
            {
                WebPreferences webPreferences = new WebPreferences();
                //��������� � ����������� �������������, ����������� ��� ������.
                //webPreferences.WebSecurity = false;
                //webPreferences.CanScriptsAccessClipboard = true;
                //webPreferences.Databases = true;
                //webPreferences.FileAccessFromFileURL = true;
                //webPreferences.JavascriptViewExecute = true;
                //webPreferences.UniversalAccessFromFileURL = true;
                //webPreferences.WebGL = true;  
                
                if (proxyConfigs != null)
                    webPreferences.ProxyConfig = proxyConfigs;
                WebSession ws;
                if (cachePath == null)
                    ws = WebCore.CreateWebSession(webPreferences);
                else
                    ws = WebCore.CreateWebSession(cachePath, webPreferences);
                wv = WebCore.CreateWebView(1000, 1000, ws, viewType);
               

                //��� �� ������������.
                //AddConsoleListener(wv);

                res.WebSession = ws;
                if (viewType == WebViewType.Window)
                {
                    if (EveryTabInOwnWindow || WindowedBrowsersPresenter == null || WindowedBrowsersPresenter.IsClosed)
                        WindowedBrowsersPresenter = new Window_CustomBrowsersPresenter();
                    Window_CustomBrowsersPresenter wbPresenter = WindowedBrowsersPresenter;
                    WebViewHost wwh = new WebViewHost();
                    wwh.View = wv;
                    var tuple = wbPresenter.AddNewWebViewHost(wwh);
                    TabItem currentTabItem = tuple.Item1;
                    res.WebViewHost = wwh;
                    res.WindowedBrowsersPresenter = wbPresenter;
                    res.BrowsersTabItem = currentTabItem;
                    res.WriteToLog = tuple.Item2.WriteLineToLogs;
                }
                else
                {
                    //if offscreen
                }
            });

            res.WebView = wv;
            res.Invoker = InvokeToWebcoreThread;
            //res.Invoker = (act) => {
            //    InvokeToWebcoreThread(act, wv);
            //};
            return res;
        }
        public ICustomBrowser CreateCustomBrowser()
        {
            return CreateCustomBrowser(null, null);
        }
        public ICustomBrowser CreateCustomBrowser(string cahcePath)
        {
            return CreateCustomBrowser(cahcePath, null);
        }
        public ICustomBrowser CreateCustomBrowser(string cahcePath, string proxyConfigs)
        {
            if (IsFree)
                throw new Exception("WebCore is disposed!");
            
            var res= new AwesomiumCustomBrowser(
                CreateWebViewAndAnother(IsWindowedBrowsers, proxyConfigs, cahcePath));
            try
            {
                res.LoadPage("http://www.google.com");
            }
            catch
            {
                res.ResetStatus();
            }
            return res;
        }
        public bool IsFree { get; private set; } = false;
        public void Free()
        {
            WebCore.Shutdown();
            IsFree = true;
        }
        /// <summary>
        /// ������������ ���� ������ ��� ��������� �� ���������.
        /// </summary>
        public bool IsWindowedBrowsers { get; set; } = true;
        public bool EveryTabInOwnWindow { get; set; } = true;
        public void ClearCacheGarbage()
        {
            foreach (var folder in Directory.GetDirectories(ResourcesAndConsts.Instance().FolderForCache))
            {
                try
                {
                    Directory.Delete(folder, true);
                }
                catch { }
            }
        }

        void AddConsoleListener(WebView wv)
        {
#if DEBUG
            //���������� �� ������ ���� ��� ��� ������ ��������� �� ������� �������� � ����.
            //������ ����� ������������ ���� �������, ���� ������� ������� ������������. ��� ����� ��������� �� ������ http://127.0.0.1:9997/ .
            System.IO.File.Delete(Environment.CurrentDirectory + "/awesomium_console_log.txt");
            bool newPage = false;
            wv.ConsoleMessage += (s, e) =>
            {
                string msg = e.Message;
                if (newPage)
                {
                        //msg += "\n\n";
                        newPage = false;
                }
                System.IO.File.AppendAllLines(Environment.CurrentDirectory + "/awesomium_console_log.txt", msg.Split('\n'));

            };
            wv.AddressChanged += (s, e) =>
             {
                 newPage = true;
                 System.IO.File.AppendAllLines(Environment.CurrentDirectory +
                     "/awesomium_console_log.txt", ("\nNavigate to " + (s as IWebView).Source + "").Split('\n'));
             };
#endif
        }

    }

    

}
