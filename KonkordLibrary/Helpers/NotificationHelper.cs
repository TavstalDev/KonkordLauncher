using KonkordLibrary.Managers;
using System.Windows;

namespace KonkordLibrary.Helpers
{
    public static class NotificationHelper
    {
        public static void SendNotificationTranslated(string messageKey, string titleKey, object[]? messageArgs = null, object[]? titleArgs = null, MessageBoxImage image = MessageBoxImage.Information)
        {
            MessageBox.Show(TranslationManager.Translate(messageKey, messageArgs), TranslationManager.Translate(titleKey, titleArgs));
        }

        public static void SendInfoTranslated(string messageKey, string titleKey, object[]? messageArgs = null, object[]? titleArgs = null)
        {
            SendNotificationTranslated(messageKey, titleKey, messageArgs, titleArgs, MessageBoxImage.Information);
        }

        public static void SendWarningTranslated(string messageKey, string titleKey, object[]? messageArgs = null, object[]? titleArgs = null)
        {
            SendNotificationTranslated(messageKey, titleKey, messageArgs, titleArgs, MessageBoxImage.Warning);
        }

        public static void SendErrorTranslated(string messageKey, string titleKey, object[]? messageArgs = null, object[]? titleArgs = null)
        {
            SendNotificationTranslated(messageKey, titleKey, messageArgs, titleArgs, MessageBoxImage.Error);
        }

        [Obsolete]
        public static void SendNotification(string message, string title, MessageBoxImage image = MessageBoxImage.Information)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, image);
        }

        [Obsolete]
        public static void SendInfo(string message, string title)
        {
            SendNotification(message, title);
        }

        [Obsolete]
        public static void SendWarning(string message, string title)
        {
            SendNotification(message, title, MessageBoxImage.Warning);
        }

        [Obsolete]
        public static void SendError(string message, string title)
        {
            SendNotification(message, title, MessageBoxImage.Error);
        }
    }
}
