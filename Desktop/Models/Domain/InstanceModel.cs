using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Common.Models.InstanceConfig;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.Platform;
using Tavstal.KonkordLauncher.Core.Instances;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.Instance;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

namespace Tavstal.KonkordLauncher.Desktop.Models.Domain;

/// <summary>
/// Represents a model for a Minecraft instance, including its properties and behaviors.
/// </summary>
public partial class InstanceModel : ObservableObject, IProgressReporter
{
    private readonly ICustomLogger _logger;
    private readonly ILauncherStore _launcherStore;
    private readonly IManifestService _manifestService;
    private readonly IBitmapService _bitmapService;
    private readonly ITranslationService _translationService;
    private readonly IJavaService _javaService;
    private readonly IInstanceInstallService _installService;
    private readonly IInstanceLaunchService _launchService;
    private long _lastReadPosition;
    private FileSystemWatcher? _watcher;

    #region Observable Properties
    /// <summary>
    /// Gets or sets the unique identifier of the instance.
    /// </summary>
    [ObservableProperty]
    public partial string Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the instance.
    /// </summary>
    [ObservableProperty]
    public partial string Name { get; set; }

    /// <summary>
    /// Gets or sets the group to which the instance belongs.
    /// </summary>
    [ObservableProperty]
    public partial string? Group { get; set; }

    /// <summary>
    /// Gets or sets the file path to the icon of the instance.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Icon))]
    public partial string? IconPath { get; set; }

    /// <summary>
    /// Gets or sets the Minecraft version associated with the instance.
    /// </summary>
    [ObservableProperty]
    public partial string MinecraftVersion { get; set; }

    /// <summary>
    /// Gets or sets the custom version of the instance, if any.
    /// </summary>
    [ObservableProperty]
    public partial string CustomVersion { get; set; }

