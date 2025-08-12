using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using NbtLib;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Common.Models.InstanceConfig;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Desktop.Helpers;
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
    [ObservableProperty] private ModModel? _selectedMod;
    
    public ObservableCollection<ResourcePackModel> ResourcePacks { get; set; } = [];
    [ObservableProperty] private ResourcePackModel? _selectedResourcePack;
    
    public ObservableCollection<ShaderPackModel> ShaderPacks { get; set; } = [];
    [ObservableProperty] private ShaderPackModel? _selectedShaderPack;
    
    public ObservableCollection<WorldModel> Worlds { get; set; } = [];
    [ObservableProperty] private WorldModel? _selectedWorld;
    
    public ObservableCollection<ServerModel> Servers { get; set; } = [];
    [ObservableProperty] private ServerModel? _selectedServer;
    
    public ObservableCollection<ScreenshotModel> Screenshots { get; set; } = [];
    [ObservableProperty] private ScreenshotModel? _selectedScreenshot;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanRemoveEnvironmentVariable))] private InstanceConfigModel _instanceConfig;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanRemoveEnvironmentVariable))] private int? _selectedEnvironmentVariableIndex;
    public bool CanRemoveEnvironmentVariable => SelectedEnvironmentVariableIndex is >= 0 && InstanceConfig.EnableEnvironment;
    
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
        
        RefreshWorlds();
        RefreshServers();
        RefreshScreenshots();
    }
    
    #region Refresh Methods

    #region Worlds

    public void RefreshWorlds()
    {
        if (_instance.GameDirectory == null)
            return;
        
        string worldsDir = Path.Combine(_instance.GameDirectory, "saves");
        if (!Directory.Exists(worldsDir))
            return;
        
        Worlds.Clear();
        var worldDirs = Directory.GetDirectories(worldsDir);
        foreach (var worldDir in worldDirs)
        {
            var worldName = "unknown";
            string levelDatPath = Path.Combine(worldDir, "level.dat");
            string gamemode = "unknown";
            string lastPlayed = "unknown";
            Bitmap? icon = null;

            if (File.Exists(levelDatPath))
            {
                using var inputStream = File.OpenRead(levelDatPath);
                using var gzip = new GZipStream(inputStream, CompressionMode.Decompress);
                using var mem = new MemoryStream();
                gzip.CopyTo(mem);
                mem.Seek(0, SeekOrigin.Begin);
                var data = NbtConvert.DeserializeObject<Level>(mem);

                if (data != null)
                {
                    DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    epoch = epoch.AddMilliseconds(data.Data.LastPlayed);
                    // TODO: Use more human-readable format

                    if (!string.IsNullOrEmpty(data.Data.LevelName))
                        worldName = data.Data.LevelName;
                    
                    // GameMode (int) — try "GameType" first, then "GameMode"
                    if (data.Data.GameMode != null)
                        gamemode = (data.Data.GameMode) switch
                        {
                            0 => "Survival",
                            1 => "Creative",
                            2 => "Adventure",
                            3 => "Spectator",
                            _ => "Unknown"
                        };
                    else if (data.Data.GameType != null)
                        gamemode = (data.Data.GameType) switch
                        {
                            0 => "Survival",
                            1 => "Creative",
                            2 => "Adventure",
                            3 => "Spectator",
                            _ => "Unknown"
                        };
                }
            }
            
            var files = Directory.EnumerateFiles(worldDir, "*", SearchOption.AllDirectories);
            long size = files.Sum(file => new FileInfo(file).Length);
            if (File.Exists(Path.Combine(worldDir, "icon.png")))
            {
                try
                {
                    icon = new Bitmap(Path.Combine(worldDir, "icon.png"));
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to load world icon for {worldName}: {ex.Message}");
                }
            }
            if (icon == null)
                icon = ImageHelper.LoadFromResource(new Uri("avares://Desktop/Assets/Images/default_world.png"));
            
            Worlds.Add(new WorldModel(worldName, gamemode, lastPlayed, size, icon));
        }
    }

    #endregion
    
    #region  Servers

    /// <summary>
    /// Saves the current list of Minecraft servers to the `servers.dat` file
    /// in the game directory. The method serializes the server data into NBT format
    /// and writes it to the file.
    /// </summary>
    public void SaveServers()
    {
        _logger.Debug("Saving servers to servers.dat file...");
        if (_instance.GameDirectory == null)
            return;
        try
        {
            string filePath = Path.Combine(_instance.GameDirectory, "servers.dat");
            
            var root = new NbtCompoundTag();
            var serversList = new NbtListTag(NbtTagType.Compound);

            foreach (var s in Servers)
            {
                var serverTag = new NbtCompoundTag
                {
                    { "name", new NbtStringTag(s.Name) },
                    { "ip", new NbtStringTag(s.Ip) },
                    { "acceptTextures", new NbtIntTag(s.AcceptTextures) },
                };
                
                if (s.HideAddress.HasValue)
                    serverTag.Add("hideAddress", new NbtIntTag(s.HideAddress.Value));
                
                if (!string.IsNullOrEmpty(s.Icon))
                    serverTag.Add("icon", new NbtStringTag(s.Icon));

                serversList.Add(serverTag);
            }

            root.Add("servers", serversList);
            
            using var outputStream = new NbtWriter().CreateUncompressedNbtStream(root, "");
            using var fileStream = File.Create(filePath);
            outputStream.Seek(0, SeekOrigin.Begin);
            outputStream.CopyTo(fileStream);
        }
        catch (Exception ex)
        {
            _logger.Exc("Failed to save servers to servers.dat file.");
            _logger.Error(ex);
        }
    }

    /// <summary>
    /// Handles changes to the `Servers` collection by saving the updated list
    /// of servers to the `servers.dat` file.
    /// </summary>
    /// <param name="sender">The source of the event, typically the `Servers` collection.</param>
    /// <param name="e">The event data containing details about the collection change.</param>
    private void ServersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SaveServers();
    }
    
    /// <summary>
    /// Refreshes the list of Minecraft servers by reading the `servers.dat` file
    /// from the game directory and updating the Servers collection with the data.
    /// </summary>
    public void RefreshServers()
    {
        if (_instance.GameDirectory == null)
            return;
    
        // Construct the file path for the servers.dat file
        string filePath = Path.Combine(_instance.GameDirectory, "servers.dat");
        if (!File.Exists(filePath))
            return;

        // Open the servers.dat file and deserialize its content
        using var inputStream = System.IO.File.OpenRead(filePath);
        var serversDat = NbtConvert.DeserializeObject<ServersDat>(inputStream);
        if (serversDat == null)
            return;
    
        Servers.CollectionChanged -= ServersOnCollectionChanged;
        
        // Clear the existing Servers collection and populate it with new data
        Servers.Clear();
        foreach (var server in serversDat.Servers)
            Servers.Add(new ServerModel(server.Name, server.Ip, server.AcceptTextures, server.HideAddress, server.Icon));
        
        Servers.CollectionChanged += ServersOnCollectionChanged;
    }

    #endregion
    
    /// <summary>
    /// Refreshes the list of screenshots by scanning the game directory for PNG files
    /// and updating the Screenshots collection with their metadata and image data.
    /// </summary>
    public void RefreshScreenshots()
    {
        if (_instance.GameDirectory == null)
            return;

        string screenshotDir = Path.Combine(_instance.GameDirectory, "screenshots");
        if (!Directory.Exists(screenshotDir))
            return;
        
        Screenshots.Clear();
        var screenshots = Directory.GetFiles(screenshotDir, "*.png");
        foreach (var screenshot in screenshots)
        {
            var bytes = File.ReadAllBytes(screenshot);
            var newScreenshot = new ScreenshotModel(screenshot, new Bitmap(screenshot), bytes.LongLength);
            Screenshots.Add(newScreenshot);
        }
    }
    #endregion

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
    
    /// <summary>
    /// Handles changes to a collection within the instance configuration model.
    /// Logs the change and saves the updated configuration to a file if the view model is initialized.
    /// </summary>
    /// <param name="sender">The source of the event, typically the collection that changed.</param>
    /// <param name="e">The event data containing details about the collection change.</param>
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