using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Common.Models.InstanceConfig;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Platform;
using Tavstal.KonkordLauncher.Core.Helpers.Serialization;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Config.Instance;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.EditInstance;

public partial class EditInstanceViewModel_Settings  : KonkordObservableObject
{
    private readonly ICustomLogger _logger;
    private readonly ITranslationService _translationService;
    private readonly ILauncherStore _launcherStore;
    private readonly EditInstanceViewModel _parent;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRemoveEnvironmentVariable))]
    public partial InstanceConfigModel InstanceConfig { get; set; }

    [ObservableProperty]
    public partial int? OverridenAccountIndex { get; set; } = 0;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanRemoveEnvironmentVariable))]
    private int? _selectedEnvironmentVariableIndex;
    
    public bool CanRemoveEnvironmentVariable =>
        SelectedEnvironmentVariableIndex is >= 0 && InstanceConfig.EnableEnvironment;
    
    public EditInstanceViewModel_Settings(EditInstanceViewModel parent)
    {
        _parent = parent;
        if (Design.IsDesignMode)
        {
            InstanceConfig = new InstanceConfigModel();
            return;
        }
        
        var services = Program.ServiceProvider;
        _logger = services.GetRequiredService<ICustomLogger<EditInstanceViewModel_Settings>>();
        _translationService = services.GetRequiredService<ITranslationService>();
        _launcherStore = services.GetRequiredService<ILauncherStore>();
    }
    
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        UnsubscribeFromConfigChildren(InstanceConfig);
        InstanceConfig = new InstanceConfigModel();
        SelectedEnvironmentVariableIndex = null;
    }
    
    public async Task InitAsync(InstanceConfig config, CancellationToken cancellationToken = default)
    {
        InstanceConfig = new InstanceConfigModel(config);
        
        //SubscribeToConfigChildren(InstanceConfig);
        if (!string.IsNullOrEmpty(InstanceConfig.Misc.AccountId))
            OverridenAccountIndex = _parent.Accounts.FindIndex(x => x.Id == InstanceConfig.Misc.AccountId);
    }
    
    #region Commands

    [RelayCommand]
    private async Task SwitchSettingsTab(EInstanceSettingsTab tab) => await _parent.SettingsTabSwitchInteraction.Handle(tab);

    [RelayCommand]
    private async Task JavaDirSelect()
    {
        var result = await _parent.ShowDirPickerInteraction.Handle(_translationService.Translate("common.select.directory"));
        if (string.IsNullOrEmpty(result) || !Directory.Exists(result))
            return;
        
        InstanceConfig.Java.DefaultJavaPath = Path.Combine(result, OSHelper.GetOperatingSystem() == EOperatingSystem.Windows ? "javaw.exe" : "java");
    }
    
    [RelayCommand]
    private async Task JavaPathSelector()
    {
        var javaVersion = await _parent.ShowJavaPathSelector.Handle(Unit.Default);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (javaVersion == null)
            return;
        
        InstanceConfig.Java.DefaultJavaPath = javaVersion.Path;
    }
    
    #endregion

    /// <summary>
    /// Subscribes to the PropertyChanged event of the child configuration models
    /// to monitor changes in their properties.
    /// </summary>
    /// <param name="config">The instance configuration model to subscribe to.</param>
    private void SubscribeToConfigChildren(InstanceConfigModel config)
    {
        config.Game.PropertyChanged += OnChildConfigPropertyChanged;
        config.Java.PropertyChanged += OnChildConfigPropertyChanged;
        config.Commands.PropertyChanged += OnChildConfigPropertyChanged;
        config.Environment.CollectionChanged += OnChildConfigCollectionChanged;
        config.Misc.PropertyChanged += OnChildConfigPropertyChanged;
    }

    /// <summary>
    /// Unsubscribes from the PropertyChanged event of the child configuration models
    /// to stop monitoring changes in their properties.
    /// </summary>
    /// <param name="config">The instance configuration model to unsubscribe from.</param>
    private void UnsubscribeFromConfigChildren(InstanceConfigModel config)
    {
        config.Game.PropertyChanged -= OnChildConfigPropertyChanged;
        config.Java.PropertyChanged -= OnChildConfigPropertyChanged;
        config.Commands.PropertyChanged -= OnChildConfigPropertyChanged;
        config.Environment.CollectionChanged -= OnChildConfigCollectionChanged;
        config.Misc.PropertyChanged -= OnChildConfigPropertyChanged;
    }

    /// <summary>
    /// Handles changes to the InstanceConfigModel by unsubscribing from the old configuration
    /// and subscribing to the new configuration. Saves the new configuration to a file if initialized.
    /// </summary>
    /// <param name="oldValue">The previous instance configuration model.</param>
    /// <param name="newValue">The new instance configuration model.</param>
    partial void OnInstanceConfigChanged(InstanceConfigModel? oldValue, InstanceConfigModel newValue)
    {
        if (_parent.IsClosing)
            return;
        
        _logger.LogDebug("InstanceConfig changed with old and new value. Unsubscribing from old, subscribing to new.");
        
        if (oldValue != null)
            UnsubscribeFromConfigChildren(oldValue);

        SubscribeToConfigChildren(newValue);

        if (!_parent.IsInitialized)
            return;
        Task.Run(async () => await SaveCoreConfigToFileAsync(InstanceConfig));
    }

    /// <summary>
    /// Handles the PropertyChanged event for child configuration models.
    /// Logs the change and saves the updated configuration to a file if initialized.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event data containing the name of the changed property.</param>
    private void OnChildConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!_parent.IsInitialized || _parent.IsClosing)
            return;
        _logger.LogDebug($"Inner property '{e.PropertyName}' changed on {sender?.GetType().Name}. Saving to file...");
        Task.Run(async () => await SaveCoreConfigToFileAsync(InstanceConfig));
    }
    
    /// <summary>
    /// Handles changes to a collection within the instance configuration model.
    /// Logs the change and saves the updated configuration to a file if the view model is initialized.
    /// </summary>
    /// <param name="sender">The source of the event, typically the collection that changed.</param>
    /// <param name="e">The event data containing details about the collection change.</param>
    private void OnChildConfigCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!_parent.IsInitialized || _parent.IsClosing)
            return;
        _logger.LogDebug($"Inner collection changed on {sender?.GetType().Name}. Saving to file...");
        Task.Run(async () => await SaveCoreConfigToFileAsync(InstanceConfig));
    }

    /// <summary>
    /// Saves the updated instance configuration to a file. Ensures that the minimum memory
    /// does not exceed the maximum memory in the Java configuration.
    /// </summary>
    /// <param name="newValue">The updated instance configuration model to save.</param>
    private async Task SaveCoreConfigToFileAsync(InstanceConfigModel newValue)
    {
        if (_parent.IsClosing)
            return;
        if (newValue.Java.MinMemory > newValue.Java.MaxMemory)
            newValue.Java.MinMemory = newValue.Java.MaxMemory;

        var instances = await _launcherStore.GetInstancesAsync();
        int index = 0;
        Instance? instanceToSave = null;
        foreach (var instance in instances)
        {
            if (instance.Id == _parent.Instance.Id)
            {
                instanceToSave = instance;
                break;
            }

            index++;
        }

        if (instanceToSave == null)
            return;
        
        var environmentVariables = newValue.Environment
            .Select(x => new EnvironmentVariable(x.Key, x.Value))
            .ToList();

        instanceToSave.Config = new InstanceConfig
        {
            Java = new JavaConfig
            {
                MinMemory = newValue.Java.MinMemory,
                MaxMemory = newValue.Java.MaxMemory,
                PermaGen = newValue.Java.PermaGen,
                JavaPath = newValue.Java.DefaultJavaPath,
                JvmArguments = newValue.Java.JvmArguments
            },
            Game = new InstanceGameConfig
            {
                StartMaximized = newValue.Game.StartMaximized,
                WindowHeight = newValue.Game.WindowHeight,
                WindowWidth = newValue.Game.WindowWidth,
                ShowConsoleWhileGameRunning = newValue.Game.ShowConsoleWhileGameRunning,
                ShowConsoleWhenGameCrashes = newValue.Game.ShowConsoleWhenGameCrashes,
                CloseConsoleOnGameExit = newValue.Game.CloseConsoleOnGameExit,
                EnableFeralGameMode = newValue.Game.EnableFeralGameMode,
                EnableMangoHud = newValue.Game.EnableMangoHud,
                UseDedicatedGpu = newValue.Game.UseDedicatedGpu,
            },
            Commands = new InstanceCommandsConfig
            {
                PreLaunchCommand = newValue.Commands.PreLaunchCommand,
                WrapperCommand = newValue.Commands.WrapperCommand,
                PostExitCommand = newValue.Commands.PostExitCommand,
            },
            EnableEnvironment = newValue.EnableEnvironment,
            Environment = environmentVariables,
            Misc = new InstanceMiscConfig
            {
                UseCustomGlfw = newValue.Misc.UseCustomGlfw,
                CustomGlfwPath = newValue.Misc.CustomGlfwPath,
                UseCustomOpenAL = newValue.Misc.UseCustomOpenAL,
                CustomOpenALPath = newValue.Misc.CustomOpenALPath,
                AccountId = newValue.Misc.AccountId,
                OverrideAccount = newValue.Misc.OverrideAccount,
                JoinServerOnLaunch = newValue.Misc.JoinServerOnLaunch,
                ServerAddress = newValue.Misc.ServerAddress,
            }
        };
        instances[index] = instanceToSave;

        await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherInstancesPath, instances);
        GlobalEvents.InvokeInstanceUpdated(_parent.Instance.Id);
        _logger.LogDebug("Saved instance config to file.");
    }
}