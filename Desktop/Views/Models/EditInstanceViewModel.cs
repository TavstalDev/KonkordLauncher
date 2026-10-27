using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tavstal.BeaconSDK.Enums;
using Tavstal.BeaconSDK.Helpers.Platform;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Services;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;
using Tavstal.KonkordLauncher.Desktop.Views.Models.EditInstance;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

/// <summary>
///     Represents the view model for editing a Minecraft instance.
///     Provides properties and methods for managing mods, resource packs, shader packs, worlds, servers, and screenshots.
/// </summary>
public partial class EditInstanceViewModel : KonkordObservableObject
{
    private const int MaxLogLines = 1000;
    public readonly InstanceModel Instance = null!;
    private readonly IEncryptionService _encryptionService = null!;
    private readonly IEventService _eventService = null!;
    private readonly IFilePickerService _filePickerService = null!;
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IBeaconLogger _logger = null!;
    private readonly IScreenService _screenService = null!;

    public EditInstanceViewModel()
    {
        Mods = new EditInstanceViewModel_Mods();
        ResourcePacks = new EditInstanceViewModel_ResourcePacks();
        Screenshots = new EditInstanceViewModel_Screenshots();
        Servers = new EditInstanceViewModel_Servers();
        Settings = new EditInstanceViewModel_Settings();
        ShaderPacks = new EditInstanceViewModel_ShaderPacks();
        Worlds = new EditInstanceViewModel_Worlds();
    }

    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public EditInstanceViewModel(InstanceModel instance, IBeaconLogger<EditInstanceViewModel> logger,
        ILauncherStore launcherStore, IEventService eventService, IBitmapService bitmapService,
        ITranslationService translationService, IEncryptionService encryptionService,
        IScreenService screenService, IDialogService dialogService, IFilePickerService filePickerService)
    {
        Instance = instance;
        _logger = logger;
        _launcherStore = launcherStore;
        _eventService = eventService;
        _encryptionService = encryptionService;
        _screenService = screenService;
        _filePickerService = filePickerService;

        Mods = new EditInstanceViewModel_Mods(instance, () => GameDirectory, launcherStore, bitmapService,
            dialogService);
        Mods.OnResourcesUpdated += OnResourcesUpdated;
        ResourcePacks = new EditInstanceViewModel_ResourcePacks(instance, () => GameDirectory,
            launcherStore, bitmapService, dialogService);
        ResourcePacks.OnResourcesUpdated += OnResourcesUpdated;
        Screenshots = new EditInstanceViewModel_Screenshots(() => GameDirectory, bitmapService, filePickerService);
        Servers = new EditInstanceViewModel_Servers(() => GameDirectory, bitmapService);
        Settings = new EditInstanceViewModel_Settings(instance.Id, launcherStore, translationService, eventService,
            filePickerService, dialogService);
        ShaderPacks = new EditInstanceViewModel_ShaderPacks(instance, () => GameDirectory, launcherStore, bitmapService,
            dialogService);
        ShaderPacks.OnResourcesUpdated += OnResourcesUpdated;
        Worlds = new EditInstanceViewModel_Worlds(() => GameDirectory, bitmapService, filePickerService);

        if (Design.IsDesignMode)
            return;

        _ = InitAsync().ContinueWith(t =>
        {
            if (t.IsFaulted)
                _logger.LogError(t.Exception, "Error initializing EditInstanceViewModel");
        });
    }

    public EditInstanceViewModel_Mods Mods { get; }
    public EditInstanceViewModel_ResourcePacks ResourcePacks { get; }
    public EditInstanceViewModel_Screenshots Screenshots { get; }
    public EditInstanceViewModel_Servers Servers { get; }
    public EditInstanceViewModel_Settings Settings { get; }
    public EditInstanceViewModel_ShaderPacks ShaderPacks { get; }
    public EditInstanceViewModel_Worlds Worlds { get; }

    public bool IsLinux => OSHelper.GetOperatingSystem() == EOperatingSystem.LINUX;
    public int ScreenWidth => Design.IsDesignMode ? 950 : _screenService.ScreenSize.Width;
    public int ScreenHeight => Design.IsDesignMode ? 600 : _screenService.ScreenSize.Height;
    public List<AccountModel> Accounts { get; private set; } = null!;

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _logger.LogDebug("Freeing memory in EditInstanceViewModel...");
        _eventService.OnInstanceLogged -= OnInstanceLogged;

        Accounts.Clear();
        Mods.OnResourcesUpdated -= OnResourcesUpdated;
        ResourcePacks.OnResourcesUpdated -= OnResourcesUpdated;
        ShaderPacks.OnResourcesUpdated -= OnResourcesUpdated;

