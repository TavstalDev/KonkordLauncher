using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.BeaconSDK.Enums;
using Tavstal.BeaconSDK.Instances;
using Tavstal.BeaconSDK.Models;
using Tavstal.BeaconSDK.Models.Config;
using Tavstal.BeaconSDK.Models.Instance;
using Tavstal.BeaconSDK.Models.Instance.Config;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Constants;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs.InstanceConfig;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Services;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

namespace Tavstal.KonkordLauncher.Desktop.Models.Domain;

public partial class InstanceModel : ObservableObject, IKonkordProgress
{
    private readonly BeaconConfiguration _beaconConfiguration;
    private readonly IBitmapService _bitmapService;
    private readonly IDialogService _dialogService;
    private readonly IEncryptionService _encryptionService;
    private readonly IEventService _eventService;
    private readonly IInstanceInstallService _installService;
    private readonly IJavaService _javaService;
    private readonly IInstanceLaunchService _launchService;
    private readonly ILauncherStore _launcherStore;
    private readonly IBeaconLogger _logger;
    private readonly IManifestService _manifestService;
    private readonly IScreenService _screenService;
    private readonly ITranslationService _translationService;
    private readonly IWindowFactory _windowFactory;
    private long _lastReadPosition;
    private FileSystemWatcher? _watcher;

    public InstanceModel(DTOs.Instance instance, IBeaconLogger<InstanceModel> logger, ILauncherStore launcherStore,
        IManifestService manifestService, IBitmapService bitmapService, ITranslationService translationService,
        IJavaService javaService, IInstanceInstallService installService, IInstanceLaunchService launchService,
        IEventService eventService, IEncryptionService encryptionService, BeaconConfiguration beaconConfiguration,
        IScreenService screenService, IWindowFactory windowFactory, IDialogService dialogService)
    {
        _logger = logger;
        _launcherStore = launcherStore;
        _manifestService = manifestService;
        _bitmapService = bitmapService;
        _translationService = translationService;
        _javaService = javaService;
        _installService = installService;
        _launchService = launchService;
        _eventService = eventService;
        _encryptionService = encryptionService;
        _beaconConfiguration = beaconConfiguration;
        _screenService = screenService;
        _windowFactory = windowFactory;
        _dialogService = dialogService;

        Id = instance.Id;
        Name = instance.Name;
        Group = instance.Group;
        IconPath = instance.IconPath;
        Icon = !string.IsNullOrEmpty(IconPath) ?
            _bitmapService.GetBitmap(IconPath, BitmapSizes.InstanceModel)
            : bitmapService.GetBitmap("avares://KonkordLauncher/Assets/Icons/dirt.png", BitmapSizes.InstanceModel);
        MinecraftVersion = instance.MinecraftVersion;
        CustomVersion = instance.CustomVersion;
        Kind = instance.Kind;
        GameDirectory = instance.GameDirectory;
        ConfigModel = instance.Config;
    }

    public bool IsModded => Kind != EMinecraftKind.VANILLA;

