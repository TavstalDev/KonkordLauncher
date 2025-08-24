using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Common.Models.InstanceConfig;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Instances;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;
using Tavstal.KonkordLauncher.Desktop.Helpers;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

namespace Tavstal.KonkordLauncher.Desktop.Models;

/// <summary>
/// Represents a model for a Minecraft instance, including its properties and behaviors.
/// </summary>
public partial class InstanceModel : ObservableObject, IProgressReporter
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(InstanceModel));
    private long _lastReadPosition;
    private FileSystemWatcher? _watcher;

    #region Observable Properties
    /// <summary>
    /// Gets or sets the unique identifier of the instance.
    /// </summary>
    [ObservableProperty] private string _id;

    /// <summary>
    /// Gets or sets the name of the instance.
    /// </summary>
    [ObservableProperty] private string _name;

    /// <summary>
    /// Gets or sets the group to which the instance belongs.
    /// </summary>
    [ObservableProperty] private string? _group;

    /// <summary>
    /// Gets or sets the file path to the icon of the instance.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Icon))]
    private string _iconPath;

    /// <summary>
    /// Gets or sets the Minecraft version associated with the instance.
    /// </summary>
    [ObservableProperty] private string _minecraftVersion;

    /// <summary>
    /// Gets or sets the custom version of the instance, if any.
    /// </summary>
    [ObservableProperty] private string _customVersion;

    /// <summary>
    /// Gets or sets the profile type of the instance.
    /// </summary>
    [ObservableProperty] private EProfileType _type;

    /// <summary>
    /// Gets or sets the kind of Minecraft associated with the instance.
    /// </summary>
    [ObservableProperty] private EMinecraftKind _kind;

    /// <summary>
    /// Gets or sets the custom game directory for the instance, if specified.
    /// </summary>
    [ObservableProperty] private string? _gameDirectory;

    /// <summary>
    /// Gets or sets the configuration of the instance.
    /// </summary>
    [ObservableProperty] private InstanceConfig _configModel;

    /// <summary>
    /// Gets or sets the process associated with the running game.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGameRunning))]
    private Process? _gameProcess;

    /// <summary>
    /// Gets or sets a value indicating whether the game is currently running.
    /// </summary>
    [ObservableProperty] private bool _isGameRunning;
    
    /// <summary>
    /// Gets the icon of the instance as a bitmap. If the icon path is not set, a default icon is used.
    /// </summary>
    public Bitmap Icon => string.IsNullOrEmpty(IconPath)
        ? ImageHelper.LoadFromResource(new Uri("avares://Desktop/Assets/Icons/dirt.png"))
        : new Bitmap(IconPath);
    #endregion
    
    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceModel"/> class.
    /// </summary>
    public InstanceModel() {}

    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceModel"/> class using the specified common instance model.
    /// </summary>
    /// <param name="instance">The common instance model to initialize from.</param>
    public InstanceModel(Common.Models.Instance instance)
    {
        Id = instance.Id;
        Name = instance.Name;
        Group = instance.Group;
        IconPath = instance.IconPath;
        MinecraftVersion = instance.MinecraftVersion;
        CustomVersion = instance.CustomVersion;
        Type = instance.Type;
        Kind = instance.Kind;
        GameDirectory = instance.GameDirectory;
        ConfigModel = instance.Config;
    }

    /// <summary>
    /// Attaches event handlers to the game process to handle its exit and disposal events.
    /// </summary>
    public void AttachProcessEvent()
    {
        if (GameProcess == null)
            return;

        GameProcess.Exited += (_, _) =>
        {
            IsGameRunning = false;
            GameProcess = null;
        };

        GameProcess.Disposed += (_, _) =>
        {
            IsGameRunning = false;
            GameProcess = null;
        };

        if (string.IsNullOrEmpty(GameDirectory))
            return;

        string logsDir = Path.Combine(GameDirectory, "logs");

        if (_watcher != null)
        {
            _watcher.Changed -= OnLogFileChanged;
            _watcher.Created -= OnLogFileChanged;
            _watcher.Renamed -= OnLogFileChanged;
        }
        
        if (!Directory.Exists(logsDir))
        {
            _logger.Warn($"Logs directory does not exist: {logsDir}. Creating it.");
            Directory.CreateDirectory(logsDir);
        }
        
        _watcher = new FileSystemWatcher
        {
            Path = logsDir,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
        };
        _watcher.EnableRaisingEvents = true;
        _watcher.Changed += OnLogFileChanged;
        _watcher.Created += OnLogFileChanged;
        _watcher.Error += (_, e) =>
        {
            _logger.Error("Watcher error: " + e.GetException().Message);
        };
    }


    public async Task LaunchAsync(Interaction<string, Unit> showLogsWindow, Interaction<Unit, Unit> closeWindow, Interaction<Alert, Unit> showAlertDialog, string? serverAddress = null)
    {
        _lastReadPosition = 0;
        _logger.Debug($"Launching instance: {Name}");
        var accountData = await LauncherHelper.GetAccountDataAsync();
        var account = ConfigModel.Misc.OverrideAccount ? 
            accountData.Accounts.FirstOrDefault(x => x.Id == ConfigModel.Misc.AccountId) 
            : accountData.Accounts.FirstOrDefault(x => x.Id == accountData.SelectedAccountId);
        
        if (account == null)
        {
            _logger.Error("No account selected for launching the ");
            await showAlertDialog.Handle(new Alert(TranslationManager.Translate("account.none.title"), TranslationManager.Translate("account.none.message"), EAlertType.Warning));
            return;
        }

        try
        {
            string wrapperCommand = ConfigModel.Commands.WrapperCommand;
            // Add gamemoderun if enabled
            if (ConfigModel.Game.EnableFeralGameMode && !wrapperCommand.Contains("gamemoderun"))
                wrapperCommand = "gamemoderun " + wrapperCommand;

            // Add mangohud if enabled
            if (ConfigModel.Game.EnableMangoHud && !wrapperCommand.Contains("mangohud"))
                wrapperCommand = "mangohud " + wrapperCommand;

            // Attempt to force the use of a dedicated GPU if configured
            var environmentVariables = ConfigModel.EnableEnvironment
                ? ConfigModel.Environment
                : [];
            var gpuInfo = OSHelper.GetDedicatedGpuType();
            if (ConfigModel.Game.UseDedicatedGpu && gpuInfo != null)
            {
                switch (OSHelper.GetOperatingSystem())
                {
                    case EOperatingSystem.Windows:
                    {
                        switch (gpuInfo.Value.Item1)
                        {
                            case "amd":
                            {
                                environmentVariables.Add(new("AMD_POWERXPRESS_REQUEST_HIGH_PERFORMANCE", "1"));
                                break;
                            }
                            case "nvidia":
                            {
                                environmentVariables.Add(new("__NV_GPU_USE_DISCRETE_GPU", "1"));
                                break;
                            }
                        }
                        break;
                    }
                    case EOperatingSystem.Linux:
                    {
                        switch (gpuInfo.Value.Item1)
                        {
                            case "amd":
                            case "intel":
                            {
                                environmentVariables.Add(new("DRI_PRIME", "1"));
                                break;
                            }
                            case "nvidia":
                            {
                                environmentVariables.Add(new("__NV_PRIME_RENDER_OFFLOAD", "1"));
                                environmentVariables.Add(new("__GLX_VENDOR_LIBRARY_NAME", "nvidia"));
                                break;
                            }
                        }
                        break;
                    }
                }
            }

            var envDic = new Dictionary<string, string>();
            foreach (var env in environmentVariables)
                envDic[env.Key] = env.Value;

            // Add custom native libraries if configured
            List<string> nativeLibraries = [];
            if (ConfigModel.Misc.UseCustomGlfw && File.Exists(ConfigModel.Misc.CustomGlfwPath))
                nativeLibraries.Add(ConfigModel.Misc.CustomGlfwPath);
            if (ConfigModel.Misc.UseCustomOpenAL && File.Exists(ConfigModel.Misc.CustomOpenALPath))
                nativeLibraries.Add(ConfigModel.Misc.CustomOpenALPath);

            // Set up the game instance with the provided details
            MinecraftInstance? gameInstance = null;
            var settings = await LauncherHelper.GetLauncherSettingsAsync();
            var gameDetails = new GameDetails(
                ConfigModel.Java.JavaPath,
                ConfigModel.Java.MinMemory,
                ConfigModel.Java.MaxMemory,
                ConfigModel.Java.JvmArguments,
                MinecraftVersion,
                Kind,
                CustomVersion,
                GameDirectory,
                ConfigModel.Commands.PreLaunchCommand,
                wrapperCommand,
                ConfigModel.Commands.PostExitCommand,
                envDic,
                !string.IsNullOrEmpty(serverAddress) ?
                    serverAddress 
                    : ConfigModel.Misc.JoinServerOnLaunch ? ConfigModel.Misc.ServerAddress : null
            );
            var launcherDetails = new LauncherDetails("KonkordLauncher", App.Version);
            var clientDetails = new ClientDetails(
                account.AccessToken,
                account.DisplayName,
                account.Uuid,
                account.Type !=
                EAccountType
                    .MICROSOFT // Might support custom login services in the future, that's why I do not use EAccountType.OFFLINE
            );
            var resolution = new Resolution(
                ConfigModel.Game.StartMaximized
                    ? (uint)App.ScreenSize.Width
                    : ConfigModel.Game.WindowWidth,
                ConfigModel.Game.StartMaximized
                    ? (uint)App.ScreenSize.Height
                    : ConfigModel.Game.WindowHeight
            );
            switch (Kind)
            {
                case EMinecraftKind.VANILLA:
                {
                    gameInstance = new MinecraftInstance(
                        gameDetails,
                        new PathDetails(
                            settings.Launcher.AssetsDirectoryPath,
                            settings.Launcher.CacheDirectoryPath,
                            settings.Launcher.LibrariesDirectoryPath,
                            settings.Launcher.VersionsDirectoryPath,
                            settings.Launcher.GetVanillaManifestPath(),
                            null,
                            nativeLibraries
                        ),
                        launcherDetails,
                        clientDetails,
                        resolution,
                        this
                    );
                    break;
                }
                case EMinecraftKind.NEOFORGE:
                {
                    gameInstance = new NeoForgeInstance(
                        gameDetails,
                        new PathDetails(
                            settings.Launcher.AssetsDirectoryPath,
                            settings.Launcher.CacheDirectoryPath,
                            settings.Launcher.LibrariesDirectoryPath,
                            settings.Launcher.VersionsDirectoryPath,
                            settings.Launcher.GetVanillaManifestPath(),
                            settings.Launcher.GetNeoForgeManifestPath(),
                            nativeLibraries
                        ),
                        launcherDetails,
                        clientDetails,
                        resolution,
                        this
                    );
                    break;
                }
                case EMinecraftKind.FORGE:
                {
                    gameInstance = ForgeInstance.GetForgeInstance(
                        gameDetails,
                        new PathDetails(
                            settings.Launcher.AssetsDirectoryPath,
                            settings.Launcher.CacheDirectoryPath,
                            settings.Launcher.LibrariesDirectoryPath,
                            settings.Launcher.VersionsDirectoryPath,
                            settings.Launcher.GetVanillaManifestPath(),
                            settings.Launcher.GetForgeManifestPath(),
                            nativeLibraries
                        ),
                        launcherDetails,
                        clientDetails,
                        resolution,
                        this
                    );
                    break;
                }
                case EMinecraftKind.FABRIC:
                {
                    gameInstance = new FabricInstance(
                        gameDetails,
                        new PathDetails(
                            settings.Launcher.AssetsDirectoryPath,
                            settings.Launcher.CacheDirectoryPath,
                            settings.Launcher.LibrariesDirectoryPath,
                            settings.Launcher.VersionsDirectoryPath,
                            settings.Launcher.GetVanillaManifestPath(),
                            settings.Launcher.GetFabricManifestPath(),
                            nativeLibraries
                        ),
                        launcherDetails,
                        clientDetails,
                        resolution,
                        this
                    );
                    break;
                }
                case EMinecraftKind.QUILT:
                {
                    gameInstance = new QuiltInstance(
                        gameDetails,
                        new PathDetails(
                            settings.Launcher.AssetsDirectoryPath,
                            settings.Launcher.CacheDirectoryPath,
                            settings.Launcher.LibrariesDirectoryPath,
                            settings.Launcher.VersionsDirectoryPath,
                            settings.Launcher.GetVanillaManifestPath(),
                            settings.Launcher.GetQuiltManifestPath(),
                            nativeLibraries
                        ),
                        launcherDetails,
                        clientDetails,
                        resolution,
                        this
                    );
                    break;
                }
            }

            if (gameInstance == null)
                return;

            gameInstance.OnSetupDefaultJava += meta => SetupDefaultJavaPath(gameInstance, meta, settings, showAlertDialog);

            var process = await gameInstance.Start();
            if (process == null)
            {
                _logger.Error("Failed to launch the  Process is null.");
                return;
            }

            GameProcess = process;
            IsGameRunning = true;
            AttachProcessEvent();

            if (ConfigModel.Game.ShowConsoleWhileGameRunning)
            {
                await showLogsWindow.Handle(Id);
            }
            
            if (settings.Minecraft.CloseLauncherOnGameStart)
            {
                closeWindow.Handle(Unit.Default);
                return;
            }

            if (settings.Minecraft.CloseLauncherOnGameExit)
            {
                GameProcess.Exited += (_, e) =>
                {
                    if (ConfigModel.Game.ShowConsoleWhenGameCrashes && GameProcess?.ExitCode != 0)
                    {
                        Dispatcher.UIThread.Invoke(async () => await showLogsWindow.Handle(Id));
                    }
                    else if (ConfigModel.Game.CloseConsoleOnGameExit)
                    {
                        //TODO
                    }
                    closeWindow.Handle(Unit.Default);
                };
            }
        }
        catch (Exception ex)
        {
            _logger.Exc($"Failed to launch the {Name} ");
            _logger.Error(ex);
        }
    }
    
    /// <summary>
    /// Sets up the default Java path for the given Minecraft instance. If the required Java version
    /// is not available, it attempts to handle the situation by either downloading it or notifying the user.
    /// </summary>
    /// <param name="gameInstance">The Minecraft instance for which the Java path is being set up.</param>
    /// <param name="meta">The metadata containing the required Java version information.</param>
    /// <param name="settings">The core configuration settings of the launcher.</param>
    /// <param name="showAlertDialog">
    /// An interaction to display an alert dialog in case the required Java version is not found.
    /// </param>
    private void SetupDefaultJavaPath(MinecraftInstance gameInstance, VersionMeta? meta, CoreConfig settings, Interaction<Alert, Unit> showAlertDialog)
    {
        string defaultJavaPath = settings.Java.JavaPath;
        var instances = LauncherHelper.GetInstances();
        var instanceIndex = instances.FindIndex(x => x.Id == Id);

        if (meta == null)
        {
            UpdateJavaPath(gameInstance, defaultJavaPath, instances, instanceIndex);
            return;
        }
        
        // Check if the Java version specified in the metadata is available, if not attempt to download it
        var javaInstallations = JavaHelper.LocateJavaInstallations(settings.Launcher.JavaDirectoryPath);
        if (javaInstallations.All(x => x.Major != meta.JavaVersionMeta.MajorVersion) && string.IsNullOrEmpty(defaultJavaPath))
        {
            if (IsGameRunning && GameProcess != null)
                GameProcess.Kill();

            showAlertDialog.Handle(new Alert(TranslationManager.Translate("instance.java.notfound.title", meta.JavaVersionMeta.MajorVersion),
                TranslationManager.Translate("instance.java.notfound.message", meta.JavaVersionMeta.MajorVersion),
                EAlertType.Warning)).Wait();
            return;
        }

        foreach (var javaInstallation in javaInstallations)
        {
            if (meta.JavaVersionMeta != null && javaInstallation.Major == meta.JavaVersionMeta.MajorVersion)
            {
                defaultJavaPath = javaInstallation.Path;
                break;
            }
        }
        UpdateJavaPath(gameInstance, defaultJavaPath, instances, instanceIndex);
    }
    
    /// <summary>
    /// Updates the Java path for the game instance and saves the updated configuration.
    /// </summary>
    /// <param name="gameInstance">The game instance to update.</param>
    /// <param name="javaPath">The new Java path to set.</param>
    /// <param name="instances">The list of instances to update.</param>
    /// <param name="instanceIndex">The index of the current instance in the list.</param>
    private void UpdateJavaPath(MinecraftInstance gameInstance, string javaPath, List<Common.Models.Instance> instances, int instanceIndex)
    {
        gameInstance.UpdateJavaPath(javaPath);

        if (instanceIndex >= 0)
        {
            instances[instanceIndex].Config.Java.JavaPath = javaPath;
            JsonHelper.WriteJsonFile(PathHelper.LauncherInstancesPath, instances);
        }
        GlobalEvents.InvokeInstancesChanged();
    }

    /// <summary>
    /// Handles the event when the log file is changed. Reads the content of the updated log file.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data containing information about the changed file.</param>
    private void OnLogFileChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            if (e.Name != "latest.log")
                return;
            
            if (e.ChangeType != WatcherChangeTypes.Changed)
                return;

            // This will ensure that no sensitive data is read while the file is being written to
            Task.Delay(100).Wait(); // Wait a bit to ensure the file is ready to be read
            
            using var fs = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            fs.Seek(_lastReadPosition, SeekOrigin.Begin);
            using var sr = new StreamReader(fs);
            if (_lastReadPosition == 0)
                GlobalEvents.CleareInstanceLogs(Id);
            
            var newLines = new StringBuilder();
            while (sr.ReadLine() is { } newLine)
                newLines.AppendLine(newLine);
            
            Dispatcher.UIThread.Post(() =>
            {
                string logs = string.Join("\n", newLines);
                GlobalEvents.InvokeInstanceLogged(Id, logs);
            });
            _lastReadPosition = fs.Position;
        }
        catch (IOException ex)
        {
            _logger.Exc("Error while reading latest log file:");
            _logger.Error(ex);
        }
    }
    
    #region Progress Reporter
    private InstallWindow? _instanceInstallWindow;
    
    /// <summary>
    /// Sets the progress value for the installation window. If the window is not open, it will be shown.
    /// </summary>
    /// <param name="progress">The progress value to set, typically between 0.0 and 1.0.</param>
    public void SetProgress(double progress)
    {
        if (_instanceInstallWindow == null)
            Show();
    
        _instanceInstallWindow?.SetProgress(progress);
    }

    /// <summary>
    /// Sets the status message for the installation window. If the window is not open, it will be shown.
    /// </summary>
    /// <param name="status">The status message to display.</param>
    public void SetStatus(string status)
    {
        if (_instanceInstallWindow == null)
            Show();
    
        _instanceInstallWindow?.SetStatus(status);
    }

    /// <summary>
    /// Sets a translated status message for the installation window. If the window is not open, it will be shown.
    /// </summary>
    /// <param name="statusKey">The translation key for the status message.</param>
    /// <param name="args">Optional arguments to format the translated message.</param>
    public void SetStatusTranslated(string statusKey, params object[]? args)
    {
        if (_instanceInstallWindow == null)
            Show();
    
        _instanceInstallWindow?.SetStatusTranslated(statusKey, args);
    }

    /// <summary>
    /// Displays the installation window as a modal dialog. If the window is already open, this method does nothing.
    /// </summary>
    public void Show()
    {
        if (_instanceInstallWindow != null)
            return;
    
        _instanceInstallWindow = new InstallWindow();
        _instanceInstallWindow.Show();
    }

    /// <summary>
    /// Hides the installation window if it is currently open.
    /// </summary>
    public void Hide()
    {
        if (_instanceInstallWindow == null)
            return;
    
        _instanceInstallWindow.Close();
        _instanceInstallWindow = null;
    }
    #endregion
}