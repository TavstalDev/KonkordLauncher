using Tavstal.KonkordLibrary.Managers;
using System.Windows;

namespace Tavstal.KonkordLibrary.Helpers
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
            MessageBox.Show(TranslationManager.Translate(messageKey, messageArgs), titleKey.Contains(' ') ? string.Format(titleKey, titleArgs ?? new object[] {}) : TranslationManager.Translate(titleKey, titleArgs));
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

        /// <summary>
        /// Sends a notification message with the specified content, title, and optional image.
        /// </summary>
        /// <param name="message">The message content.</param>
        /// <param name="title">The message title.</param>
        /// <param name="image">Optional: The message image (default is Information).</param>
        public static void SendNotificationMsg(string message, string title, MessageBoxImage image = MessageBoxImage.Information)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, image);
        }

        /// <summary>
        /// Sends an informational message with the specified content and title.
        /// </summary>
        /// <param name="message">The message content.</param>
        /// <param name="title">The message title.</param>
        public static void SendInfoMsg(string message, string title)
        {
            SendNotificationMsg(message, title);
        }

        /// <summary>
        /// Sends a warning message with the specified content and title.
        /// </summary>
        /// <param name="message">The message content.</param>
        /// <param name="title">The message title.</param>
        public static void SendWarningMsg(string message, string title)
        {
            SendNotificationMsg(message, title, MessageBoxImage.Warning);
        }

        /// <summary>
        /// Sends an error message with the specified content and title.
        /// </summary>
        /// <param name="message">The message content.</param>
        /// <param name="title">The message title.</param>
        public static void SendErrorMsg(string message, string title)
        {
            SendNotificationMsg(message, title, MessageBoxImage.Error);
        }
    }
}
