using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tavstal.BeaconSDK.Enums;
using Tavstal.BeaconSDK.Helpers.Platform;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.BeaconSDK.Services.Abstractions.Auth;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Services;
using Tavstal.KonkordLauncher.Desktop.Services.Instances;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;
using Tavstal.KonkordLauncher.Desktop.Views.Models.MainView;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

/// <summary>
///     The main view model of the launcher window. It groups the view models for each page.
/// </summary>
public partial class MainViewModel : KonkordObservableObject
{
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IScreenService _screenService = null!;

    /// <summary>
    ///     Creates a view model for the XAML designer.
    /// </summary>
    public MainViewModel()
    {
        Accounts = new MainViewModel_Accounts();
        About = new MainViewModel_About();
        Config = new MainViewModel_Config();
        Instances = new MainViewModel_Instances();
    }

    /// <summary>
    ///     Creates the main view model and starts loading the launcher data.
    /// </summary>
    /// <param name="logger">The logger for the view model.</param>
    /// <param name="launcherStore">The store that reads and saves launcher data.</param>
    /// <param name="translationService">The service that translates text.</param>
    /// <param name="microsoftAuthService">The Microsoft sign-in service.</param>
    /// <param name="mojangSkinService">The service that manages Mojang skins.</param>
    /// <param name="bitmapService">The service that loads bitmaps.</param>
    /// <param name="eventService">The service that raises launcher events.</param>
    /// <param name="encryptionService">The service that encrypts and decrypts data.</param>
    /// <param name="instanceModelFactory">The factory that creates instance view models.</param>
    /// <param name="screenService">The service that gives the screen size.</param>
    /// <param name="dialogService">The service that shows dialogs.</param>
    /// <param name="filePickerService">The service that opens file and folder pickers.</param>
    public MainViewModel(IBeaconLogger<MainViewModel> logger, ILauncherStore launcherStore,
        ITranslationService translationService, IMicrosoftAuthService microsoftAuthService,
        IMojangSkinService mojangSkinService, IBitmapService bitmapService,
        IEventService eventService, IEncryptionService encryptionService,
        IInstanceModelFactory instanceModelFactory, IScreenService screenService,
        IDialogService dialogService, IFilePickerService filePickerService)
    {
        if (Design.IsDesignMode)
            return;

        _launcherStore = launcherStore;
        _screenService = screenService;

        Accounts = new MainViewModel_Accounts(launcherStore, translationService, microsoftAuthService,
            mojangSkinService,
            bitmapService, eventService, encryptionService, dialogService, filePickerService);
        About = new MainViewModel_About();
        Config = new MainViewModel_Config(_launcherStore, eventService, filePickerService, dialogService,
            translationService);
        Instances = new MainViewModel_Instances(_launcherStore, translationService, eventService,
            instanceModelFactory, filePickerService, dialogService);

        Initialization = InitAsync();
        Initialization.ContinueWith(task =>
        {
            if (task.IsFaulted)
                logger.LogError("Initialization failed: " + task.Exception);
            else
                logger.LogDebug("Initialization completed successfully.");
        }, TaskScheduler.Default);
    }

    /// <summary>
    ///     Gets or sets the selected tab on the "About" page.
    /// </summary>
    [ObservableProperty] public partial EAboutTab CurrentAboutTab { get; set; } = EAboutTab.ABOUT;

    /// <summary>
    ///     Gets or sets the selected tab on the settings page.
    /// </summary>
    [ObservableProperty] public partial ESettingsTab CurrentSettingsTab { get; set; } = ESettingsTab.LAUNCHER;

    /// <summary>
    ///     Gets or sets a value that indicates whether patch notes are available.
    /// </summary>
    [ObservableProperty] public partial bool HasPatches { get; set; }

    /// <summary>
    ///     Gets a value that indicates whether the launcher runs on Linux.
    /// </summary>
    public bool IsLinux { get; } = OSHelper.GetOperatingSystem() == EOperatingSystem.LINUX;

    /// <summary>
    ///     Gets the time of the next cache refresh.
    /// </summary>
    public DateTime NextCacheRefresh { get; private set; }

    /// <summary>
    ///     Gets the time of the next update check.
    /// </summary>
    public DateTime NextUpdate { get; private set; }

    /// <summary>
    ///     Gets the task that represents the startup work of the view model.
    /// </summary>
    public Task Initialization { get; } = null!;

    /// <summary>
    ///     Gets the view model for the accounts page.
    /// </summary>
    public MainViewModel_Accounts Accounts { get; } = null!;

    /// <summary>
    ///     Gets the view model for the "About" page.
    /// </summary>
    public MainViewModel_About About { get; } = null!;

    /// <summary>
    ///     Gets the view model for the settings page.
    /// </summary>
    public MainViewModel_Config Config { get; } = null!;

    /// <summary>
    ///     Gets the view model for the instances page.
    /// </summary>
    public MainViewModel_Instances Instances { get; } = null!;

    /// <summary>
    ///     Gets or sets a value that indicates whether the launcher is loading.
    /// </summary>
    [ObservableProperty] public partial bool IsLoading { get; set; }

    /// <summary>
    ///     Gets or sets the page that is shown in the sidebar.
    /// </summary>
    [ObservableProperty] public partial ESidebarType CurrentPageIndex { get; set; } = ESidebarType.Play;

    /// <summary>
    ///     Gets the patch notes that are shown to the user.
    /// </summary>
    public ObservableCollection<PatchNote> Patches { get; } = [];

    /// <summary>
    ///     Gets the screen width in pixels.
    /// </summary>
    public int ScreenWidth => Design.IsDesignMode ? 950 : _screenService.ScreenSize.Width;

    /// <summary>
    ///     Gets the screen height in pixels.
    /// </summary>
    public int ScreenHeight => Design.IsDesignMode ? 600 : _screenService.ScreenSize.Height;

    /// <summary>
    ///     Loads the launcher settings, accounts, instances, patch notes and about information.
    /// </summary>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A task that represents the operation.</returns>
    public async Task InitAsync(CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        try
        {
            var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);
            NextUpdate = settings.Launcher.NextUpdateCheck;
            NextCacheRefresh = settings.CacheRefreshDate;
            var accountData = await _launcherStore.GetAccountDataAsync(cancellationToken);

            await Config.InitAsync(settings);
            await Accounts.InitAsync(accountData);
            await Instances.InitAsync(cancellationToken);

            #region Patches

            var patches =
                await _launcherStore.GetPatchNotesAsync(settings.Launcher.CacheDirectoryPath, cancellationToken);
            foreach (var patch in patches)
                Patches.Add(new PatchNote(patch.Title, patch.Content, patch.Url));

            HasPatches = Patches.Count > 0;

            #endregion

            await About.InitAsync(cancellationToken);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Initialization.Dispose();
        Accounts.Dispose();
        About.Dispose();
        Config.Dispose();
        Instances.Dispose();
    }

    #region Interactions

    /// <summary>
    ///     Gets the interaction that asks the window to close.
    /// </summary>
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();

    /// <summary>
    ///     Gets the interaction that switches the page in the sidebar.
    /// </summary>
    public Interaction<ESidebarType, Unit> SwitchSidebarBtnInteraction { get; } = new();

    #endregion

    #region Commands

    [RelayCommand]
    private async Task CloseWindow()
    {
        await CloseWindowInteraction.Handle(Unit.Default);
    }

    [RelayCommand]
    private async Task HandleSidebarBtn(ESidebarType sidebarType)
    {
        await SwitchSidebarBtnInteraction.Handle(sidebarType);
    }

    #endregion
}
