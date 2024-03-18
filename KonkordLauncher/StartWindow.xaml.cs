using KonkordLauncher.API.Helpers;
using KonkordLauncher.API.Managers;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace KonkordLauncher
{
    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        private readonly double _maxStep = 7;

        public StartWindow()
        {
            InitializeComponent();
            Loaded += Window_Loaded;
        }

        
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double currentStep = 0;
            // Check Java

            lab_status.Content = $"Validating java... ({currentStep + 1}/{_maxStep})";
            await IOHelper.ValidateJava();
            UpdateProgressbar(currentStep);
            await Task.Delay(200); // Little delay, so people can read what the launcher is doing
            currentStep++;

            // Check Folders
            if (!Directory.Exists(IOHelper.MainDirectory))
                lab_status.Content = $"Generating folders... ({currentStep + 1}/{_maxStep})";
            else
                lab_status.Content = $"Validating folders... ({currentStep + 1}/{_maxStep})";
            IOHelper.ValidateDataFolder();
            UpdateProgressbar(currentStep);
            await Task.Delay(200);

            // Check Settings
            currentStep++;
            if (!File.Exists(Path.Combine(IOHelper.MainDirectory, "launcher.json")))
                lab_status.Content = $"Generating launcher settings... ({currentStep + 1}/{_maxStep})";
            else
                lab_status.Content = $"Validating launcher settings... ({currentStep + 1}/{_maxStep})";

            await IOHelper.ValidateSettings();
            UpdateProgressbar(currentStep);
            await Task.Delay(200);
            currentStep++;

            // Check Translations
            lab_status.Content = $"Loading translations... ({currentStep + 1}/{_maxStep})";
            await IOHelper.ValidateTranslations();

            UpdateProgressbar(currentStep);
            await Task.Delay(200);
            currentStep++;

            // Check Manifests
            lab_status.Content = $"Downloading manifests... ({currentStep + 1}/{_maxStep})";
            await IOHelper.ValidateManifests();

            UpdateProgressbar(currentStep);
            await Task.Delay(200);
            currentStep++;

            // Check Accounts
            lab_status.Content = $"Validating accounts... ({currentStep + 1}/{_maxStep})";

            await IOHelper.ValidateAccounts();
            UpdateProgressbar(currentStep);
            await Task.Delay(200);
            currentStep++;

            // Handle authentication
            lab_status.Content = $"Trying to authenticate... ({currentStep + 1}/{_maxStep})";
            UpdateProgressbar(currentStep);

            bool result = await AuthenticationManager.ValidateAuthentication();
            if (result)
            {
                // The authentication is valid, redirecting to launch window
                LaunchWindow window = new LaunchWindow();
                window.Show();
                this.Close();
            }
            else
            {
                // The authentication is invalid, redirecting to login window
                AuthWindow window = new AuthWindow();
                window.Show();
                this.Close();
            }
        }

        #region Functions
        private void UpdateProgressbar(double step)
        {
            pb_status.Value = step / _maxStep * 100;
        }
        #endregion

        #region Window Events
        /// <summary>
        /// Event handler for minimizing the window when a specific button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void WindowMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        #endregion
    }
}
