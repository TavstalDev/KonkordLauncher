using System.Windows;

namespace KonkordLauncher.API.Helpers
{
    public static class NotificationHelper
    {

        public static void SendNotification(string message, string title, MessageBoxImage image = MessageBoxImage.Information)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, image);
        }

        public static void SendInfo(string message, string title)
        {
            SendNotification(message, title);
        }

        public static void SendWarning(string message, string title)
        {
            SendNotification(message, title, MessageBoxImage.Warning);
        }

        public static void SendError(string message, string title)
        {
            SendNotification(message, title, MessageBoxImage.Error);
        }
    }
}