        InstanceName = string.Empty;
        GameDirectory = null;
        Logs.Clear();
        Mods.Dispose();
        ResourcePacks.Dispose();
        Screenshots.Dispose();
        Servers.Dispose();
        Settings.Dispose();
        Worlds.Dispose();
    }

    private void OnInstanceLogged(string instanceId, string logMessage)
    {
        if (instanceId != Instance.Id)
            return;

        Logs.Add(logMessage);
        if (Logs.Count > MaxLogLines)
            Logs.RemoveAt(0);

        if (Logs.Count % 10 == 0)
            Dispatcher.UIThread.Invoke(async () => await LogsScrollToEnd.Handle(Unit.Default));
    }

    private async Task InitAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        var accountData = await _launcherStore.GetAccountDataAsync(cancellationToken);

        InstanceName = Instance.Name;
        IsVanilla = Instance.Kind == EMinecraftKind.VANILLA;
        GameDirectory = Instance.GameDirectory;

        Accounts = [];
        foreach (var account in accountData.Accounts)
            Accounts.Add(new AccountModel(account, account.Id == accountData.SelectedAccountId, _encryptionService));


        // Logging setup
        _eventService.OnInstanceLogged += OnInstanceLogged;
        var logs = _eventService.GetInstanceLogs(Instance.Id);
        if (!string.IsNullOrEmpty(logs))
            foreach (var line in logs.Split(Environment.NewLine))
                Logs.Add(line);

        // TODO: Performance issue - initializing all tabs together can make the edit window open slowly.
        await Task.WhenAll(
            Mods.InitAsync(cancellationToken),
            ResourcePacks.InitAsync(cancellationToken),
            Screenshots.InitAsync(cancellationToken),
            Servers.InitAsync(cancellationToken),
            ShaderPacks.InitAsync(cancellationToken),
            Worlds.InitAsync(cancellationToken)
        );

        await Settings.InitAsync(Instance.ConfigModel, cancellationToken);
    }

    private void OnResourcesUpdated(object? sender, EResourceType type)
    {
        Dispatcher.UIThread.Invoke(async () =>
        {
            switch (type)
            {
                case EResourceType.RESOURCE_PACK:
                {
                    await ResourcePacks.RefreshResourcePacksAsync();
                    break;
                }
                case EResourceType.MOD:
                {
                    await Mods.RefreshModsAsync();
                    break;
                }
                case EResourceType.SHADER_PACK:
                {
                    await ShaderPacks.RefreshShaderPacksAsync();
                    break;
                }
            }
        });
    }

    #region Interactions

    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();
    public Interaction<EEditInstanceTab, Unit> TabSwitchInteraction { get; } = new();
    public Interaction<EInstanceSettingsTab, Unit> SettingsTabSwitchInteraction { get; } = new();
    public Interaction<Unit, Unit> BeginWorldRename { get; } = new();
    public Interaction<Unit, Unit> BeginScreenshotRename { get; } = new();
    public Interaction<Unit, Unit> LogsScrollToEnd { get; } = new();

    #endregion

    #region Observable Properties

    [ObservableProperty] public partial EEditInstanceTab EditInstanceTab { get; set; }

    [ObservableProperty] public partial EInstanceSettingsTab InstanceSettingsTab { get; set; }

    [ObservableProperty] public partial string InstanceName { get; set; } = string.Empty;

    [ObservableProperty] public partial string? GameDirectory { get; set; }

    [ObservableProperty] public partial bool IsVanilla { get; set; }

    public ObservableCollection<string> Logs { get; set; } = [];

    #endregion

    #region Commands

    #region Common

    [RelayCommand]
    private async Task CloseWindow()
    {
        await CloseWindowInteraction.Handle(Unit.Default);
    }

    [RelayCommand]
    private async Task SwitchTab(EEditInstanceTab tab)
    {
        await TabSwitchInteraction.Handle(tab);
    }

    #endregion

    #region Logs

    [RelayCommand]
    private async Task ScrollLogsToEnd()
    {
        await LogsScrollToEnd.Handle(Unit.Default);
    }

    [RelayCommand]
    private async Task CopyLogs()
    {
        await _filePickerService.SetClipboardTextAsync(string.Join(Environment.NewLine, Logs));
    }

    [RelayCommand]
    private void ClearLogs()
    {
        Logs.Clear();
        _eventService.CleareInstanceLogs(Instance.Id);
    }

    #endregion

    #region Screenshots

    [RelayCommand]
    private async Task RenameScreenshot(ScreenshotModel screenshot)
    {
        await BeginScreenshotRename.Handle(Unit.Default);
    }

    #endregion

    #region Worlds

    [RelayCommand]
    private async Task RenameWorld(WorldModel world)
    {
        await BeginWorldRename.Handle(Unit.Default);
    }

    #endregion

    #region Settings

    [RelayCommand]
    private async Task SwitchSettingsTab(EInstanceSettingsTab tab)
    {
        await SettingsTabSwitchInteraction.Handle(tab);
    }

    #endregion

    #endregion
}
