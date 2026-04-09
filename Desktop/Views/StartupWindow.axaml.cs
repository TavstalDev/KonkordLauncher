using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Config;
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
using Velopack;

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
    private const int _stepDelay = 100;
    
    private const int _maxParallelDownloads = 16;

    private readonly int[] _javaVersionsToDownload = [8, 17, 21];

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
    
    #region Events
    /// <summary>
    /// Handles the loading event of the startup window. This method initializes and validates
    /// various components required for the application to function properly.
    /// </summary>
    /// <param name="e">The event data associated with the loaded event.</param>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Task.Run(async () => {
            try
            {
                await InitAsync();
            }
            catch (Exception ex)
            {
                _logger.Error("An error occurred during startup initialization:");
                _logger.Error(ex);
            } 
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
    
    /// <summary>
    /// Begins a window drag operation when the primary (left) mouse button is pressed on the drag region.
    /// </summary>
    /// <param name="sender">The source of the pointer event (typically the control that raised the event). May be null.</param>
    /// <param name="e">The <see cref="PointerPressedEventArgs"/> instance containing event data for the pointer press.</param>
    private void DragStart_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Start moving the window when left mouse button is pressed
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }
    #endregion

    private async Task InitAsync(CancellationToken cancellationToken = default)
    {
        var settings = await LauncherHelper.GetLauncherSettingsAsync(cancellationToken);

        // Set initial status
        UpdateStatusTranslated("startup.progress.initializing");

        // Validate Directory Structure
        UpdateStatusTranslated("startup.validation.dataFolder");
        await Task.Delay(_stepDelay, cancellationToken);
        bool shouldGenerateIcons = !Directory.Exists(settings.Launcher.IconsDirectoryPath);
        if (!ValidationHelper.ValidateDataFolder())
        {
            UpdateStatusTranslated("startup.validation.dataFolderFailed");
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
                await stream.CopyToAsync(outFile, cancellationToken);
            }
        }

        // Validate Translations
        UpdateStatusTranslated("startup.validation.translations");
        await Task.Delay(_stepDelay, cancellationToken);
        await TranslationManager.InitializeTranslations(cancellationToken: cancellationToken);

        // Validate Accounts
        UpdateStatusTranslated("startup.validation.accounts");
        await Task.Delay(_stepDelay, cancellationToken);
        if (!await ValidationHelper.ValidateAccounts(cancellationToken))
        {
            UpdateStatusTranslated("startup.validation.accountsFailed");
            return;
        }

        // Validate Manifests
        UpdateStatusTranslated("startup.validation.manifests");
        await Task.Delay(_stepDelay, cancellationToken);
        if (!await ValidationHelper.ValidateManifests(this, cancellationToken))
        {
            UpdateStatusTranslated("startup.validation.manifestsFailed");
            return;
        }

        // Validate Java
        await ValidateJavaAsync(settings, cancellationToken);

        // Refresh GitHub Cache & Skins Cache
        bool shouldRefreshCache = await ValidateCachesAsync(settings, cancellationToken);

        // Check for Updates
        App.IsUpToDate = true;
        if (settings.Launcher.EnableAutomaticUpdates && DateTime.Now > settings.Launcher.NextUpdateCheck)
        {
            UpdateStatusTranslated("startup.progress.checking");
            await Task.Delay(_stepDelay, cancellationToken);

            settings.Launcher.NextUpdateCheck = DateTime.Now.AddHours(MathHelper.Clamp(settings.Launcher.UpdateInterval, 1, int.MaxValue));
            await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherConfigPath, settings, cancellationToken);

            if (await CheckUpdateAsync(cancellationToken: cancellationToken))
            {
                _logger.Debug("Update found and applied, exiting startup to restart application...");
                return;
            }
            _logger.Debug("No updates found, starting application...");
        }
        else
            App.IsUpToDate = !await CheckUpdateAsync(true, cancellationToken);

        // Update cache refresh time if needed
        if (shouldRefreshCache)
        {
            settings.CacheRefreshDate = DateTime.Now.AddDays(1);
            await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherConfigPath, settings, cancellationToken);
        }

        // Start Main Window
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            _logger.Error("Failed to start main window: Application lifetime is not IClassicDesktopStyleApplicationLifetime");
            return;
        }

        Dispatcher.UIThread.Invoke(() =>
        {
            desktop.MainWindow = new MainWindow
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            desktop.MainWindow.Show();
            Close();
        });
    }

    private async Task ValidateJavaAsync(CoreConfig settings, CancellationToken cancellationToken = default)
    {
        UpdateStatusTranslated("startup.validation.java");
        await Task.Delay(_stepDelay, cancellationToken);
        var javaInstallations = JavaHelper.LocateJavaInstallations(settings.Launcher.JavaDirectoryPath);
        bool wasJavaUpdated = false;
        foreach (int javaVersion in _javaVersionsToDownload)
        {
            var jdkResult = javaInstallations.FirstOrDefault(x => x.Major == javaVersion);
            if (jdkResult != null)
                continue;

            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (_, prog) =>
            {
                UpdateStatusTranslated("startup.validation.java.download", javaVersion,
                    prog.ToString("0.00"));
            };
            await JavaHelper.DownloadJavaVersionAsync(javaVersion, settings.Launcher.JavaDirectoryPath, progress, cancellationToken);
            wasJavaUpdated = true;
        }

        if (!wasJavaUpdated)
            return;
        
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

    private async Task<bool> ValidateCachesAsync(CoreConfig settings, CancellationToken cancellationToken = default)
    {
        // Refresh github cache
        UpdateStatusTranslated("startup.validation.github");
        bool shouldRefreshCache = settings.CacheRefreshDate < DateTime.Now;
        string githubCachePath = Path.Combine(settings.Launcher.CacheDirectoryPath, "github_cache.json");
        if (!File.Exists(githubCachePath) || shouldRefreshCache)
        {
            string? response = await HttpHelper.GetStringAsync(KonkordEndpoints.AllReleases, cancellationToken);
            if (response == null)
            {
                _logger.Error("Failed to fetch GitHub cache data");
                response = "[]";
            }
            await File.WriteAllTextAsync(githubCachePath, response, cancellationToken);
        }

        // Refresh skins cache
        UpdateStatusTranslated("startup.validation.skins");
        AccountData accountData = await LauncherHelper.GetAccountDataAsync(cancellationToken);
        var semaphore = new SemaphoreSlim(_maxParallelDownloads);
        var tasks = new List<Task>();
        foreach (Account account in accountData.Accounts)
        {
            await semaphore.WaitAsync(cancellationToken);
            Task t = Task.Run(async () =>
            {
                try
                {
                    foreach (var skin in account.Skins)
                        await SkinService.FetchSkins(settings.Launcher.CacheDirectoryPath, account.Id, account.Uuid,
                            skin, cancellationToken);
                    var capes = account.MojangProfile?.Capes ?? [];
                    if (capes.Count > 0)
                        await SkinService.FetchCapes(settings.Launcher.CacheDirectoryPath, capes, cancellationToken);
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken);
            tasks.Add(t);
        }

        await Task.WhenAll(tasks);
        return shouldRefreshCache;
    }

    private async Task<bool> CheckUpdateAsync(bool justCheck = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var mgr = new UpdateManager(new Velopack.Sources.GithubSource("https://github.com/TavstalDev/KonkordLauncher", null, false));
            
            if (!mgr.IsInstalled) 
            {
                _logger.Info("Running in portable/dev mode. Update check skipped.");
                return false;
            }
            
            // check for new version
            var newVersion = await mgr.CheckForUpdatesAsync();
            if (newVersion == null)
                return false; // no update available

            if (justCheck)
                return true;
            
            // download new version
            await mgr.DownloadUpdatesAsync(newVersion, cancelToken: cancellationToken);

            // install new version and restart app
            mgr.ApplyUpdatesAndRestart(newVersion);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while checking for updates");
            _logger.Error(ex);
            return false;
        }
    }
    
    #region IProgressReporter Implementation
    
    /// <summary>
    /// Sets the progress value for the startup process.
    /// </summary>
    /// <param name="progress">The progress value, typically between 0.0 and 1.0.</param>
    public void ReportProgress(double progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.Progress = progress;
        });
    }

    /// <summary>
    /// Sets the status message for the startup process.
    /// </summary>
    /// <param name="status">The status message to display.</param>
    public void UpdateStatus(string status)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.ProgressText = status;
        });
    }

    /// <summary>
    /// Sets the status message using a translation key and optional arguments.
    /// </summary>
    /// <param name="key">The translation key for the status message.</param>
    /// <param name="args">Optional arguments for formatting the status message.</param>
    public void UpdateStatusTranslated(string key, params object[]? args)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.ProgressText = TranslationManager.Translate(key, args);
        });
    }

    /// <summary>
    /// Opens or displays the progress reporter UI for this view model.
    /// </summary>
    public void OpenReporter() { /* unused */ } 
    
    /// <summary>
    /// Closes or hides the progress reporter UI for this view model.
    /// </summary>
    public void CloseReporter() { /* unused */ }

    #endregion
}