using Hardcodet.Wpf.TaskbarNotification;
using System.Windows;

namespace Helpers
{
    public static class TrayHelper
    {
        static TaskbarIcon taskbarIcon;
        static TaskbarIcon TaskbarIcon { get
            {
                if (taskbarIcon == null)
                    taskbarIcon = new TaskbarIcon();
                return taskbarIcon;

            }
        }

        public static void ShowTextInTray(string title, string msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TaskbarIcon.ShowBalloonTip(title, msg, BalloonIcon.Info);
            });
        }
    }
}
