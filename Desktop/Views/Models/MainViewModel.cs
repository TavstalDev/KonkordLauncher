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
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Views.Models.MainView;
using JavaVersionModel = Tavstal.KonkordLauncher.Desktop.Models.Domain.JavaVersionModel;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

/// <summary>
/// Represents the main view model for the application, managing the state and behavior of the UI.
/// </summary>
public partial class MainViewModel : KonkordObservableObject
{
    private readonly IBeaconLogger _logger = null!;
    private readonly ILauncherStore _launcherStore = null!;
    private readonly ITranslationService _translationService = null!;
    private readonly IMicrosoftAuthService _authService = null!;
    private readonly IMojangSkinService _mojangSkinService = null!;
    private readonly IBitmapService _bitmapService = null!;
    private readonly IEventService _eventService = null!;
    private readonly IEncryptionService _encryptionService = null!;

    public bool IsLinux { get; } = OSHelper.GetOperatingSystem() == EOperatingSystem.LINUX;
    public DateTime NextCacheRefresh { get; private set; }
    public DateTime NextUpdate { get; private set; }
    public Task Initialization { get; } = null!;

    public MainViewModel_Accounts Accounts { get; }
    public MainViewModel_About About { get; }
    public MainViewModel_Config Config { get; }
    public MainViewModel_Instances Instances { get; }

    #region Interactions


    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();
    public Interaction<Alert, bool> ShowAlertDialogInteraction { get; } = new();
    public Interaction<Alert, bool> ShowConfirmDialogInteraction { get; } = new();
    public Interaction<ESidebarType, Unit> SwitchSidebarBtnInteraction { get; } = new();
    public Interaction<Unit, string?> OpenFolderPickerInteraction { get; } = new();
    public Interaction<Unit, string?> OpenImagePickerInteraction { get; } = new();
    public Interaction<Unit, Unit> ShowInstanceCreationDialogInteraction { get; } = new();
    public Interaction<InstanceModel, Unit> ShowInstanceEditDialogInteraction { get; } = new();
    public Interaction<Unit, Unit> ShowAccountsDialogInteraction { get; } = new();
    public Interaction<Unit, JavaVersionModel> ShowJavaSelectorDialogInteraction { get; } = new();
    public Interaction<string, Unit> ShowLogsWindowInteraction { get; } = new();
    public Interaction<string, Unit> CloseLogsWindowInteraction { get; } = new();
    public Interaction<string, string?> ShowTextInputDialogInteraction { get; } = new();
    public Interaction<Unit, string?> ShowIconSelectorDialogInteraction { get; } = new();
    public Interaction<ESettingsTab, Unit> UpdateSettingsTabButtonInteraction { get; } = new();
    public Interaction<EAboutTab, Unit> SwitchAboutTabInteractionInteraction { get; } = new();
    public Interaction<Instance, Unit> ExportModrinthInstanceInteraction { get; } = new();
    public Interaction<Instance, Unit> ExportCurseForgeInstanceInteraction { get; } = new();
    public Interaction<Instance, bool> ShowInstanceVersionSelectorInteraction { get; } = new();
    #endregion

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial ESidebarType CurrentPageIndex { get; set; } = ESidebarType.Play;

    [ObservableProperty] private ESettingsTab _currentSettingsTab = ESettingsTab.LAUNCHER;
    [ObservableProperty] private EAboutTab _currentAboutTab = EAboutTab.ABOUT;

    public ObservableCollection<PatchNote> Patches { get; } = new();
    [ObservableProperty] private bool _hasPatches;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    public MainViewModel()
    {
        Accounts = new MainViewModel_Accounts(this, _launcherStore, _translationService, _authService,
            _mojangSkinService, _bitmapService, _eventService, _encryptionService);
        About = new MainViewModel_About(this);
        Config = new MainViewModel_Config(this, _launcherStore, _eventService);
        Instances = new MainViewModel_Instances(this, _launcherStore, _translationService, _eventService);

        if (Design.IsDesignMode)
            return;

        Initialization = InitAsync();
        Initialization.ContinueWith(task =>
        {
            if (task.IsFaulted)
                _logger.LogError("Initialization failed: " + task.Exception);
            else
                _logger.LogDebug("Initialization completed successfully.");
        }, TaskScheduler.Default);
    }

    public MainViewModel(IBeaconLogger<MainViewModel> logger, ILauncherStore launcherStore,
        ITranslationService translationService, IMicrosoftAuthService microsoftAuthService,
        IMojangSkinService mojangSkinService, IBitmapService bitmapService,
        IEventService eventService, IEncryptionService encryptionService) : this()
    {
        _logger = logger;
        _launcherStore = launcherStore;
        _translationService = translationService;
        _authService = microsoftAuthService;
        _mojangSkinService = mojangSkinService;
        _bitmapService = bitmapService;
        _eventService = eventService;
        _encryptionService = encryptionService;
    }

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

    /// <summary>
    /// Releases the resources used by the MainViewModel and unsubscribes from global events.
    /// </summary>
    /// <param name="disposing">
    /// A boolean value indicating whether the method is being called directly or indirectly by a finalizer.
    /// If true, the method has been called directly or indirectly by a user's code. Managed and unmanaged resources can be disposed.
    /// If false, the method has been called by the runtime from inside the finalizer, and only unmanaged resources can be disposed.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Initialization.Dispose();
        Accounts.Dispose();
        About.Dispose();
        Config.Dispose();
        Instances.Dispose();
    }


    #region Commands

    [RelayCommand]
    public async Task CloseWindow()
    {
        await CloseWindowInteraction.Handle(Unit.Default);
    }

    /// <summary>
    /// Handles the sidebar button click event by changing the current sidebar view.
    /// </summary>
    /// <param name="sidebarType">The type of sidebar to switch to.</param>
    [RelayCommand]
    public async Task HandleSidebarBtn(ESidebarType sidebarType)
    {
        await SwitchSidebarBtnInteraction.Handle(sidebarType);
    }

    [RelayCommand]
    private async Task SwitchAboutTab(EAboutTab tab)
    {
        await SwitchAboutTabInteractionInteraction.Handle(tab);
    }

    #endregion
}
