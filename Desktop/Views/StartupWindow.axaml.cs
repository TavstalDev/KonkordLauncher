using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Platform;
using Tavstal.KonkordLauncher.Core.Helpers.Serialization;
using Tavstal.KonkordLauncher.Core.Helpers.Utils;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Accounts;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;
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
    private readonly ICustomLogger _logger;
    private readonly IHttpService _httpService;
    private readonly ITranslationService _translationService;
    private readonly ILauncherStore _launcherStore;
    private readonly IValidationService _validationService;
    private readonly IJavaService _javaService;
    private readonly ISkinService _skinService;
    private const int _stepDelay = 100;
    private const int _maxParallelDownloads = 16;
    private readonly int[] _javaVersionsToDownload = [8, 16, 17, 21, 25];

    /// <summary>
    /// Initializes a new instance of the <see cref="StartupWindow"/> class with default settings.
    /// </summary>
    public StartupWindow()
    {
        var services = Program.ServiceProvider;
        _logger = services.GetRequiredService<ICustomLogger<StartupWindow>>();
        _httpService = services.GetRequiredService<IHttpService>();
        _translationService = services.GetRequiredService<ITranslationService>();
        _launcherStore = services.GetRequiredService<ILauncherStore>();
        _validationService = services.GetRequiredService<IValidationService>();
        _javaService = services.GetRequiredService<IJavaService>();
        _skinService = services.GetRequiredService<ISkinService>();
        
        InitializeComponent();
        
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
                _logger.LogError(ex, "An error occurred during startup initialization:");
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
    
    #endregion

    /// <summary>
    /// Performs the full startup initialization sequence for the launcher.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel startup work if initialization is aborted.</param>
    private async Task InitAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);

        // Set initial status
        UpdateStatusTranslated("startup.progress.initializing");

        // Validate Directory Structure
        UpdateStatusTranslated("startup.validation.dataFolder");
        await Task.Delay(_stepDelay, cancellationToken);
        bool shouldGenerateIcons = !Directory.Exists(settings.Launcher.IconsDirectoryPath);
        if (!await _validationService.ValidateLauncherDirectoryAsync(cancellationToken))
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
                _logger.LogDebug($"Extracting icon: {fileName}");
                var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
                if (stream == null)
                {
                    _logger.LogError($"Failed to get resource stream for {fileName}");
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

        // Validate Accounts
        UpdateStatusTranslated("startup.validation.accounts");
        await Task.Delay(_stepDelay, cancellationToken);
        if (!await _validationService.ValidateAccounts(cancellationToken))
        {
            UpdateStatusTranslated("startup.validation.accountsFailed");
            return;
        }

        // Validate Manifests
        UpdateStatusTranslated("startup.validation.manifests");
        await Task.Delay(_stepDelay, cancellationToken);
        if (!await _validationService.ValidateManifests(this, cancellationToken))
        {
            UpdateStatusTranslated("startup.validation.manifestsFailed");
            return;
        }

        // Validate Java
        await ValidateJavaAsync(settings, cancellationToken);

        // Refresh GitHub Cache & Skins Cache
        bool shouldRefreshCache = await ValidateCachesAsync(settings, cancellationToken);

        // Check for Updates
        if (settings.Launcher.EnableAutomaticUpdates && DateTime.Now > settings.Launcher.NextUpdateCheck)
        {
            UpdateStatusTranslated("startup.progress.checking");
            await Task.Delay(_stepDelay, cancellationToken);

            settings.Launcher.NextUpdateCheck = DateTime.Now.AddHours(MathHelper.Clamp(settings.Launcher.UpdateInterval, 1, int.MaxValue));
            await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherConfigPath, settings, cancellationToken);

            if (await CheckUpdateAsync(cancellationToken: cancellationToken))
            {
                _logger.LogDebug("Update found and applied, exiting startup to restart application...");
                return;
            }
            _logger.LogDebug("No updates found, starting application...");
        }

        // Update cache refresh time if needed
        if (shouldRefreshCache)
        {
            settings.CacheRefreshDate = DateTime.Now.AddDays(1);
            await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherConfigPath, settings, cancellationToken);
        }

        // Start Main Window
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            _logger.LogError("Failed to start main window: Application lifetime is not IClassicDesktopStyleApplicationLifetime");
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

    /// <summary>
    /// Ensures that the required Java runtimes are present in the launcher Java directory,
    /// downloads any missing versions, and updates executable permissions on non-Windows systems.
    /// </summary>
    /// <param name="settings">The launcher configuration containing the Java directory path.</param>
    /// <param name="cancellationToken">A token used to cancel Java download operations and related asynchronous work.</param>
    private async Task ValidateJavaAsync(CoreConfig settings, CancellationToken cancellationToken = default)
    {
        UpdateStatusTranslated("startup.validation.java");
        await Task.Delay(_stepDelay, cancellationToken);
        var javaInstallations = await _javaService.LocateJavaInstallationsAsync(settings.Launcher.JavaDirectoryPath, cancellationToken: cancellationToken);
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
            await _javaService.DownloadJavaVersionAsync(javaVersion, settings.Launcher.JavaDirectoryPath, progress, cancellationToken);
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
                        _translationService.Translate("startup.validation.java.exec.failedTitle"),
                        _translationService.Translate("startup.validation.java.exec.failedMessage",
                            javaExecutablePath),
                        EAlertType.Error
                    );
                    await window.ShowDialog(this);
                }
            }
        }

        await _javaService.LocateJavaInstallationsAsync(settings.Launcher.JavaDirectoryPath, true, cancellationToken);
    }

    /// <summary>
    /// Validates and refreshes local cache files used during startup, including GitHub release metadata
    /// and cached skin/cape assets for accounts.
    /// </summary>
    /// <param name="settings">The launcher configuration containing cache paths and refresh timing information.</param>
    /// <param name="cancellationToken">A token used to cancel network requests and background cache refresh operations.</param>
    /// <returns>
    /// A task that resolves to <c>true</c> if the GitHub cache was refreshed during this run;
    /// otherwise, <c>false</c>.
    /// </returns>
    private async Task<bool> ValidateCachesAsync(CoreConfig settings, CancellationToken cancellationToken = default)
    {
        // Refresh github cache
        UpdateStatusTranslated("startup.validation.github");
        bool shouldRefreshCache = settings.CacheRefreshDate < DateTime.Now;
        string githubCachePath = Path.Combine(settings.Launcher.CacheDirectoryPath, "github_cache.json");
        if (!File.Exists(githubCachePath) || shouldRefreshCache)
        {
            string? response = await _httpService.GetStringAsync(KonkordEndpoints.AllReleases, cancellationToken);
            if (response == null)
            {
                _logger.LogError("Failed to fetch GitHub cache data");
                response = "[]";
            }
            await File.WriteAllTextAsync(githubCachePath, response, cancellationToken);
        }

        // Refresh skins cache
        UpdateStatusTranslated("startup.validation.skins");
        AccountData accountData = await _launcherStore.GetAccountDataAsync(cancellationToken);
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
                        await _skinService.FetchSkinsAsync(settings.Launcher.CacheDirectoryPath, account.Id, account.Uuid,
                            skin, cancellationToken);
                    var capes = account.MojangProfile?.Capes ?? [];
                    if (capes.Count > 0)
                        await _skinService.FetchCapesAsync(settings.Launcher.CacheDirectoryPath, capes, cancellationToken);
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

    /// <summary>
    /// Checks for application updates using Velopack and, unless <paramref name="justCheck"/> is <c>true</c>,
    /// downloads and applies the update before restarting the application.
    /// </summary>
    /// <param name="justCheck">
    /// If <c>true</c>, the method only checks whether an update is available and returns <c>true</c> if one exists.
    /// If <c>false</c>, the update is downloaded and applied automatically.
    /// </param>
    /// <param name="cancellationToken">A token used to cancel the update download operation.</param>
    /// <returns>
    /// <c>true</c> if an update is available (and optionally applied); otherwise, <c>false</c>.
    /// Returns <c>false</c> when running in portable/dev mode, when no update is available, or when an error occurs.
    /// </returns>
    private async Task<bool> CheckUpdateAsync(bool justCheck = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var mgr = new UpdateManager(new Velopack.Sources.GithubSource("https://github.com/TavstalDev/KonkordLauncher", null, false));
            
            if (!mgr.IsInstalled) 
            {
                _logger.LogInformation("Running in portable/dev mode. Update check skipped.");
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
            _logger.LogCritical(ex, "Error while checking for updates");
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
            DataContext.ProgressText =  _translationService.Translate(key, args);
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