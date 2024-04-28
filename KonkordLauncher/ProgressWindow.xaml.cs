using KonkordLibrary.Models;
using System.Windows;
using Tavstal.KonkordLibrary.Managers;

namespace KonkordLauncher
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window, IProgressWindow
    {
        public ProgressWindow()
        {
            InitializeComponent();
        }

        public void UpdateProgressBar(double percent, string text)
        {
            pb_status.Value = percent > pb_status.Maximum ? pb_status.Maximum : percent;
            lab_status.Content = text;
        }

        public void UpdateProgressBarTranslated(double percent, string text, params object[]? args)
        {
            pb_status.Value = percent > pb_status.Maximum ? pb_status.Maximum : percent;
            lab_status.Content = TranslationManager.Translate(text, args);
        }

        /// <summary>
        /// Event handler for minimizing the window when a specific button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void WindowMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        public void ShowWindow()
        {
            Show();
        }

        public bool GetIsVisible()
        {
            return IsVisible;
        }
    }
}
