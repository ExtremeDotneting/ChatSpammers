using System;
using System.Windows;
using System.Windows.Controls;
using Awesomium.Core;
using Awesomium.Windows.Controls;

namespace CustomWebBrowsers
{
    /// <summary>
    /// Логика взаимодействия для Control_CustomBrowserPresenter.xaml
    /// </summary>
    public partial class Control_CustomBrowserPresenter : UserControl
    {
        bool isConsoleVisible = true;
        public bool IsConsoleVisible
        {
            get { return isConsoleVisible; }
            set
            {
                isConsoleVisible = value;
                if (isConsoleVisible)
                {
                    browsersGrid.Margin = defaultMarginForBrowser;
                    tabControl.Visibility = Visibility.Visible; 
                }
                else
                {
                    browsersGrid.Margin = new Thickness(defaultMarginForBrowser.Left,
                        defaultMarginForBrowser.Top,
                        tabControl.Margin.Right,
                        defaultMarginForBrowser.Bottom
                        );
                    tabControl.Visibility = Visibility.Hidden;
                }
            }
        }
        public bool EnableWorkLogs { get; set; } = false;
        public WebViewHost WVHost { get; private set; }

        Thickness defaultMarginForBrowser;
        public Control_CustomBrowserPresenter(WebViewHost wvh)
        {
            InitializeComponent();
            WVHost = wvh;
            browsersGrid.Children.Add(WVHost);
            WVHost.View.AddressChanged += (s, e) =>
            {
                if (!EnableWorkLogs)
                    return;
                textBox_Url.Text = e.Url.ToString();
                WriteLineToConsole("Navigate to " + e.Url.ToString());
            };
            WVHost.View.ConsoleMessage += (s, e) =>
            {
                if (!EnableWorkLogs)
                    return;
                WriteLineToConsole(e.Message);
            };

            defaultMarginForBrowser = browsersGrid.Margin;
            IsConsoleVisible = false;
        }
        void Invoke(Action act)
        {
            WVHost.View.Invoke(act, new object[] { });
        }

        public string WriteLineToConsole(string text, bool execute = true)
        {
            text = text.Trim();
            Dispatcher.Invoke(() =>
            {
                textBox_ConsoleOutput.AppendText(text + "\n");
                textBox_ConsoleOutput.ScrollToEnd();
            });

            if (text.StartsWith("--enable-work-logs"))
            {
                EnableWorkLogs = true;
                return "";
            }
            if (text.StartsWith("--disable-work-logs"))
            {
                EnableWorkLogs = false;
                return "";
            }

            if (execute)
            {
                string jsResult = ExJsWithResult(text);
                WriteLineToConsole("res>>>>  " + jsResult, false);
                return jsResult;
            }

            return "";
        }
        public void WriteLineToLogs(string text)
        {
            text = text.Trim();
            Dispatcher.Invoke(() =>
            {
                textBox_Logs.AppendText(text + "\n");
                textBox_Logs.ScrollToEnd();
            });

        }
        string ExJsWithResult(string script)
        {
            string res = "not executed";
            Invoke(() =>
            {
               // script = string.Format("(function (){{ {0} }})();", script);
                res =WVHost.View.ExecuteJavascriptWithResult(script);
            });
            return res;
        }
        void OnEnterTextToConsole()
        {
            string text = textBox_ConsoleInput.Text;
            textBox_ConsoleInput.Text="";
            WriteLineToConsole(text);
    
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            string text = textBox_Url.Text;
            Invoke(() =>
            {
                WVHost.View.Source = text.ToUri();
            });
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            OnEnterTextToConsole();
        }

        private void buttonConsoleVisability_Click(object sender, RoutedEventArgs e)
        {
            IsConsoleVisible = !IsConsoleVisible;
        }
    }
}
