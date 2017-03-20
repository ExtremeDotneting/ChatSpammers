using System;
using System.Windows;
using System.Windows.Controls;
using Awesomium.Windows.Controls;

namespace CustomWebBrowsers
{
    /// <summary>
    /// Логика взаимодействия для Window_CustomBrowsersPresenter.xaml
    /// </summary>
    public partial class Window_CustomBrowsersPresenter : Window
    {
        static int itemNum = 1;
        int browsersCount=0;
        public bool IsClosed { get; private set; } = false;
        public Window_CustomBrowsersPresenter()
        {
            InitializeComponent();
            browsersTabControl.Items.Clear();
            Show();
            Closing += (s, e) =>
            {
                //e.Cancel = true;
                IsClosed = true;
            };
        }

        public Tuple<TabItem, Control_CustomBrowserPresenter> AddNewWebViewHost(WebViewHost webViewHost)
        {
            TabItem ti = new TabItem();
            //ti.Content = webViewHost;
            var customBrowserPresenter=new Control_CustomBrowserPresenter(webViewHost);
            ti.Content = customBrowserPresenter;
            ti.Header = "br" + itemNum.ToString();
            itemNum++;
            browsersTabControl.Items.Add(ti);
            browsersTabControl.SelectedItem = ti;
            browsersCount++;
            return new Tuple<TabItem, Control_CustomBrowserPresenter>(ti, customBrowserPresenter);
        }
        public void RemoveTabItem(TabItem tabItem)
        {
            browsersTabControl.Items.Remove(tabItem);
            browsersCount--;
            if (browsersCount == 0 && !IsClosed)
                Close();
        }
    }
}
