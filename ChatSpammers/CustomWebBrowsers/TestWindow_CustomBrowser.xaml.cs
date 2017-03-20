using System;
using System.Windows;
using System.Threading;
using System.IO;

namespace CustomWebBrowsers
{
    /// <summary>
    /// Логика взаимодействия для TestWindow_CustomBrowser.xaml
    /// </summary>
    public partial class TestWindow_CustomBrowser : Window
    {
        public TestWindow_CustomBrowser()
        {
            InitializeComponent();
            browserFactory = AwesomiumCustomBrowserFactory.Instance();
        }

        ICustomBrowser cusBr;
        ICustomBrowserFactory browserFactory;
        string jsScript { get { return File.ReadAllText(Environment.CurrentDirectory + "/script_for_test.js"); } }

        string Url
        {
            get
            {
                string res = null;
                Application.Current.Dispatcher.Invoke(() =>
                {
                   res=textBox.Text.Trim();
                });
                return res;
            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            if (cusBr == null)
                return;

            cusBr.LoadPageAsync(Url);
            
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            if (cusBr == null)
                return;
            
            cusBr.ExJs(jsScript);
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            if (cusBr == null)
                return;
            
            cusBr.ExJsAsync(jsScript);

            
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            if (cusBr == null)
                return;
            
            string str=cusBr.ExJsWithResult(jsScript).ToString();

            
            MessageBox.Show("Result: "+str);
        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
            {
                

                cusBr = browserFactory.CreateCustomBrowser();
                

                
            }).Start();
            label2.Content = "Created in: background thread.";
        }

        private void button8_Click(object sender, RoutedEventArgs e)
        {
            if (cusBr == null)
                return;
            new Thread(() =>
            {
                
                cusBr.LoadPage(Url);
                
            }).Start();
        }

        private void button9_Click(object sender, RoutedEventArgs e)
        {
            if (cusBr == null)
                return;
            new Thread(() =>
            {
                
                cusBr.LoadPageAsync(Url);
                
            }).Start();
        }

        private void button10_Click(object sender, RoutedEventArgs e)
        {
            if (cusBr == null)
                return;
            new Thread(() =>
            {
                
                cusBr.ExJs(jsScript);

                
            }).Start();
        }

        private void button11_Click(object sender, RoutedEventArgs e)
        {
            if (cusBr == null)
                return;
            new Thread(() =>
            {
                
                cusBr.ExJsAsync(jsScript);

                
            }).Start();
        }

        private void button12_Click(object sender, RoutedEventArgs e)
        {
            if (cusBr == null)
                return;
            new Thread(() =>
            {
                string str = cusBr.ExJsWithResult(jsScript).ToString();        
                MessageBox.Show("Result: " + str);
            }).Start();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (cusBr != null)
                cusBr.Free();
            cusBr = browserFactory.CreateCustomBrowser(); 
            label2.Content = "Created in: main thread.";  

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (cusBr == null)
                return;
            cusBr.LoadPage(Url);
            
        }
    }
}
