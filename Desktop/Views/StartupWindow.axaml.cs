using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Reactive;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Primitives;
using Tavstal.BeaconSDK.Models;
using Tavstal.BeaconSDK.Models.Accounts;
using Tavstal.BeaconSDK.Models.Config;
using Tavstal.BeaconSDK.Models.Instance;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs.Config;
using Tavstal.KonkordLauncher.Desktop.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Translation;
using Tavstal.KonkordLauncher.Desktop.Views.Models;
using Velopack;

namespace Tavstal.KonkordLauncher.Desktop.Views;

public partial class StartupWindow : KonkordWindow<StartupViewModel>, IKonkordProgress
{
    private readonly IBeaconLogger _logger = null!;
    private readonly IHttpClientFactory _httpClientFactory = null!;
    private readonly ITranslationService _translationService = null!;
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IValidationService _validationService = null!;
    private readonly IJavaService _javaService = null!;
    private readonly ISkinService _skinService = null!;
    private readonly IEventService _eventService = null!;
    private readonly BeaconConfiguration _beaconConfiguration = null!;
    private const int _stepDelay = 100;

    [RequiresUnreferencedCode("May be affected by trimming due to use of reflection for service resolution.")]
    public StartupWindow()
    {
        InitializeComponent();

        DataContext = new StartupViewModel();
        this.WhenActivated(disposables =>
        {
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
        });
    }

    [RequiresUnreferencedCode("May be affected by trimming due to use of reflection for service resolution.")]
    public StartupWindow(IBeaconLogger<StartupWindow> logger, IHttpClientFactory httpClientFactory,
        ITranslationService translationService, ILauncherStore launcherStore,
        IValidationService validationService, IJavaService javaService, ISkinService skinService,
        IEventService eventService, BeaconConfiguration beaconConfiguration) : this()
    {
        if (Design.IsDesignMode)
            return;

        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _translationService = translationService;
        _launcherStore = launcherStore;
        _validationService = validationService;
        _javaService = javaService;
        _skinService = skinService;
        _eventService = eventService;
        _beaconConfiguration = beaconConfiguration;
    }

    #region Events

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        var screen = Screens.ScreenFromWindow(this);
        Resolution? resolution = null;
        if (screen != null)
            resolution = new Resolution { X = (uint)screen.Bounds.Width, Y = (uint)screen.WorkingArea.Height };
        Task.Run(async () => {
            try
            {
                var settings = await _launcherStore.GetSettingsAsync(resolution);
                _eventService.RaiseThemeChanged(settings.Launcher.Theme);
                await Task.Yield();
                await InitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during startup initialization:");
            }
        });
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        // Ensure the progress bar is not indeterminate when closing
        // it may use more resources than necessary otherwise
        ProgressBar.IsIndeterminate = false;
    }

    #endregion

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

        // Fill java installation cache
        await _javaService.LocateJavaInstallationsAsync(settings.Launcher.JavaDirectoryPath, cancellationToken: cancellationToken);

        // Refresh GitHub Cache & Skins Cache
        bool shouldRefreshCache = await ValidateCachesAsync(settings, cancellationToken);

        // Check for Updates
        if (settings.Launcher.EnableAutomaticUpdates && DateTime.Now > settings.Launcher.NextUpdateCheck)
        {
            UpdateStatusTranslated("startup.progress.checking");
            await Task.Delay(_stepDelay, cancellationToken);

            settings.Launcher.NextUpdateCheck = DateTime.Now.AddHours(Math.Clamp(settings.Launcher.UpdateInterval, 1, int.MaxValue));
            await _launcherStore.SaveSettingsAsync(settings, cancellationToken);

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
            await _launcherStore.SaveSettingsAsync(settings, cancellationToken);
        }

        // Start Main Window
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            _logger.LogError("Failed to start main window: Application lifetime is not IClassicDesktopStyleApplicationLifetime");
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Hide();
            var mainWindow = new MainWindow
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            desktop.MainWindow = mainWindow;
            mainWindow.Show();
            Close();
        }, DispatcherPriority.Background, cancellationToken);
    }

    private async Task<bool> ValidateCachesAsync(CoreConfig settings, CancellationToken cancellationToken = default)
    {
        // Refresh github cache
        UpdateStatusTranslated("startup.validation.github");
        bool shouldRefreshCache = settings.CacheRefreshDate < DateTime.Now;
        string githubCachePath = Path.Combine(settings.Launcher.CacheDirectoryPath, "github_cache.json");
        if (!File.Exists(githubCachePath) || shouldRefreshCache)
        {
            using var client = _httpClientFactory.CreateClient();
            string? response = await client.GetStringAsync("https://api.github.com/repos/TavstalDev/KonkordLauncher/releases", cancellationToken);
            if (string.IsNullOrWhiteSpace(response))
            {
                _logger.LogError("Failed to fetch GitHub cache data");
                response = "[]";
            }
            await File.WriteAllTextAsync(githubCachePath, response, cancellationToken);
        }

        // Refresh skins cache
        UpdateStatusTranslated("startup.validation.skins");
        AccountData accountData = await _launcherStore.GetAccountDataAsync(cancellationToken);
        var semaphore = new SemaphoreSlim(_beaconConfiguration.Http.MaxParallelDownloads);
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

    private void UpdateStatusTranslated(string key, params object[]? args)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.ProgressText =  _translationService.Translate(key, args);
        });
    }

    public void Report(double progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.Progress = progress;
        });
    }

    public void Report(double progress, string fileName, bool downloading = true)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.Progress = progress;
            // TODO
        });
    }

    public void Report(EProgressKind kind, double progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.Progress = progress;
            DataContext.ProgressText = _translationService.Translate(TranslationHelper.GetProgressKey(kind), progress.ToString("0.00"));
        });
    }

    public void Report(double progress, string key, params object[]? args)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.Progress = progress;
            DataContext.ProgressText = _translationService.Translate(key, args);
        });
    }

    public void ReportExpectedTask(int? count) { /* ignored */ }

    public void ReportTaskCompleted() { /* ignored */ }

    #endregion
}