    /// <summary>
    /// Gets or sets the kind of Minecraft associated with the instance.
    /// </summary>
    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsModded))]
    public partial EMinecraftKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the custom game directory for the instance, if specified.
    /// </summary>
    [ObservableProperty]
    public partial string? GameDirectory { get; set; }

    /// <summary>
    /// Gets or sets the configuration of the instance.
    /// </summary>
    [ObservableProperty]
    public partial InstanceConfig ConfigModel { get; set; }

    /// <summary>
    /// Gets or sets the process associated with the running game.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGameRunning))]
    public partial Process? GameProcess { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the game is currently running.
    /// </summary>
    [ObservableProperty]
    public partial bool IsGameRunning { get; set; }
    
    /// <summary>
    /// Gets the icon of the instance as a bitmap. If the icon path is not set, a default icon is used.
    /// </summary>
    [ObservableProperty]
    public partial BitmapEntry Icon { get; set; }
    #endregion

    public bool IsModded => Kind != EMinecraftKind.VANILLA;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceModel"/> class.
    /// </summary>
    public InstanceModel()
    {
        var services = Program.ServiceProvider;
        _logger = services.GetRequiredService<ICustomLogger<InstanceModel>>();
        _launcherStore = services.GetRequiredService<ILauncherStore>();
        _manifestService = services.GetRequiredService<IManifestService>();
        _translationService = services.GetRequiredService<ITranslationService>();
        _javaService = services.GetRequiredService<IJavaService>();
        _installService = services.GetRequiredService<IInstanceInstallService>();
        _launchService = services.GetRequiredService<IInstanceLaunchService>();
        _bitmapService = services.GetRequiredService<IBitmapService>();
        
        Icon = string.IsNullOrEmpty(IconPath) ? _bitmapService.GetBitmap("avares://KonkordLauncher/Assets/Icons/dirt.png") 
                : _bitmapService.GetBitmap(IconPath);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceModel"/> class using the specified common instance model.
    /// </summary>
    /// <param name="instance">The common instance model to initialize from.</param>
    public InstanceModel(Common.Models.Instance instance) : this()
    {
        Id = instance.Id;
        Name = instance.Name;
        Group = instance.Group;
        IconPath = instance.IconPath;
        if (!string.IsNullOrEmpty(IconPath))
            Icon = _bitmapService.GetBitmap(IconPath);
        MinecraftVersion = instance.MinecraftVersion;
        CustomVersion = instance.CustomVersion;
        Kind = instance.Kind;
        GameDirectory = instance.GameDirectory;
        ConfigModel = instance.Config;
    }
    
    /// <summary>
    /// Converts the current instance model into a common instance model.
    /// </summary>
    /// <returns>
    /// A new instance of <see cref="Common.Models.Instance"/> populated with the properties
    /// of the current instance model.
    /// </returns>
    public Common.Models.Instance getInstance()
    {
        return new Common.Models.Instance
        {
            Id = Id,
            Name = Name,
            Group = Group,
            IconPath = IconPath,
            MinecraftVersion = MinecraftVersion,
            CustomVersion = CustomVersion,
            Kind = Kind,
            GameDirectory = GameDirectory,
            Config = ConfigModel
        };
    }

    /// <summary>
    /// Updates this view-model instance from a common instance data model.
    /// </summary>
    /// <param name="newData">
    /// The source instance data to copy from. The update is only applied when the IDs match.
    /// </param>
    public void UpdateDetails(Common.Models.Instance newData)
    {
        if (newData.Id != Id)
            return;
        
        Name = newData.Name;
        Group = newData.Group;
        IconPath = newData.IconPath;
        if (Icon.Key != null)
            Icon.Dispose(_bitmapService);
        if (!string.IsNullOrEmpty(IconPath))
            Icon = _bitmapService.GetBitmap(IconPath);
        MinecraftVersion = newData.MinecraftVersion;
        CustomVersion = newData.CustomVersion;
        Kind = newData.Kind;
        GameDirectory = newData.GameDirectory;
        ConfigModel = newData.Config;
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
            Dispatcher.UIThread.Post(() =>
            {
                _logger.LogDebug($"The instance has exited.");
                GameProcess = null;
                IsGameRunning = false;
            });
        };

        GameProcess.Disposed += (_, _) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                _logger.LogDebug($"The instance process has been disposed.");
                GameProcess = null;
                IsGameRunning = false;
            });
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
        
        Directory.CreateDirectory(logsDir);
        
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
            _logger.LogError("Watcher error: " + e.GetException());
        };
    }

    /// <summary>
    /// Launches this Minecraft instance.
    /// </summary>
    /// <param name="showLogsWindow">
    /// Interaction used to request showing the logs/console window for this instance. The view-model
    /// will call <c>Handle</c> on this interaction when it wants the UI to open the logs' dialog.
    /// </param>
    /// <param name="closeLogsWindow">Interaction used to request closing the logs/console window for this instance.</param>
    /// <param name="closeWindow">Interaction used to request closing the launcher main window (used when the launcher is configured to close on game start/exit).</param>
    /// <param name="showAlertDialog">Interaction used to show an alert dialog to the user (e.g. when no account is selected or Java is missing).</param>
    /// <param name="serverAddress">Optional server address to connect to on launch.</param>
    /// <returns>A task that completes when the launch sequence has finished (not when the game exits).</returns>
    public async Task LaunchAsync(Interaction<string, Unit> showLogsWindow, Interaction<string, Unit> closeLogsWindow, Interaction<Unit, Unit> closeWindow, Interaction<Alert, Unit> showAlertDialog, string? serverAddress = null)
    {
        _lastReadPosition = 0;
        var accountData = await _launcherStore.GetAccountDataAsync();
        var account = ConfigModel.Misc.OverrideAccount ? 
            accountData.Accounts.FirstOrDefault(x => x.Id == ConfigModel.Misc.AccountId) 
            : accountData.Accounts.FirstOrDefault(x => x.Id == accountData.SelectedAccountId);
        
        if (account == null)
        {
            await showAlertDialog.Handle(new Alert(_translationService.Translate("account.none.title"), _translationService.Translate("account.none.message"), EAlertType.Warning));
            return;
        }

        try
        {
            List<string> command = [];
            var customCommands =  ConfigModel.Commands.WrapperCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Add gamemoderun if enabled
            if (ConfigModel.Game.EnableFeralGameMode && !ConfigModel.Commands.WrapperCommand.Contains("gamemoderun"))
                command.Add("gamemoderun");

            // Add mangohud if enabled
            if (ConfigModel.Game.EnableMangoHud && !ConfigModel.Commands.WrapperCommand.Contains("mangohud"))
                command.Add("mangohud");

            foreach (var cc in customCommands)
                command.Add(cc);

            // Attempt to force the use of a dedicated GPU if configured
            var environmentVariables = ConfigModel.EnableEnvironment
                ? ConfigModel.Environment
                : [];
            var gpuInfo = OSHelper.GetDedicatedGpuType();
            if (ConfigModel.Game.UseDedicatedGpu && gpuInfo != null)
            {
                switch (OSHelper.GetOperatingSystem())
                {
                    case EOperatingSystem.WINDOWS:
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
                    case EOperatingSystem.LINUX:
                    {
                        switch (gpuInfo.Value.Item1)
                        {
                            case "amd":
                            {
                                environmentVariables.Add(new("DRI_PRIME", "1"));
                                environmentVariables.Add(new("LIBVA_DRIVER_NAME", "radeonsi"));
                                environmentVariables.Add(new("VDPAU_DRIVER", "radeonsi"));
                                break;
                            }
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
            var settings = await _launcherStore.GetSettingsAsync();
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
                command,
                ConfigModel.Commands.PostExitCommand,
                envDic,
                !string.IsNullOrEmpty(serverAddress) ?
                    serverAddress 
                    : ConfigModel.Misc.JoinServerOnLaunch ? ConfigModel.Misc.ServerAddress : null
            );
            var launcherDetails = new LauncherDetails("KonkordLauncher", App.Version);
            var clientDetails = new ClientDetails(
                account.GetAccessToken(),
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

            var minecraftManifest = await _manifestService.GetMinecraftManifestAsync(settings.Launcher.GetVanillaManifestPath());
            if (minecraftManifest == null)
                throw new InvalidOperationException("Failed to load Minecraft manifest.");
            
            var minecraftVersion = minecraftManifest.Versions.FirstOrDefault(x => x.Id == gameDetails.MinecraftVersion);
            if (minecraftVersion == null)
                throw new InvalidOperationException($"Minecraft version {gameDetails.MinecraftVersion} not found in manifest.");
            
            gameInstance = Kind switch
            {
                EMinecraftKind.VANILLA => new MinecraftInstance(Id, minecraftVersion, gameDetails,
                    new PathDetails(settings.Launcher.AssetsDirectoryPath, settings.Launcher.CacheDirectoryPath,
                        settings.Launcher.LibrariesDirectoryPath, settings.Launcher.VersionsDirectoryPath,
                        settings.Launcher.GetVanillaManifestPath(), null, nativeLibraries), launcherDetails,
                    clientDetails, new CustomLogger<MinecraftInstance>(_logger.GetLogLevel()), resolution, this),
                EMinecraftKind.NEOFORGE => new NeoForgeInstance(Id, minecraftVersion, gameDetails,
                    new PathDetails(settings.Launcher.AssetsDirectoryPath, settings.Launcher.CacheDirectoryPath,
                        settings.Launcher.LibrariesDirectoryPath, settings.Launcher.VersionsDirectoryPath,
                        settings.Launcher.GetVanillaManifestPath(), settings.Launcher.GetNeoForgeManifestPath(),
                        nativeLibraries), launcherDetails, clientDetails, new CustomLogger<NeoForgeInstance>(_logger.GetLogLevel()), resolution, this),
                EMinecraftKind.FORGE => ForgeInstance.GetForgeInstance(Id, minecraftVersion, gameDetails,
                    new PathDetails(settings.Launcher.AssetsDirectoryPath, settings.Launcher.CacheDirectoryPath,
                        settings.Launcher.LibrariesDirectoryPath, settings.Launcher.VersionsDirectoryPath,
                        settings.Launcher.GetVanillaManifestPath(), settings.Launcher.GetForgeManifestPath(),
                        nativeLibraries), launcherDetails, clientDetails, new CustomLogger<MinecraftInstance>(_logger.GetLogLevel()),  resolution, this),
                EMinecraftKind.FABRIC => new FabricInstance(Id, minecraftVersion, gameDetails,
                    new PathDetails(settings.Launcher.AssetsDirectoryPath, settings.Launcher.CacheDirectoryPath,
                        settings.Launcher.LibrariesDirectoryPath, settings.Launcher.VersionsDirectoryPath,
                        settings.Launcher.GetVanillaManifestPath(), settings.Launcher.GetFabricManifestPath(),
                        nativeLibraries), launcherDetails, clientDetails,  new CustomLogger<FabricInstance>(_logger.GetLogLevel()), resolution, this),
                EMinecraftKind.QUILT => new QuiltInstance(Id, minecraftVersion, gameDetails,
                    new PathDetails(settings.Launcher.AssetsDirectoryPath, settings.Launcher.CacheDirectoryPath,
                        settings.Launcher.LibrariesDirectoryPath, settings.Launcher.VersionsDirectoryPath,
                        settings.Launcher.GetVanillaManifestPath(), settings.Launcher.GetQuiltManifestPath(),
                        nativeLibraries), launcherDetails, clientDetails, new CustomLogger<QuiltInstance>(_logger.GetLogLevel()), resolution, this),
                _ => gameInstance
            };

            if (gameInstance == null)
                return;
            
            gameInstance.OnSetupDefaultJava += meta => Dispatcher.UIThread.Invoke(async () => await SetupDefaultJavaPathAsync(gameInstance, meta, settings, showAlertDialog));

            if (!await _installService.InstallAsync(gameInstance, this))
            {
                _logger.LogWarning("Installation failed or was cancelled.");
                await showAlertDialog.Handle(new Alert(
                    _translationService.Translate("instance.launch.install_failed.title"), 
                    _translationService.Translate("instance.launch.install_failed.message"), 
                    EAlertType.Error));
                return;
            }
            
            var process = await _launchService.LaunchAsync(gameInstance, this);
            if (process == null)
            {
                _logger.LogError("Failed to launch the  Process is null.");
                return;
            }

            GameProcess = process;
            AttachProcessEvent();
            App.ClearRPC();

            if (ConfigModel.Game.ShowConsoleWhileGameRunning)
            {
                await showLogsWindow.Handle(Id);
            }
            
            if (settings.Minecraft.CloseLauncherOnGameStart)
            {
                await closeWindow.Handle(Unit.Default);
                return;
            }

            GameProcess.Exited += (_, _) =>
            {
                App.UpdateRPC("Browsing instances...");
                if (settings.Minecraft.CloseLauncherOnGameExit)
                {
                    Dispatcher.UIThread.Invoke<Task>(async () =>
                    {
                        if (ConfigModel.Game.ShowConsoleWhenGameCrashes && GameProcess?.ExitCode != 0)
                            await showLogsWindow.Handle(Id);
                        else if (ConfigModel.Game.CloseConsoleOnGameExit)
                            await closeLogsWindow.Handle(Id);

                        await closeWindow.Handle(Unit.Default);
                    });
                }
            };
            IsGameRunning = !process.HasExited;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to launch the {Name}");
        }
    }
    
    /// <summary>
    /// Sets up the default Java path for the given Minecraft instance. If the required Java version
    /// is not available, it attempts to handle the situation by either downloading it or notifying the user.
    /// </summary>
    /// <param name="gameInstance">The Minecraft instance for which the Java path is being set up.</param>
    /// <param name="meta">The metadata containing the required Java version information.</param>
    /// <param name="settings">The core configuration settings of the launcher.</param>
    /// <param name="showAlertDialog">An interaction to display an alert dialog in case the required Java version is not found.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    private async Task SetupDefaultJavaPathAsync(MinecraftInstance gameInstance, VersionMeta? meta, CoreConfig settings, Interaction<Alert, Unit> showAlertDialog, CancellationToken cancellationToken = default)
    {
        try
        {
            string defaultJavaPath = settings.Java.JavaPath;
            var instances = await _launcherStore.GetInstancesAsync(cancellationToken);
            var instanceIndex = instances.FindIndex(x => x.Id == Id);

            if (meta == null)
            {
                await UpdateJavaPathAsync(gameInstance, defaultJavaPath, instances, instanceIndex);
                return;
            }

            // Check if the Java version specified in the metadata is available, if not attempt to download it
            var javaInstallations = await _javaService.LocateJavaInstallationsAsync(settings.Launcher.JavaDirectoryPath, cancellationToken: cancellationToken);
            if (javaInstallations.All(x => x.Major != meta.JavaVersionMeta.MajorVersion) &&
                string.IsNullOrEmpty(defaultJavaPath))
            {
                if (IsGameRunning && GameProcess != null)
                    GameProcess.Kill();
                
                await showAlertDialog.Handle(new Alert(
                    _translationService.Translate("instance.java.notfound.title", meta.JavaVersionMeta.MajorVersion),
                    _translationService.Translate("instance.java.notfound.message", meta.JavaVersionMeta.MajorVersion),
                    EAlertType.Warning));
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

            await UpdateJavaPathAsync(gameInstance, defaultJavaPath, instances, instanceIndex);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error while setting up default Java path:");
        }
    }
    
    /// <summary>
    /// Updates the Java path for the game instance and saves the updated configuration.
    /// </summary>
    /// <param name="gameInstance">The game instance to update.</param>
    /// <param name="javaPath">The new Java path to set.</param>
    /// <param name="instances">The list of instances to update.</param>
    /// <param name="instanceIndex">The index of the current instance in the list.</param>
    private async Task UpdateJavaPathAsync(MinecraftInstance gameInstance, string javaPath, List<Common.Models.Instance> instances, int instanceIndex)
    {
        gameInstance.UpdateJavaPath(javaPath);

        if (instanceIndex >= 0)
        {
            instances[instanceIndex].Config.Java.JavaPath = javaPath;
            await _launcherStore.SaveInstancesAsync(instances);
        }
        GlobalEvents.InvokeInstanceUpdated(instances[instanceIndex].Id);
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
            Task.Delay(100).Wait();
            
            using var fs = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            fs.Seek(_lastReadPosition, SeekOrigin.Begin);
            using var sr = new StreamReader(fs);
            if (_lastReadPosition == 0)
                GlobalEvents.CleareInstanceLogs(Id);
            
            var newLines = new StringBuilder();
            while (sr.ReadLine() is { } newLine)
                newLines.AppendLine(newLine);
            
            string logs = string.Join("\n", newLines);
            GlobalEvents.InvokeInstanceLogged(Id, logs);
            _lastReadPosition = fs.Position;
        }
        catch (IOException ex)
        {
            _logger.LogCritical(ex, "Error while reading latest log file:");
        }
    }
    
    #region Progress Reporter
    private ProgressWindow? _instanceInstallWindow;

    /// <summary>
    /// Sets the progress value for the installation window. If the window is not open, it will be shown.
    /// </summary>
    /// <param name="progress">The progress value to set, typically between 0.0 and 1.0.</param>
    public void ReportProgress(double progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_instanceInstallWindow == null)
                OpenReporter();

            _instanceInstallWindow?.ReportProgress(progress);
        });
    }

    /// <summary>
    /// Sets the status message for the installation window. If the window is not open, it will be shown.
    /// </summary>
    /// <param name="status">The status message to display.</param>
    public void UpdateStatus(string status)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_instanceInstallWindow == null)
                OpenReporter();

            _instanceInstallWindow?.UpdateStatus(status);
        });
    }

    /// <summary>
    /// Sets a translated status message for the installation window. If the window is not open, it will be shown.
    /// </summary>
    /// <param name="key">The translation key for the status message.</param>
    /// <param name="args">Optional arguments to format the translated message.</param>
    public void UpdateStatusTranslated(string key, params object[]? args)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_instanceInstallWindow == null)
                OpenReporter();

            _instanceInstallWindow?.UpdateStatusTranslated(key, args);
        });
    }

    /// <summary>
    /// Displays the installation window as a modal dialog. If the window is already open, this method does nothing.
    /// </summary>
    public void OpenReporter()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_instanceInstallWindow != null)
                return;

            _instanceInstallWindow = new ProgressWindow();
            _instanceInstallWindow.Show();
        });
    }

    /// <summary>
    /// Hides the installation window if it is currently open.
    /// </summary>
    public void CloseReporter()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_instanceInstallWindow == null)
                return;

            _instanceInstallWindow.Close();
            _instanceInstallWindow = null;
        });
    }

    #endregion
}