    /// <summary>
    ///     Converts the current instance model into a common instance model.
    /// </summary>
    /// <returns>
    ///     A new instance of <see cref="Instance" /> populated with the properties
    ///     of the current instance model.
    /// </returns>
    public DTOs.Instance getInstance()
    {
        return new DTOs.Instance
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
    ///     Updates this view-model instance from a common instance data model.
    /// </summary>
    /// <param name="newData">
    ///     The source instance data to copy from. The update is only applied when the IDs match.
    /// </param>
    public void UpdateDetails(DTOs.Instance newData)
    {
        if (newData.Id != Id)
            return;

        Name = newData.Name;
        Group = newData.Group;
        IconPath = newData.IconPath;
        if (Icon.Key != null)
            Icon.Dispose(_bitmapService);
        if (!string.IsNullOrEmpty(IconPath))
            Icon = _bitmapService.GetBitmap(IconPath, BitmapSizes.InstanceModel);
        MinecraftVersion = newData.MinecraftVersion;
        CustomVersion = newData.CustomVersion;
        Kind = newData.Kind;
        GameDirectory = newData.GameDirectory;
        ConfigModel = newData.Config;
    }

    /// <summary>
    ///     Attaches event handlers to the game process to handle its exit and disposal events.
    /// </summary>
    public void AttachProcessEvent()
    {
        if (GameProcess == null)
            return;

        GameProcess.Exited += (_, _) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                _logger.LogDebug("The instance has exited.");
                GameProcess = null;
                IsGameRunning = false;
            });
        };

        GameProcess.Disposed += (_, _) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                _logger.LogDebug("The instance process has been disposed.");
                GameProcess = null;
                IsGameRunning = false;
            });
        };

        if (string.IsNullOrEmpty(GameDirectory))
            return;

        var logsDir = Path.Combine(GameDirectory, "logs");

        if (_watcher != null)
        {
            _watcher.Changed -= OnLogFileChanged;
            _watcher.Created -= OnLogFileChanged;
            _watcher.Renamed -= OnLogFileChanged;
        }

        Directory.CreateDirectory(logsDir);

        _watcher = new FileSystemWatcher
        {
            Path = logsDir, NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
        };
        _watcher.EnableRaisingEvents = true;
        _watcher.Changed += OnLogFileChanged;
        _watcher.Created += OnLogFileChanged;
        _watcher.Error += (_, e) =>
        {
            _logger.LogError("Watcher error: " + e.GetException());
        };
    }

    public async Task LaunchAsync(string? serverAddress = null)
    {
        _lastReadPosition = 0;
        _cancellationTokenSource = new CancellationTokenSource();
        var cts = _cancellationTokenSource.Token;
        var accountData = await _launcherStore.GetAccountDataAsync(cts);
        var account = ConfigModel.Misc.OverrideAccount
            ? accountData.Accounts.FirstOrDefault(x => x.Id == ConfigModel.Misc.AccountId)
            : accountData.Accounts.FirstOrDefault(x => x.Id == accountData.SelectedAccountId);

        if (account == null)
        {
            await _dialogService.ShowAlertAsync(
                _translationService.Translate("account.none.title"),
                _translationService.Translate("account.none.message"),
                EAlertType.Warning);
            return;
        }

        try
        {
            List<string> command = [];
            var customCommands = ConfigModel.Commands.WrapperCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Add gamemoderun if enabled
            if (ConfigModel.Game.EnableFeralGameMode && !ConfigModel.Commands.WrapperCommand.Contains("gamemoderun"))
                command.Add("gamemoderun");

            // Add mangohud if enabled
            if (ConfigModel.Game.EnableMangoHud && !ConfigModel.Commands.WrapperCommand.Contains("mangohud"))
                command.Add("mangohud");

            foreach (var cc in customCommands)
                command.Add(cc);

            var envDic = new Dictionary<string, string>();
            if (ConfigModel.EnableEnvironment)
                foreach (var env in ConfigModel.Environment)
                    envDic[env.Key] = env.Value;

            // Add custom native libraries if configured
            List<string> nativeLibraries = [];
            if (ConfigModel.Misc.UseCustomGlfw && File.Exists(ConfigModel.Misc.CustomGlfwPath))
                nativeLibraries.Add(ConfigModel.Misc.CustomGlfwPath);
            if (ConfigModel.Misc.UseCustomOpenAL && File.Exists(ConfigModel.Misc.CustomOpenALPath))
                nativeLibraries.Add(ConfigModel.Misc.CustomOpenALPath);

            // Set up the game instance with the provided details
            MinecraftInstance? gameInstance = null;
            var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cts);
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
                !string.IsNullOrEmpty(serverAddress) ? serverAddress
                : ConfigModel.Misc.JoinServerOnLaunch ? ConfigModel.Misc.ServerAddress : null
            );
            var launcherDetails = new LauncherDetails("KonkordLauncher", Program.Version);
            var clientDetails = new ClientDetails(
                Encoding.UTF8.GetString(account.GetAccessToken(_encryptionService)),
                account.DisplayName,
                account.Uuid,
                account.Type !=
                EAccountType
                    .MICROSOFT // Might support custom login services in the future, that's why I do not use EAccountType.OFFLINE
            );
            var resolution = new Resolution(
                ConfigModel.Game.StartMaximized
                    ? (uint)_screenService.ScreenSize.Width
                    : ConfigModel.Game.WindowWidth,
                ConfigModel.Game.StartMaximized
                    ? (uint)_screenService.ScreenSize.Height
                    : ConfigModel.Game.WindowHeight
            );

            var minecraftManifest =
                await _manifestService.GetMinecraftManifestAsync(settings.Launcher.GetVanillaManifestPath(), cts);
            if (minecraftManifest == null)
                throw new InvalidOperationException("Failed to load Minecraft manifest.");

            var minecraftVersion = minecraftManifest.Versions.FirstOrDefault(x => x.Id == gameDetails.MinecraftVersion);
            if (minecraftVersion == null)
                throw new InvalidOperationException(
                    $"Minecraft version {gameDetails.MinecraftVersion} not found in manifest.");

            var configuration = new InstanceConfiguration
            {
                Version = minecraftVersion,
                Game = gameDetails,
                Path = new PathDetails(settings.Launcher.AssetsDirectoryPath, settings.Launcher.CacheDirectoryPath,
                    settings.Launcher.LibrariesDirectoryPath, settings.Launcher.VersionsDirectoryPath,
                    nativeLibraries),
                Launcher = launcherDetails,
                Client = clientDetails,
                Resolution = resolution
            };

            gameInstance = Kind switch
            {
                EMinecraftKind.VANILLA => new MinecraftInstance(Id, _beaconConfiguration, configuration, _logger, this),
                EMinecraftKind.NEOFORGE => new NeoForgeInstance(Id, _beaconConfiguration, configuration, _logger, this),
                EMinecraftKind.FORGE => ForgeInstance.GetForgeInstance(Id, _beaconConfiguration, configuration,
                    _logger, this),
                EMinecraftKind.FABRIC => new FabricInstance(Id, _beaconConfiguration, configuration, _logger, this),
                EMinecraftKind.QUILT => new QuiltInstance(Id, _beaconConfiguration, configuration, _logger, this),
                _ => gameInstance
            };

            if (gameInstance == null)
                return;

            cts.ThrowIfCancellationRequested();
            var stopwatch = new Stopwatch();
            if (!await _installService.InstallAsync(gameInstance, this, stopwatch, cts))
            {
                _logger.LogWarning("Installation failed or was cancelled.");
                await _dialogService.ShowAlertAsync(
                    _translationService.Translate("instance.launch.install_failed.title"),
                    _translationService.Translate("instance.launch.install_failed.message"),
                    EAlertType.Error);
                return;
            }

            CloseReporter();

            // Handle Java
            var requiredJavaVersion = gameInstance.MinecraftVersionMeta.JavaVersionMeta?.MajorVersion ?? 8;
            cts.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(gameInstance.Config.Game.JavaPath) ||
                string.IsNullOrEmpty(gameInstance.Config.Game.JavaPath))
            {
                var javaPath = await _javaService.GetJavaVersionAsync(requiredJavaVersion, cts);
                if (string.IsNullOrEmpty(javaPath))
                {
                    var alertResult = await _dialogService.ShowAlertAsync(
                        _translationService.Translate("instance.java.missing.title", requiredJavaVersion),
                        _translationService.Translate("instance.java.missing.message"),
                        EAlertType.Confirm
                    );

                    if (!alertResult)
                        return;

                    var prog = new Progress<double>(p =>
                    {
                        Report(p, "instance.download.file", $"java {requiredJavaVersion}",
                            p.ToString("0.00"));
                    });

                    var mirrorsPath = Path.Combine(settings.Launcher.CacheDirectoryPath, "javaMirrors.json");
                    cts.ThrowIfCancellationRequested();
                    OpenReporter();
                    javaPath = await _javaService.DownloadJavaVersionAsync(requiredJavaVersion,
                        settings.Launcher.JavaDirectoryPath, mirrorsPath, prog, cts);
                    CloseReporter();
                    if (string.IsNullOrEmpty(javaPath))
                    {
                        await _dialogService.ShowAlertAsync(
                            _translationService.Translate("instance.java.error.title", requiredJavaVersion),
                            _translationService.Translate("instance.java.error.message"),
                            EAlertType.Error
                        );
                        return;
                    }
                }

                gameInstance.Config.Game.JavaPath = javaPath;
                var instances = await _launcherStore.GetInstancesAsync(cts);
                var instanceIndex = instances.FindIndex(x => x.Id == Id);
                await UpdateJavaPathAsync(gameInstance, javaPath, instances, instanceIndex);
            }
            else
            {
                var javaDetails = await _javaService.GetJavaVersionDetailsAsync(gameInstance.Config.Game.JavaPath, cts);
                if (javaDetails == null || javaDetails.Major != requiredJavaVersion)
                {
                    await _dialogService.ShowAlertAsync(
                        _translationService.Translate("instance.java.notfound.title", requiredJavaVersion),
                        _translationService.Translate("instance.java.notfound.message", requiredJavaVersion),
                        EAlertType.Error);
                    return;
                }
            }

            cts.ThrowIfCancellationRequested();
            var process = await _launchService.LaunchAsync(gameInstance, this, cts);
            if (process == null)
            {
                _logger.LogError("Failed to launch the  Process is null.");
                return;
            }

            GameProcess = process;
            AttachProcessEvent();
            //App.ClearRPC();

            if (ConfigModel.Game.ShowConsoleWhileGameRunning)
                await _dialogService.ShowInstanceLogsAsync(Id);

            if (settings.Minecraft.CloseLauncherOnGameStart)
            {
                await _dialogService.CloseAsync();
                return;
            }

            GameProcess.Exited += (_, _) =>
            {
                if (settings.Minecraft.CloseLauncherOnGameExit)
                    Dispatcher.UIThread.Invoke<Task>(async () =>
                    {
                        if (ConfigModel.Game.ShowConsoleWhenGameCrashes && GameProcess?.ExitCode != 0)
                            await _dialogService.ShowInstanceLogsAsync(Id);
                        else if (ConfigModel.Game.CloseConsoleOnGameExit)
                            await _dialogService.CloseInstanceLogsAsync(Id);

                        await _dialogService.CloseAsync();
                    });
            };
            IsGameRunning = !process.HasExited;
        }
        catch (OperationCanceledException)
        {
            CloseReporter();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to launch the {Name}");
        }
    }

    /// <summary>
    ///     Updates the Java path for the game instance and saves the updated configuration.
    /// </summary>
    /// <param name="gameInstance">The game instance to update.</param>
    /// <param name="javaPath">The new Java path to set.</param>
    /// <param name="instances">The list of instances to update.</param>
    /// <param name="instanceIndex">The index of the current instance in the list.</param>
    private async Task UpdateJavaPathAsync(MinecraftInstance gameInstance, string javaPath,
        List<DTOs.Instance> instances, int instanceIndex)
    {
        gameInstance.Config.Game.JavaPath = javaPath;
        ConfigModel.Java.JavaPath = javaPath;
        if (instanceIndex < 0)
            return;

        instances[instanceIndex].Config.Java.JavaPath = javaPath;
        await _launcherStore.SaveInstancesAsync(instances);
        _eventService.RaiseInstanceUpdated(instances[instanceIndex].Id);
    }

    /// <summary>
    ///     Handles the event when the log file is changed. Reads the content of the updated log file.
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
                _eventService.CleareInstanceLogs(Id);

            var newLines = new StringBuilder();
            while (sr.ReadLine() is { } newLine)
                newLines.AppendLine(newLine);

            var logs = string.Join("\n", newLines);
            _eventService.RaiseInstanceLogged(Id, logs);
            _lastReadPosition = fs.Position;
        }
        catch (IOException ex)
        {
            _logger.LogCritical(ex, "Error while reading latest log file:");
        }
    }

    #region Observable Properties

    /// <summary>
    ///     Gets or sets the unique identifier of the instance.
    /// </summary>
    [ObservableProperty]
    public partial string Id { get; set; }

    /// <summary>
    ///     Gets or sets the name of the instance.
    /// </summary>
    [ObservableProperty]
    public partial string Name { get; set; }

    /// <summary>
    ///     Gets or sets the group to which the instance belongs.
    /// </summary>
    [ObservableProperty]
    public partial string? Group { get; set; }

    /// <summary>
    ///     Gets or sets the file path to the icon of the instance.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Icon))]
    public partial string? IconPath { get; set; }

    /// <summary>
    ///     Gets or sets the Minecraft version associated with the instance.
    /// </summary>
    [ObservableProperty]
    public partial string MinecraftVersion { get; set; }

    /// <summary>
    ///     Gets or sets the custom version of the instance, if any.
    /// </summary>
    [ObservableProperty]
    public partial string? CustomVersion { get; set; }

    /// <summary>
    ///     Gets or sets the kind of Minecraft associated with the instance.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsModded))]
    public partial EMinecraftKind Kind { get; set; }

    /// <summary>
    ///     Gets or sets the custom game directory for the instance, if specified.
    /// </summary>
    [ObservableProperty]
    public partial string? GameDirectory { get; set; }

    /// <summary>
    ///     Gets or sets the configuration of the instance.
    /// </summary>
    [ObservableProperty]
    public partial InstanceConfig ConfigModel { get; set; }

    /// <summary>
    ///     Gets or sets the process associated with the running game.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGameRunning))]
    public partial Process? GameProcess { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the game is currently running.
    /// </summary>
    [ObservableProperty]
    public partial bool IsGameRunning { get; set; }

    /// <summary>
    ///     Gets the icon of the instance as a bitmap. If the icon path is not set, a default icon is used.
    /// </summary>
    [ObservableProperty]
    public partial BitmapEntry Icon { get; set; }

    #endregion

    #region Progress Reporter

    private CancellationTokenSource _cancellationTokenSource = new();
    private ProgressWindow? _installWindow;

    public void Report(double progress)
    {
        EnsureWindowExists();
        _installWindow?.Report(progress);
    }

    public void Report(double progress, string fileName, bool downloading = true)
    {
        EnsureWindowExists();
        _installWindow?.Report(progress, fileName, downloading);
    }

    public void Report(EProgressKind kind, double progress)
    {
        EnsureWindowExists();
        _installWindow?.Report(kind, progress);
    }

    public void ReportExpectedTask(int? count)
    {
        EnsureWindowExists();
        _installWindow?.ReportExpectedTask(count);
    }

    public void ReportTaskCompleted()
    {
        EnsureWindowExists();
        _installWindow?.ReportTaskCompleted();
    }

    public void Report(string key)
    {
        Dispatcher.UIThread.Post(() =>
        {
            EnsureWindowExists();
            _installWindow?.Report(key);
        });
    }

    public void Report(double progress, string key, params object[]? args)
    {
        EnsureWindowExists();
        _installWindow?.Report(progress, key, args);
    }

    public void OpenReporter()
    {
        EnsureWindowExists();
    }

    public void CloseReporter()
    {
        _installWindow?.Close();
        _installWindow = null;
    }

    private void EnsureWindowExists()
    {
        _installWindow ??= _windowFactory.CreateProgressWindow(_cancellationTokenSource);
        _installWindow.Show();
    }

    #endregion
}
