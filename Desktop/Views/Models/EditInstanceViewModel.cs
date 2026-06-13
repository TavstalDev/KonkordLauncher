using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.Platform;
using Tavstal.KonkordLauncher.Core.Models.Logging;
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
    public readonly InstanceModel Instance = null!;
    private readonly ICustomLogger _logger = null!;
    private readonly ILauncherStore _launcherStore = null!;
    private const int MaxLogLines = 1000;
    
    public bool IsClosing;
    public bool IsInitialized;
    public EditInstanceViewModel_Mods Mods { get; private set; }  = null!;
    public EditInstanceViewModel_ResourcePacks ResourcePacks { get; private set; }  = null!;
    public EditInstanceViewModel_Screenshots Screenshots { get; private set; }  = null!;
    public EditInstanceViewModel_Servers Servers { get; private set; }  = null!;
    public EditInstanceViewModel_Settings Settings { get; private set; }  = null!;
    public EditInstanceViewModel_ShaderPacks ShaderPacks { get; private set; }  = null!;
    public EditInstanceViewModel_Worlds Worlds { get; private set; }  = null!;

    public bool IsLinux => OSHelper.GetOperatingSystem() == EOperatingSystem.LINUX;
    public List<AccountModel> Accounts { get; private set; }  = null!;

    #region Interactions
    
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
    public Interaction<EResourceType, Unit> OpenResourceDownloadDialog { get; } = new();

    #endregion

    #region Observable Properties

    [ObservableProperty]
    public partial EEditInstanceTab EditInstanceTab { get; set; }

    [ObservableProperty]
    public partial EInstanceSettingsTab InstanceSettingsTab { get; set; }

    [ObservableProperty] 
    public partial string InstanceName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? GameDirectory { get; set; }

    [ObservableProperty]
    public partial bool IsVanilla { get; set; }
    public ObservableCollection<string> Logs { get; set; } = [];

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="EditInstanceViewModel"/> class.
    /// </summary>
    /// <param name="instance">The instance model representing the Minecraft instance to be edited.</param>
    [RequiresUnreferencedCode( "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public EditInstanceViewModel(InstanceModel instance)
    {
        if (Design.IsDesignMode)
            return;

        var services = Program.ServiceProvider;
        _logger = services.GetRequiredService<ICustomLogger<EditInstanceViewModel>>();
        _launcherStore = services.GetRequiredService<ILauncherStore>();
        
        Instance = instance;
        Mods = new EditInstanceViewModel_Mods(this);
        ResourcePacks = new EditInstanceViewModel_ResourcePacks(this);
        Screenshots = new EditInstanceViewModel_Screenshots(this);
        Servers = new EditInstanceViewModel_Servers(this);
        Settings = new EditInstanceViewModel_Settings(this);
        ShaderPacks = new EditInstanceViewModel_ShaderPacks(this);
        Worlds = new EditInstanceViewModel_Worlds(this);
        _  = InitAsync().ContinueWith(t =>
        {
            if (t.IsFaulted)
                _logger.LogError(t.Exception, "Error initializing EditInstanceViewModel");
        });
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
        _logger.LogDebug("Freeing memory in EditInstanceViewModel...");
        IsClosing = true;
        GlobalEvents.OnInstanceLogged -= OnInstanceLogged;
        
        Accounts.Clear();
        
        InstanceName = string.Empty;
        GameDirectory = null;
        Logs.Clear();
    }
    
    /// <summary>
    /// Handles log messages for a specific instance by updating the Logs property
    /// and triggering the LogsScrollToEnd interaction to scroll to the end of the logs.
    /// </summary>
    /// <param name="instanceId">The ID of the instance that generated the log message.</param>
    /// <param name="logMessage">The log message to be handled.</param>
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

    /// <summary>
    /// Asynchronously initializes the view-model for the edit-instance UI.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    private async Task InitAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        var accountData = await _launcherStore.GetAccountDataAsync(cancellationToken);

        InstanceName = Instance.Name;
        IsVanilla = Instance.Kind == EMinecraftKind.VANILLA;
        GameDirectory = Instance.GameDirectory;

        IsInitialized = true;
        Accounts = [];
        foreach (var account in accountData.Accounts)
            Accounts.Add(new AccountModel(account, account.Id == accountData.SelectedAccountId));
        

        // Logging setup
        GlobalEvents.OnInstanceLogged += OnInstanceLogged;
        var logs = GlobalEvents.GetInstanceLogs(Instance.Id);
        if (!string.IsNullOrEmpty(logs))
            Logs.AddRange(logs.Split(Environment.NewLine));

        // TODO: Performanc issue - initializing all tabs together can make the edit window open slowly.
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

    #region Common
    
    /// <summary>
    /// Requests the window to close by invoking the <see cref="CloseWindowInteraction"/> interaction.
    /// </summary>
    /// <returns>A task that completes when the close request has been handled.</returns>
    [RelayCommand]
    public async Task CloseWindow() => await CloseWindowInteraction.Handle(Unit.Default);

    /// <summary>
    /// Requests a tab switch inside the edit-instance UI by invoking the <see cref="TabSwitchInteraction"/> interaction.
    /// </summary>
    /// <param name="tab">The target tab to switch to.</param>
    /// <returns>A task that completes when the tab switch request has been handled.</returns>
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
    private async Task CopyLogs()
    {
        var newLine = Environment.NewLine;
        await SetClipboardText.Handle(string.Join(newLine, Logs));
    }

    /// <summary>
    /// Clears the logs for the current instance and updates the global log storage.
    /// </summary>
    [RelayCommand]
    private void ClearLogs()
    {
        Logs.Clear();
        GlobalEvents.CleareInstanceLogs(Instance.Id);
    }

    #endregion
}