using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.Platform;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Views.Models.EditInstance;
using JavaVersionModel = Tavstal.KonkordLauncher.Desktop.Models.Domain.JavaVersionModel;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

/// <summary>
/// Represents the view model for editing a Minecraft instance. 
/// Provides properties and methods for managing mods, resource packs, shader packs, worlds, servers, and screenshots.
/// </summary>
public partial class EditInstanceViewModel : KonkordObservableObject
{
    public readonly string InstanceId;
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(EditInstanceViewModel));
    public bool IsClosing;
    public bool IsInitialized;
    public EditInstanceViewModel_Mods Mods { get; private set; }
    public EditInstanceViewModel_ResourcePacks ResourcePacks { get; private set; }
    public EditInstanceViewModel_Screenshots Screenshots { get; private set; }
    public EditInstanceViewModel_Servers Servers { get; private set; }
    public EditInstanceViewModel_Settings Settings { get; private set; }
    public EditInstanceViewModel_ShaderPacks ShaderPacks { get; private set; }
    public EditInstanceViewModel_Worlds Worlds { get; private set; }

    public bool IsLinux => OSHelper.GetOperatingSystem() == EOperatingSystem.Linux;
    public List<Account> Accounts { get; private set; }

    #region Interactions

    public Interaction<Unit, Unit> MinimizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> MaximizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();
    public Interaction<EEditInstanceTab, Unit> TabSwitchInteraction { get; } = new();
    public Interaction<EInstanceSettingsTab, Unit> SettingsTabSwitchInteraction { get; } = new();
    public Interaction<Alert, Unit> ShowAlertDialog { get; } = new();
    public Interaction<Unit, JavaVersionModel?> ShowJavaPathSelector { get; } = new();
    public Interaction<string, string?> ShowDirPickerInteraction { get; } = new();
    public Interaction<string, Unit> SetClipboardText { get; } = new();
    public Interaction<ScreenshotModel, Unit> SetClipboardImage { get; } = new();
    public Interaction<Unit, Unit> BeginWorldRename { get; } = new();
    public Interaction<Unit, Unit> BeginScreenshotRename { get; } = new();
    public Interaction<Unit, Unit> LogsScrollToEnd { get; } = new();

    #endregion

    #region Observable Properties

    [ObservableProperty] private EEditInstanceTab _editInstanceTab;
    [ObservableProperty] private EInstanceSettingsTab _instanceSettingsTab;
    [ObservableProperty] private string _instanceName;
    [ObservableProperty] private string? _gameDirectory;
    [ObservableProperty] private bool _isVanilla;
    [ObservableProperty] private string _logs;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="EditInstanceViewModel"/> class.
    /// </summary>
    /// <param name="instanceId">The unique identifier of the instance to edit.</param>
    public EditInstanceViewModel(string instanceId)
    {
        if (Design.IsDesignMode)
            return;

        InstanceId = instanceId;
        Mods = new EditInstanceViewModel_Mods(this);
        ResourcePacks = new EditInstanceViewModel_ResourcePacks(this);
        Screenshots = new EditInstanceViewModel_Screenshots(this);
        Servers = new EditInstanceViewModel_Servers(this);
        Settings = new EditInstanceViewModel_Settings(this);
        ShaderPacks = new EditInstanceViewModel_ShaderPacks(this);
        Worlds = new EditInstanceViewModel_Worlds(this);
        Dispatcher.UIThread.Invoke(async () => await InitAsync());
    }

    /// <summary>
    /// Handles log messages for a specific instance by updating the Logs property
    /// and triggering the LogsScrollToEnd interaction to scroll to the end of the logs.
    /// </summary>
    /// <param name="instanceId">The ID of the instance that generated the log message.</param>
    /// <param name="logMessage">The log message to be handled.</param>
    private void OnInstanceLogged(string instanceId, string logMessage)
    {
        if (instanceId != InstanceId)
            return;

        Logs += logMessage;
        Dispatcher.UIThread.Invoke(async () => await LogsScrollToEnd.Handle(Unit.Default));
    }

    /// <summary>
    /// Releases the resources used by the EditInstanceViewModel and performs cleanup operations.
    /// </summary>
    /// <param name="disposing">
    /// A boolean value indicating whether the method is being called directly or indirectly by a finalizer.
    /// If true, the method has been called directly or indirectly by a user's code. Managed and unmanaged resources can be disposed.
    /// If false, the method has been called by the runtime from inside the finalizer, and only unmanaged resources can be disposed.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _logger.Debug("Freeing memory in EditInstanceViewModel...");
        IsClosing = true;
        GlobalEvents.OnInstanceLogged -= OnInstanceLogged;
        
        Accounts.Clear();
        
        InstanceName = string.Empty;
        GameDirectory = null;
        Logs = string.Empty;
    }

    private async Task InitAsync(CancellationToken cancellationToken = default)
    {
        var instances = await LauncherHelper.GetInstancesAsync(cancellationToken);
        var currentInstance = instances.FirstOrDefault(x => x.Id == InstanceId);
        var accountData = await LauncherHelper.GetAccountDataAsync(cancellationToken);
        if (currentInstance == null)
        {
            _logger.Error($"Instance with ID '{InstanceId}' not found.");
            throw new KeyNotFoundException($"Instance with ID '{InstanceId}' not found.");
        }

        InstanceName = currentInstance.Name;
        IsVanilla = currentInstance.Kind == EMinecraftKind.VANILLA;
        GameDirectory = currentInstance.GameDirectory;

        IsInitialized = true;
        Accounts = accountData.Accounts;
        

        // Logging setup
        GlobalEvents.OnInstanceLogged += OnInstanceLogged;
        Logs = GlobalEvents.GetInstanceLogs(InstanceId);
        if (!string.IsNullOrEmpty(Logs))
            await LogsScrollToEnd.Handle(Unit.Default);
        
        await Mods.InitAsync(cancellationToken);
        await ResourcePacks.InitAsync(cancellationToken);
        await Screenshots.InitAsync(cancellationToken);
        await Servers.InitAsync(cancellationToken);
        await Settings.InitAsync(currentInstance.Config, cancellationToken);
        await ShaderPacks.InitAsync(cancellationToken);
        await Worlds.InitAsync(cancellationToken);
    }

    #region Common
    
    #region Window
    [RelayCommand]
    public async Task MinimizeWindow()
    {
        await MinimizeWindowInteraction.Handle(Unit.Default);
    }

    [RelayCommand]
    public async Task MaximizeWindow()
    {
        await MaximizeWindowInteraction.Handle(Unit.Default);
    }

    [RelayCommand]
    public async Task CloseWindow()
    {
        await CloseWindowInteraction.Handle(Unit.Default);
    }
    #endregion

    [RelayCommand]
    private async Task SwitchTab(EEditInstanceTab tab) => await TabSwitchInteraction.Handle(tab);

    #endregion
    
    #region Logs

    /// <summary>
    /// Scrolls the logs to the end by triggering the LogsScrollToEnd interaction.
    /// </summary>
    [RelayCommand]
    private async Task ScrollLogsToEnd() => await LogsScrollToEnd.Handle(Unit.Default);

    /// <summary>
    /// Copies the current logs to the system clipboard by triggering the SetClipboardText interaction.
    /// </summary>
    [RelayCommand]
    private async Task CopyLogs() => await SetClipboardText.Handle(Logs);

    /// <summary>
    /// Clears the logs for the current instance and updates the global log storage.
    /// </summary>
    [RelayCommand]
    private void ClearLogs()
    {
        Logs = string.Empty;
        GlobalEvents.CleareInstanceLogs(InstanceId);
    }

    #endregion
}