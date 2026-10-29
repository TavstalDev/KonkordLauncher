using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
using Tavstal.BeaconSDK.Models.Instance;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Services;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Services.Startup;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;
using Tavstal.KonkordLauncher.Desktop.Services.Update;
using Tavstal.KonkordLauncher.Desktop.Translation;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views;

public partial class StartupWindow : KonkordWindow<StartupViewModel>, IKonkordProgress
{
    private readonly IEventService _eventService = null!;
    private readonly IJavaService _javaService = null!;
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IBeaconLogger _logger = null!;
    private readonly IScreenService _screenService = null!;
    private readonly ITopLevelProvider _topLevelProvider = null!;
    private readonly ITranslationService _translationService = null!;
    private readonly IUpdateService _updateService = null!;
    private readonly IValidationService _validationService = null!;
    private readonly IWindowFactory _windowFactory = null!;

    public StartupWindow()
    {
        InitializeComponent();
    }

    [RequiresUnreferencedCode("May be affected by trimming due to use of reflection for service resolution.")]
    public StartupWindow(StartupViewModel viewModel, IBeaconLogger<StartupWindow> logger,
        ITranslationService translationService,
        ILauncherStore launcherStore, IValidationService validationService, IJavaService javaService,
        IEventService eventService, IScreenService screenService, IWindowFactory windowFactory,
        IUpdateService updateService, ITopLevelProvider topLevelProvider, IDialogService dialogService)
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;

        _logger = logger;
        _translationService = translationService;
        _launcherStore = launcherStore;
        _validationService = validationService;
        _javaService = javaService;
        _eventService = eventService;
        _screenService = screenService;
        _windowFactory = windowFactory;
        _updateService = updateService;
        _topLevelProvider = topLevelProvider;
        dialogService.SetMainWindow(this);
        DataContext = viewModel;

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

    #region Events

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (Design.IsDesignMode)
            return;

        var screen = Screens.ScreenFromWindow(this) ?? Screens.Primary;
        Resolution? resolution = null;
        if (screen != null)
        {
            _screenService.ScreenSize = new PixelSize(screen.Bounds.Width, screen.Bounds.Height);
            resolution = new Resolution { X = (uint)screen.Bounds.Width, Y = (uint)screen.WorkingArea.Height };
        }

        _topLevelProvider.SetActive(this);

