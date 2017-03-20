using System.Windows;

namespace Helpers
{
    /// <summary>
    /// Логика взаимодействия для Window_CustomConsole.xaml
    /// </summary>
    public partial class Window_CustomConsole : Window
    {
        public bool IsClosed { get; private set; } = false;
        public bool isReadingString = false;
        public Window_CustomConsole()
        {
            InitializeComponent();
            textBox_ConsoleInput.Visibility = Visibility.Hidden;
            buttonGo.Visibility = Visibility.Hidden;
            Show();
            Closing += delegate
            {
                IsClosed = true;
            };
        }

        public static Window_CustomConsole Create(string title="Console")
        {
            Window_CustomConsole res = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                res = new Window_CustomConsole();
                res.Title = title;
            });
            return res;
        }

        public void Write(string text)
        {
            if (IsClosed)
                return;
            Dispatcher.Invoke(() =>
            {
                textBox.AppendText(text);
                textBox.ScrollToEnd();
            });
            
        }
        public void WriteLine(string text)
        {
            Write(text + "\n");
        }
        public string Read()
        {
            string res = null;
            Dispatcher.Invoke(() =>
            {
                while (isReadingString)
                {
                    SynchronizationHelper.Pause(200);
                }
                textBox_ConsoleInput.Visibility = Visibility.Visible;
                buttonGo.Visibility = Visibility.Visible;
                isReadingString = true;
                while (isReadingString)
                {
                    SynchronizationHelper.Pause(200);
                }
                res=textBox_ConsoleInput.Text;
                textBox_ConsoleInput.Text = "";
                textBox_ConsoleInput.Visibility = Visibility.Hidden;
                buttonGo.Visibility = Visibility.Hidden;
            });
            return res;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            isReadingString = false;
        }
    }
}
