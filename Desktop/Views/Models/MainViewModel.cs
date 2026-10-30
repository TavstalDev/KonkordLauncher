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
///     Represents the main view model for the application, managing the state and behavior of the UI.
/// </summary>
public partial class MainViewModel : KonkordObservableObject
{
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IScreenService _screenService = null!;

    public MainViewModel()
    {
        Accounts = new MainViewModel_Accounts();
        About = new MainViewModel_About();
        Config = new MainViewModel_Config();
        Instances = new MainViewModel_Instances();
    }

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

    [ObservableProperty] public partial EAboutTab CurrentAboutTab { get; set; } = EAboutTab.ABOUT;

    [ObservableProperty] public partial ESettingsTab CurrentSettingsTab { get; set; } = ESettingsTab.LAUNCHER;

    [ObservableProperty] public partial bool HasPatches { get; set; }

    public bool IsLinux { get; } = OSHelper.GetOperatingSystem() == EOperatingSystem.LINUX;
    public DateTime NextCacheRefresh { get; private set; }
    public DateTime NextUpdate { get; private set; }
    public Task Initialization { get; } = null!;

    public MainViewModel_Accounts Accounts { get; } = null!;
    public MainViewModel_About About { get; } = null!;
    public MainViewModel_Config Config { get; } = null!;
    public MainViewModel_Instances Instances { get; } = null!;

    [ObservableProperty] public partial bool IsLoading { get; set; }

    [ObservableProperty] public partial ESidebarType CurrentPageIndex { get; set; } = ESidebarType.Play;

    public ObservableCollection<PatchNote> Patches { get; } = [];

    public int ScreenWidth => Design.IsDesignMode ? 950 : _screenService.ScreenSize.Width;
    public int ScreenHeight => Design.IsDesignMode ? 600 : _screenService.ScreenSize.Height;

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

    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();
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
