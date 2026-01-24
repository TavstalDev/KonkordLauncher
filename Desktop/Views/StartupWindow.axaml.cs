using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;
using Tavstal.KonkordLauncher.Core.Services;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views;

/// <summary>
/// Represents the startup window of the application, responsible for initializing and validating
/// various components before launching the main application window.
/// </summary>
public partial class StartupWindow : KonkordWindow<StartupViewModel>, IProgressReporter
{
    /// <summary>
    /// Logger instance for the StartupWindow class.
    /// </summary>
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(StartupWindow));

    /// <summary>
    /// Delay in milliseconds for each validation step.
    /// </summary>
    private readonly int _stepDelay = 100;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartupWindow"/> class with default settings.
    /// </summary>
    public StartupWindow()
    {
        InitializeComponent();
        
#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif
        
        DataContext = new StartupViewModel();
        this.WhenActivated(disposables =>
        {
            DataContext.MinimizeWindowInteraction.RegisterHandler(action =>
            {
                WindowState = WindowState.Minimized;
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.MaximizeWindowInteraction.RegisterHandler(action =>
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
        });
    }
    
    /// <summary>
    /// Handles the loading event of the startup window. This method initializes and validates
    /// various components required for the application to function properly.
    /// </summary>
    /// <param name="e">The event data associated with the loaded event.</param>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var settings = await LauncherHelper.GetLauncherSettingsAsync();

            // 0. Set initial status
            SetStatusTranslated("startup.progress.initializing");

            // 1. Validate Directory Structure
            SetStatusTranslated("startup.validation.dataFolder");
            await Task.Delay(_stepDelay);
            bool shouldGenerateIcons = !Directory.Exists(settings.Launcher.IconsDirectoryPath);
            if (!ValidationHelper.ValidateDataFolder())
            {
                SetStatusTranslated("startup.validation.dataFolderFailed");
                return;
            }

            // Generate icons if the directory does not exist
            if (shouldGenerateIcons)
            {
                string[] resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                foreach (string path in resourceNames)
                {
                    if (!path.Contains("Desktop.Assets.Icons"))
                        continue;

                    var fileName = path.Replace("Tavstal.KonkordLauncher.Desktop.Assets.Icons.", "");
                    if (!fileName.EndsWith(".png"))
                        continue;
                    _logger.Debug($"Extracting icon: {fileName}");
                    var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
                    if (stream == null)
                    {
                        _logger.Error($"Failed to get resource stream for {fileName}");
                        continue;
                    }

                    var destPath = Path.Combine(settings.Launcher.IconsDirectoryPath, fileName);
                    await using FileStream outFile = new FileStream(destPath, FileMode.Create, FileAccess.Write);
                    await stream.CopyToAsync(outFile);
                }
            }

            // 2. Validate Translations
            SetStatusTranslated("startup.validation.translations");
            await Task.Delay(_stepDelay);
            await TranslationManager.InitializeTranslations();

            // 3. Validate Accounts
            SetStatusTranslated("startup.validation.accounts");
            await Task.Delay(_stepDelay);
            if (!await ValidationHelper.ValidateAccounts())
            {
                SetStatusTranslated("startup.validation.accountsFailed");
                return;
            }

            // 4. Validate Manifests
            SetStatusTranslated("startup.validation.manifests");
            await Task.Delay(_stepDelay);
            if (!await ValidationHelper.ValidateManifests(this))
            {
                SetStatusTranslated("startup.validation.manifestsFailed");
                return;
            }

            // 5. Validate Java
            SetStatusTranslated("startup.validation.java");
            await Task.Delay(_stepDelay);
            var javaInstallations = JavaHelper.LocateJavaInstallations(settings.Launcher.JavaDirectoryPath);
            bool wasJavaUpdated = false;
            int[] javaVersionsToDownload = [8, 17, 21];
            foreach (int javaVersion in javaVersionsToDownload)
            {
                var jdkResult = javaInstallations.FirstOrDefault(x => x.Major == javaVersion);
                if (jdkResult != null)
                    continue;

                Progress<double> progress = new Progress<double>();
                progress.ProgressChanged += (_, prog) =>
                {
                    SetStatusTranslated("startup.validation.java.download", javaVersion, prog.ToString("0.00"));
                };
                await JavaHelper.DownloadJavaVersionAsync(javaVersion, settings.Launcher.JavaDirectoryPath, progress);
                wasJavaUpdated = true;
            }

            if (wasJavaUpdated)
            {
                if (OSHelper.GetOperatingSystem() != EOperatingSystem.Windows)
                {
                    string[] directories = Directory.GetDirectories(settings.Launcher.JavaDirectoryPath);
                    foreach (string directory in directories)
                    {
                        string javaExecutablePath = Path.Combine(directory, "bin", "java");
                        if (!File.Exists(javaExecutablePath))
                            continue;
                        if (!await FileSystemHelper.MakeExecutableAsync(javaExecutablePath))
                        {
                            AlertWindow window = new AlertWindow(
                                TranslationManager.Translate("startup.validation.java.exec.failedTitle"),
                                TranslationManager.Translate("startup.validation.java.exec.failedMessage",
                                    javaExecutablePath),
                                EAlertType.Error
                            );
                            await window.ShowDialog(this);
                        }
                    }
                }

                JavaHelper.LocateJavaInstallations(settings.Launcher.JavaDirectoryPath, true);
            }

            // 6.1 Refresh GitHub Cache for patches
            SetStatusTranslated("startup.validation.github");
            bool shouldRefreshCache = settings.CacheRefreshDate < DateTime.Now;
            string githubCachePath = Path.Combine(settings.Launcher.CacheDirectoryPath, "github_cache.json");
            if (!File.Exists(githubCachePath) || shouldRefreshCache)
            {
                string? response = await HttpHelper.GetStringAsync(KonkordEndpoints.AllReleases);
                if (response == null)
                {
                    SetStatusTranslated("startup.validation.github.failed");
                    return;
                }
                await File.WriteAllTextAsync(githubCachePath, response);
            }
            
            // 6.2 Refresh skins cache
            AccountData accountData = await LauncherHelper.GetAccountDataAsync();
            foreach (Account account in accountData.Accounts)
            {
                // Refresh head if needed or missing
                string headCachePath = Path.Combine(settings.Launcher.CacheDirectoryPath, "skins", $"{account.Uuid}_head.png");
                if (!File.Exists(headCachePath) || shouldRefreshCache)
                {
                    byte[]? skinResult = await StartlightSkinService.GetHeadshotAsync(account.DisplayName);
                    if (skinResult != null)
                        await File.WriteAllBytesAsync(headCachePath, skinResult);
                    else 
                        _logger.Error($"Failed to fetch headshot for {account.DisplayName}");
                }
            }

            // 7. Check for Updates
            App.IsUpToDate = true;
            if (settings.Launcher.EnableAutomaticUpdates && DateTime.Now > settings.Launcher.NextUpdateCheck)
            {
                SetStatusTranslated("startup.progress.checking");
                await Task.Delay(_stepDelay);

                settings.Launcher.NextUpdateCheck =
                    DateTime.Now.AddHours(settings.Launcher.UpdateInterval == 0 ? 1 : settings.Launcher.UpdateInterval);
                await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherConfigPath, settings);

                if (await CheckUpdateAsync())
                {
                    App.IsUpToDate = false;
                    Close();
                    return;
                }

                _logger.Debug("No updates found, starting application...");
            }
            else
                App.IsUpToDate = !await CheckUpdateAsync(true);

            // 8. Update cache refresh time if needed
            if (shouldRefreshCache)
            {
                settings.CacheRefreshDate = DateTime.Now.AddDays(1);
                await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherConfigPath, settings);
            }

            // 9. Start Main Window
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            {
                _logger.Error(
                    "Failed to start main window: Application lifetime is not IClassicDesktopStyleApplicationLifetime");
                return;
            }

            var oldWindow = desktop.MainWindow;
            var newWindow = new MainWindow
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            desktop.MainWindow = newWindow;
            newWindow.Show();
            if (oldWindow != null)
                oldWindow.Close();
            else
                Close();
        });
    }

    /// <summary>
    /// Handles the window closing event. Ensures that the progress bar is not set to an indeterminate state
    /// when the window is closing, as this may consume unnecessary resources.
    /// </summary>
    /// <param name="e">The event data associated with the window closing event.</param>
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        // Ensure the progress bar is not indeterminate when closing
        // it may use more resources than necessary otherwise
        ProgressBar.IsIndeterminate = false;
    }
    
    private void DragStart_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Start moving the window when left mouse button is pressed
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }
    
    /// <summary>
    /// Checks for updates by comparing the current launcher version with the latest version available on GitHub.
    /// If an update is available, optionally initiates the update process.
    /// </summary>
    /// <param name="justCheck">
    /// A boolean flag indicating whether to only check for updates without initiating the update process.
    /// Defaults to false.
    /// </param>
    private async Task<bool> CheckUpdateAsync(bool justCheck = false)
    {
        try
        {
            // 1. Fetch the latest release information from GitHub
            var result = await HttpHelper.GetAsync(KonkordEndpoints.LatestRelease);
            if (result == null)
            {
                _logger.Error("Failed to get latest release");
                return false;
            }

            if (!result.IsSuccessStatusCode)
            {
                _logger.Error("Failed to get latest release, status code: " + result.StatusCode);
                return false;
            }

            string json = await result.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(json);
            string? latestVersion = jsonObject["tag_name"]?.ToString();

            // 2. Compare the current version with the latest version
            Version? currentVersion;
            object[] versionAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
            if (versionAttributes.Length > 0)
            {
                AssemblyInformationalVersionAttribute informationalVersionAttribute = (AssemblyInformationalVersionAttribute)versionAttributes[0];
                currentVersion = new Version(informationalVersionAttribute.InformationalVersion);
            }
            else
                currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

            if (latestVersion == null || currentVersion == null)
            {
                _logger.Error("Failed to parse latest version or current version");
                return false;
            }

            var latestVer = new Version(latestVersion);
            _logger.Debug($"Comparing versions: current={currentVersion}, latest={latestVer}");
            if (currentVersion >= latestVer)
                return false;
            
            if (!justCheck)
                await UpdateLauncherAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while checking for updates");
            _logger.Error(ex);
            return false;
        }
    }

    /// <summary>
    /// Updates the launcher by starting the updater process.
    /// Determines the appropriate updater executable based on the operating system
    /// and attempts to launch it. Displays an error dialog if the process fails to start.
    /// </summary>
    private async Task UpdateLauncherAsync()
    {
        try
        {
            string fileName = "Updater";
            if (OSHelper.GetOperatingSystem() == EOperatingSystem.Windows)
                fileName += ".exe";
            else if (OSHelper.GetOperatingSystem() == EOperatingSystem.MacOS)
                fileName += ".app";
            
            ProcessStartInfo processInfo = new ProcessStartInfo()
            {
                FileName = Path.Combine(Directory.GetCurrentDirectory(), fileName),
                UseShellExecute = true,
            };
            var process = Process.Start(processInfo);
            if (process == null)
            {
                AlertWindow window = new AlertWindow(
                    TranslationManager.Translate("startup.update.fail"),
                    TranslationManager.Translate("startup.update.failMessage", fileName),
                    EAlertType.Error
                );
                await window.ShowDialog(this);
            }
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while updating the launcher");
            _logger.Error(ex);
        }
    }

    #region IProgressReporter Implementation
    /// <summary>
    /// Sets the progress value for the startup process.
    /// </summary>
    /// <param name="progress">The progress value, typically between 0.0 and 1.0.</param>
    public void SetProgress(double progress)
    {
        if (DataContext == null)
            return;

        DataContext.Progress = progress;
    }

    /// <summary>
    /// Sets the status message for the startup process.
    /// </summary>
    /// <param name="status">The status message to display.</param>
    public void SetStatus(string status)
    {
        if (DataContext == null)
            return;

        DataContext.ProgressText = status;
    }

    /// <summary>
    /// Sets the status message using a translation key and optional arguments.
    /// </summary>
    /// <param name="statusKey">The translation key for the status message.</param>
    /// <param name="args">Optional arguments for formatting the status message.</param>
    public void SetStatusTranslated(string statusKey, params object[]? args)
    {
        if (DataContext == null)
            return;

        DataContext.ProgressText = TranslationManager.Translate(statusKey, args);
    }
    #endregion
}