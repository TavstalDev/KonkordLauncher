using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Common.Models.InstanceConfig;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Config.Instance;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

public partial class EditInstanceViewModel : ObservableObject
{
    private readonly bool _isInitialized;
    private readonly EditInstanceWindow _parentWindow;
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(EditInstanceViewModel));
    private readonly InstanceModel _instance;
    public bool IsLinux => OSHelper.GetOperatingSystem() == EOperatingSystem.Linux;
    public bool IsVanilla => _instance.Kind == EMinecraftKind.VANILLA;
    public List<Account> Accounts => LauncherHelper.GetAccountData().Accounts;

    public ObservableCollection<ModModel> Mods { get; set; } = [];
    public ObservableCollection<ResourcePackModel> ResourcePacks { get; set; } = [];
    public ObservableCollection<ShaderPackModel> ShaderPacks { get; set; } = [];
    public ObservableCollection<WorldModel> Worlds { get; set; } = [];
    public ObservableCollection<ServerModel> Servers { get; set; } = [];
    public ObservableCollection<ScreenshotModel> Screenshots { get; set; } = [];

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanRemoveEnvironmentVariable))] private InstanceConfigModel _instanceConfig;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanRemoveEnvironmentVariable))] private int? _selectedEnvironmentVariableIndex;
    public bool CanRemoveEnvironmentVariable => SelectedEnvironmentVariableIndex.HasValue && SelectedEnvironmentVariableIndex.Value >= 0 && InstanceConfig.EnableEnvironment;
    
    public EditInstanceViewModel(EditInstanceWindow parent, InstanceModel instance, InstanceConfig instanceConfig)
    {
        if (Design.IsDesignMode)
        {
            _instanceConfig = new InstanceConfigModel(instanceConfig);
            return;
        }

        _parentWindow = parent;
        _instance = instance;
        _instanceConfig = new InstanceConfigModel(instanceConfig);
        _isInitialized = true;
        SubscribeToConfigChildren(_instanceConfig);
        if (!string.IsNullOrEmpty(_instanceConfig.Misc.AccountId))
            _parentWindow.StOverridenAccountInput.SelectedIndex =
                Accounts.FindIndex(x => x.Id == _instanceConfig.Misc.AccountId);
        
        RefreshScreenshots();
    }
    
    /// <summary>
    /// Refreshes the list of screenshots by scanning the game directory for PNG files
    /// and updating the Screenshots collection with their metadata and image data.
    /// </summary>
    private void RefreshScreenshots()
    {
        if (_instance.GameDirectory == null)
            return;

        string screenshotDir = System.IO.Path.Combine(_instance.GameDirectory, "screenshots");
        if (!System.IO.Directory.Exists(screenshotDir))
            return;
        
        Screenshots.Clear();
        var screenshots = System.IO.Directory.GetFiles(screenshotDir, "*.png");
        foreach (var screenshot in screenshots)
        {
            var bytes = System.IO.File.ReadAllBytes(screenshot);
            Screenshots.Add(new ScreenshotModel()
            {
                Name = System.IO.Path.GetFileName(screenshot),
                Image = new Bitmap(screenshot),
                Size = bytes.LongLength
            });
        }
    }

    #region Settings

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
        _logger.Debug("InstanceConfig changed with old and new value. Unsubscribing from old, subscribing to new.");

        if (oldValue != null)
            UnsubscribeFromConfigChildren(oldValue);

        SubscribeToConfigChildren(newValue);

        if (!_isInitialized)
            return;
        SaveCoreConfigToFile(newValue);
    }

    /// <summary>
    /// Handles the PropertyChanged event for child configuration models.
    /// Logs the change and saves the updated configuration to a file if initialized.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event data containing the name of the changed property.</param>
    private void OnChildConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!_isInitialized)
            return;
        _logger.Debug($"Inner property '{e.PropertyName}' changed on {sender?.GetType().Name}. Saving to file...");
        SaveCoreConfigToFile(InstanceConfig);
    }
    
    private void OnChildConfigCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!_isInitialized)
            return;
        _logger.Debug($"Inner collection changed on {sender?.GetType().Name}. Saving to file...");
        SaveCoreConfigToFile(InstanceConfig);
    }

    /// <summary>
    /// Saves the updated instance configuration to a file. Ensures that the minimum memory
    /// does not exceed the maximum memory in the Java configuration.
    /// </summary>
    /// <param name="newValue">The updated instance configuration model to save.</param>
    private void SaveCoreConfigToFile(InstanceConfigModel newValue)
    {
        if (newValue.Java.MinMemory > newValue.Java.MaxMemory)
            newValue.Java.MinMemory = newValue.Java.MaxMemory;

        var instances = LauncherHelper.GetInstances();
        int index = 0;
        Instance? instanceToSave = null;
        foreach (var instance in instances)
        {
            if (instance.Id == _instance.Id)
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

        instanceToSave.Config = new InstanceConfig()
        {
            Java = new JavaConfig()
            {
                MinMemory = newValue.Java.MinMemory,
                MaxMemory = newValue.Java.MaxMemory,
                PermaGen = newValue.Java.PermaGen,
                JavaPath = newValue.Java.DefaultJavaPath,
                JvmArguments = newValue.Java.JvmArguments
            },
            Game = new InstanceGameConfig()
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
            Commands = new InstanceCommandsConfig()
            {
                PreLaunchCommand = newValue.Commands.PreLaunchCommand,
                WrapperCommand = newValue.Commands.WrapperCommand,
                PostExitCommand = newValue.Commands.PostExitCommand,
            },
            EnableEnvironment = newValue.EnableEnvironment,
            Environment = environmentVariables,
            Misc = new InstanceMiscConfig()
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

        JsonHelper.WriteJsonFile(PathHelper.LauncherInstancesPath, instances);
        App.InvokeInstancesChanged();
    }

    #endregion
}