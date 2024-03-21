using KonkordLibrary.Managers;
using System.Windows;

namespace KonkordLibrary.Helpers
{
    public static class NotificationHelper
    {
        /// <summary>
        /// Sends a notification with translated message and title.
        /// </summary>
        /// <param name="messageKey">The key for the translated message.</param>
        /// <param name="titleKey">The key for the translated title.</param>
        /// <param name="messageArgs">Optional arguments to format into the translated message.</param>
        /// <param name="titleArgs">Optional arguments to format into the translated title.</param>
        /// <param name="image">The type of message box image.</param>
        public static void SendNotificationTranslated(string messageKey, string titleKey, object[]? messageArgs = null, object[]? titleArgs = null, MessageBoxImage image = MessageBoxImage.Information)
        {
            MessageBox.Show(TranslationManager.Translate(messageKey, messageArgs), TranslationManager.Translate(titleKey, titleArgs));
        }

        /// <summary>
        /// Sends an informational message with translated message and title.
        /// </summary>
        /// <param name="messageKey">The key for the translated message.</param>
        /// <param name="titleKey">The key for the translated title.</param>
        /// <param name="messageArgs">Optional arguments to format into the translated message.</param>
        /// <param name="titleArgs">Optional arguments to format into the translated title.</param>
        public static void SendInfoTranslated(string messageKey, string titleKey, object[]? messageArgs = null, object[]? titleArgs = null)
        {
            SendNotificationTranslated(messageKey, titleKey, messageArgs, titleArgs, MessageBoxImage.Information);
        }

        /// <summary>
        /// Sends a warning message with translated message and title.
        /// </summary>
        /// <param name="messageKey">The key for the translated message.</param>
        /// <param name="titleKey">The key for the translated title.</param>
        /// <param name="messageArgs">Optional arguments to format into the translated message.</param>
        /// <param name="titleArgs">Optional arguments to format into the translated title.</param>
        public static void SendWarningTranslated(string messageKey, string titleKey, object[]? messageArgs = null, object[]? titleArgs = null)
        {
            SendNotificationTranslated(messageKey, titleKey, messageArgs, titleArgs, MessageBoxImage.Warning);
        }

        /// <summary>
        /// Sends an error message with translated message and title.
        /// </summary>
        /// <param name="messageKey">The key for the translated message.</param>
        /// <param name="titleKey">The key for the translated title.</param>
        /// <param name="messageArgs">Optional arguments to format into the translated message.</param>
        /// <param name="titleArgs">Optional arguments to format into the translated title.</param>
        public static void SendErrorTranslated(string messageKey, string titleKey, object[]? messageArgs = null, object[]? titleArgs = null)
        {
            SendNotificationTranslated(messageKey, titleKey, messageArgs, titleArgs, MessageBoxImage.Error);
        }

        // Todo remove these and use translated versions instead

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
