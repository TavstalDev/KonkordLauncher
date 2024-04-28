using Tavstal.KonkordLibrary.Helpers;
using Tavstal.KonkordLibrary.Managers;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Diagnostics;
using System.Net.Http;

namespace Tavstal.KonkordLauncher
{
    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        private readonly double _maxStep = 8;

        public StartWindow()
        {
            InitializeComponent();
            Loaded += Window_Loaded;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double currentStep = 0;
            #region Check Launcher Version
            lab_status.Content = $"Validating launcher version... ({currentStep + 1}/{_maxStep})";

            try
            {
                HttpClient client = HttpHelper.GetHttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");

                string? githubResult = await HttpHelper.GetStringAsync("https://api.github.com/repos/TavstalDev/KonkordLauncher/releases/latest", false);
                if (githubResult != null)
                {
                    JObject obj = JObject.Parse(githubResult);

                    string? version = obj["name"]?.ToString();
                    if (version != null && version != App.Version.ToString())
                    {
                        string? downloadFolderPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "{374DE290-123F-4565-9164-39C4925E467B}", String.Empty)?.ToString();
                        if (!string.IsNullOrEmpty(downloadFolderPath))
                        {
                            string newExePath = Path.Combine(downloadFolderPath, $"KonkordLauncher_{version}.exe");
                            if (!File.Exists(newExePath))
                            {
                                string? exeDownloadUrl = obj["assets"]?.ToList()[0]?["browser_download_url"]?.ToString();
                                Progress<double> progress = new Progress<double>();
                                progress.ProgressChanged += (sender, e) =>
                                {
                                    pb_status.Value = e;
                                    lab_status.Content = $"Downloading new launcher... {e:0.00}%";
                                };

                                byte[]? exeByteArray = await HttpHelper.GetByteArrayAsync(exeDownloadUrl, progress);
                                if (exeByteArray != null)
                                {
                                    await File.WriteAllBytesAsync(newExePath, exeByteArray);
                                    NotificationHelper.SendNotificationMsg($"New version of the launcher has been downloaded to your downloads folder. Please delete the .exe of the current launcher.\nPath: {newExePath}", "Downloaded version");
                                    this.Close();
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }

            UpdateProgressbar(currentStep);
            await Task.Delay(200); // Little delay, so people can read what the launcher is doing
            currentStep++;
            #endregion

            #region Check Java Version
            lab_status.Content = $"Validating java... ({currentStep + 1}/{_maxStep})";
            await IOHelper.ValidateJava();
            this.Focus();

            UpdateProgressbar(currentStep);
            await Task.Delay(200); // Little delay, so people can read what the launcher is doing
            currentStep++;
            #endregion

            #region Check Folders
            if (!Directory.Exists(IOHelper.MainDirectory))
                lab_status.Content = $"Generating folders... ({currentStep + 1}/{_maxStep})";
            else
                lab_status.Content = $"Validating folders... ({currentStep + 1}/{_maxStep})";
            IOHelper.ValidateDataFolder();
            UpdateProgressbar(currentStep);
            await Task.Delay(200);
            #endregion

            #region Check Settings
            currentStep++;
            if (!File.Exists(Path.Combine(IOHelper.MainDirectory, "launcher.json")))
                lab_status.Content = $"Generating launcher settings... ({currentStep + 1}/{_maxStep})";
            else
                lab_status.Content = $"Validating launcher settings... ({currentStep + 1}/{_maxStep})";

            await IOHelper.ValidateSettings();
            UpdateProgressbar(currentStep);
            await Task.Delay(200);
            currentStep++;
            #endregion

            #region Check Translations
            lab_status.Content = $"Loading translations... ({currentStep + 1}/{_maxStep})";
            await IOHelper.ValidateTranslations();

            UpdateProgressbar(currentStep);
            await Task.Delay(200);
            currentStep++;
            #endregion

            #region Check Manifests
            lab_status.Content = $"Downloading manifests... ({currentStep + 1}/{_maxStep})";
            await IOHelper.ValidateManifests();

            UpdateProgressbar(currentStep);
            await Task.Delay(200);
            currentStep++;
            #endregion

            #region Check Accounts
            lab_status.Content = $"Validating accounts... ({currentStep + 1}/{_maxStep})";

            await IOHelper.ValidateAccounts();
            UpdateProgressbar(currentStep);
            await Task.Delay(200);
            currentStep++;
            #endregion

            #region Handle authentication
            lab_status.Content = $"Trying to authenticate... ({currentStep + 1}/{_maxStep})";
            UpdateProgressbar(currentStep);

            bool? result = await AuthenticationManager.ValidateAuthentication<LaunchWindow, AuthWindow>(this);
            if (result == null) // The authentication is valid, but it is going to be updated
                return;

            if (result.Value)
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
            #endregion
        }

        private void UpdateProgressbar(double step)
        {
            pb_status.Value = step / _maxStep * 100;
        }

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
