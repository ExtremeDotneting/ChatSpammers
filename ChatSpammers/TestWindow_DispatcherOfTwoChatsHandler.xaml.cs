using System.Windows;
using CustomWebBrowsers;
using System.Threading;

namespace ChatSpammers
{
    /// <summary>
    /// Логика взаимодействия для TestWindow_DispatcherOfTwoChatsHandler.xaml
    /// </summary>
    public partial class TestWindow_DispatcherOfTwoChatsHandler : Window
    {
        DispatcherOfTwoChatsHandler disp;
        public TestWindow_DispatcherOfTwoChatsHandler()
        {
            InitializeComponent();
            Closed += delegate
            {
                AwesomiumCustomBrowserFactory.Instance().Free();
            };
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (disp != null)
                return;

            var botScenario = new BotScenario_ByDelegate((ctrl) =>
            {
                ctrl.WaitMessagesCountInOneOfChats(3);
                ctrl.Chat1.SendMessage(new ChatMessage("НОЖ В ПЕЧЕНЬ - НИКТО НЕ ВЕЧЕН", true));
                ctrl.Chat2.SendMessage(new ChatMessage("НОЖ В ПЕЧЕНЬ - НИКТО НЕ ВЕЧЕН", true));
            });
            botScenario = null;///////

            ChatSpammerSettings settings = new ChatSpammerSettings(
                true,
                null,
                null,
                false,
                false,
                botScenario,
                300
                );

            //var factory = new FactoryOfChatHandler_FromTwoFactories(
            //     new FactoryOfChatHandler_2Chatvdvoem(AwesomiumCustomBrowserFactory.Instance()),
            //     new FactoryOfChatHandler_SilentBot()
            //    );
            var factory =new FactoryOfChatHandler_2Chatvdvoem(AwesomiumCustomBrowserFactory.Instance());

            disp = new DispatcherOfTwoChatsHandler(
                settings,
                factory
                );
            disp.NeededWorkUnitsCount = 1;
            disp.SaveLog = true;
            disp.StartWork();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            disp?.СarefullyStopWork();
            disp = null;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            disp?.AbortWork();
            disp = null;
        }
    }
}