        Task.Run(async () =>
        {
            try
            {
                var settings = await _launcherStore.GetSettingsAsync(resolution);
                _eventService.RaiseThemeChanged(settings.Launcher.Theme);
                await Task.Yield();
                await InitializeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during startup initialization:");
            }
        });
    }

    /// <inheritdoc />
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        // Ensure the progress bar is not indeterminate when closing
        // it may use more resources than necessary otherwise
        ProgressBar.IsIndeterminate = false;
        _topLevelProvider.ClearIfMatches(this);
    }

    private async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);

        // Set initial status
        UpdateStatusTranslated("startup.progress.initializing");

        // Validate Directory Structure
        UpdateStatusTranslated("startup.validation.dataFolder.status");
        var shouldGenerateIcons = !Directory.Exists(settings.Launcher.IconsDirectoryPath);
        if (!await _validationService.ValidateLauncherDirectoryAsync(cancellationToken))
        {
            UpdateStatusTranslated("startup.validation.dataFolder.failed");
            return;
        }

        // Generate icons if the directory does not exist
        if (shouldGenerateIcons)
        {
            var resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            foreach (var path in resourceNames)
            {
                if (!path.Contains("Desktop.Assets.Icons"))
                    continue;

                var fileName = path.Replace("Tavstal.KonkordLauncher.Desktop.Assets.Icons.", "");
                if (!fileName.EndsWith(".png"))
                    continue;
                _logger.LogDebug($"Extracting icon: {fileName}");
                await using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
                if (stream == null)
                {
                    _logger.LogError($"Failed to get resource stream for {fileName}");
                    continue;
                }

                var destPath = Path.Combine(settings.Launcher.IconsDirectoryPath, fileName);
                await using var outFile = new FileStream(destPath, FileMode.Create, FileAccess.Write);
                await stream.CopyToAsync(outFile, cancellationToken);
            }
        }

        // Validate Accounts
        UpdateStatusTranslated("startup.validation.accounts.status");
        if (!await _validationService.ValidateAccounts(cancellationToken))
        {
            UpdateStatusTranslated("startup.validation.accounts.failed");
            return;
        }

        // Validate Manifests
        UpdateStatusTranslated("startup.validation.manifests.status");
        if (!await _validationService.ValidateManifests(this, cancellationToken))
        {
            UpdateStatusTranslated("startup.validation.manifests.failed");
            return;
        }

        // Fill java installation cache
        await _javaService.LocateJavaInstallationsAsync(settings.Launcher.JavaDirectoryPath,
            cancellationToken: cancellationToken);

        // Refresh GitHub Cache & Skins Cache
        var shouldRefreshCache = await _validationService.ValidateCachesAsync(this, cancellationToken);

        // Check for Updates
        if (settings.Launcher.EnableAutomaticUpdates && DateTime.UtcNow > settings.Launcher.NextUpdateCheck)
        {
            UpdateStatusTranslated("startup.progress.checking");

            settings.Launcher.NextUpdateCheck =
                DateTime.UtcNow.AddHours(Math.Clamp(settings.Launcher.UpdateInterval, 1, int.MaxValue));
            await _launcherStore.SaveSettingsAsync(settings, cancellationToken);

            if (await _updateService.CheckAsync(cancellationToken))
                if (await _updateService.ApplyAsync(cancellationToken))
                {
                    _logger.LogDebug("Update found and applied, exiting startup to restart application...");
                    return;
                }

            _logger.LogDebug("No updates found, starting application...");
        }

        // Update cache refresh time if needed
        if (shouldRefreshCache)
        {
            settings.CacheRefreshDate = DateTime.UtcNow.AddDays(1);
            await _launcherStore.SaveSettingsAsync(settings, cancellationToken);
        }

        // Start Main Window
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            _logger.LogError(
                "Failed to start main window: Application lifetime is not IClassicDesktopStyleApplicationLifetime");
            return;
        }

        await Dispatcher.InvokeAsync(() =>
        {
            Hide();
            var mainWindow = _windowFactory.CreateMainWindow();
            mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            desktop.MainWindow = mainWindow;
            mainWindow.Show();
            Close();
        }, DispatcherPriority.Background, cancellationToken);
    }

    #endregion

    #region IProgressReporter Implementation

    /// <inheritdoc />
    public void Report(double progress)
    {
        Dispatcher.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.Progress = progress;
        });
    }

    /// <inheritdoc />
    public void Report(double progress, string fileName, bool downloading = true)
    {
        Dispatcher.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.Progress = progress;
            DataContext.ProgressText = _translationService.Translate(
                downloading ? "instance.downloading.file" : "instance.reading.file",
                fileName, progress.ToString("0.00"));
        });
    }

    /// <inheritdoc />
    public void Report(EProgressKind kind, double progress)
    {
        Dispatcher.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.Progress = progress;
            DataContext.ProgressText =
                _translationService.Translate(TranslationHelper.GetProgressKey(kind), progress.ToString("0.00"));
        });
    }

    /// <inheritdoc />
    public void Report(string key)
    {
        Dispatcher.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.ProgressText = _translationService.Translate(key);
        });
    }

    /// <inheritdoc />
    public void Report(double progress, string key, params object[]? args)
    {
        Dispatcher.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.Progress = progress;
            DataContext.ProgressText = _translationService.Translate(key, args);
        });
    }

    /// <inheritdoc />
    public void ReportExpectedTask(int? count)
    {
        /* ignored */
    }

    /// <inheritdoc />
    public void ReportTaskCompleted()
    {
        /* ignored */
    }

    private void UpdateStatusTranslated(string key, params object[]? args)
    {
        Dispatcher.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.ProgressText = _translationService.Translate(key, args);
        });
    }

    #endregion
}
