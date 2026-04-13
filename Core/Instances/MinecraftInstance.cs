using System.Diagnostics;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.Domain;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;
using Tavstal.KonkordLauncher.Core.Services;

namespace Tavstal.KonkordLauncher.Core.Instances;

/// <summary>
/// Represents a Minecraft instance, handling installation, configuration, and launching of the game.
/// </summary>
// TODO: Refactor
public class MinecraftInstance
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(MinecraftInstance));
    private readonly LauncherDetails _launcherDetails;
    private readonly ClientDetails _client;
    private FileSystemWatcher? _watcher;
    private readonly Lock _watcherLock = new();
    private bool _isSanitizingLogFile;

    protected GameDetails GameDetails { get; }
    protected PathDetails PathDetails { get; }
    protected Resolution? Resolution { get; }
    protected VersionDetails VersionData { get; }
    public VersionManifest VersionManifest { get; }
    protected MinecraftVersion MinecraftVersion { get; }
    protected ArgumentBuilder? ArgumentBuilder { get; private set; }
    protected IProgressReporter? _progressReporter { get; }

    protected VersionMeta MinecraftVersionMeta { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinecraftInstance"/> class.
    /// </summary>
    /// <param name="gameDetails">Details about the game being installed.</param>
    /// <param name="pathDetails">Details about the file paths used for installation.</param>
    /// <param name="launcherDetails">Details about the launcher being used.</param>
    /// <param name="clientDetails">Details about the client user.</param>
    /// <param name="resolution">Optional resolution settings for the game.</param>
    /// <param name="progressReporter">Optional progress reporter for tracking installation progress.</param>
    public MinecraftInstance(GameDetails gameDetails, PathDetails pathDetails, LauncherDetails launcherDetails,
        ClientDetails clientDetails, Resolution? resolution = null, IProgressReporter? progressReporter = null)
    {
        _progressReporter = progressReporter;
        GameDetails = gameDetails;
        PathDetails = pathDetails;
        Resolution = resolution;
        _launcherDetails = launcherDetails;
        _client = clientDetails;

        VersionManifest = ManifestHelper.GetMinecraftManifest()
                          ?? throw new InvalidOperationException(
                              "Failed to read the local vanilla manifest. Please ensure that the file exists and is valid.");

        MinecraftVersion = VersionManifest.Versions.FirstOrDefault(x => x.Id == GameDetails.MinecraftVersion)
                           ?? throw new InvalidOperationException(
                               $"The specified Minecraft version does not exist in the manifest: {GameDetails.MinecraftVersion}");


        string vanillaVersionsRoot = Path.Combine(PathDetails.VersionsDir, "vanilla");
        Directory.CreateDirectory(vanillaVersionsRoot);
        string vanillaVersionDir = Path.Combine(vanillaVersionsRoot, GameDetails.MinecraftVersion);
        
        VersionData = new VersionDetails
        {
            MinecraftVersion = GameDetails.MinecraftVersion,
            CustomVersion = GameDetails.CustomVersion,
            VanillaVersionDirectory = vanillaVersionDir,
            VanillaJarPath = Path.Combine(vanillaVersionDir, $"{GameDetails.MinecraftVersion}.jar"),
            VanillaJsonPath = Path.Combine(vanillaVersionDir, $"{GameDetails.MinecraftVersion}.json"),
        };

        bool hasCustomGameDir = string.IsNullOrEmpty(GameDetails.CustomGameDirectory);
        if (GameDetails.Kind != EMinecraftKind.VANILLA)
        {
            string customVersionRoot = Path.Combine(PathDetails.VersionsDir, GameDetails.Kind.ToString().ToLower());
            Directory.CreateDirectory(customVersionRoot);
            string customVersionName = $"{GameDetails.MinecraftVersion}-{GameDetails.CustomVersion}";
            string customVersionDir = Path.Combine(customVersionRoot, customVersionName);
            bool isFabric = GameDetails.Kind is EMinecraftKind.FABRIC or EMinecraftKind.QUILT;
                
            VersionData.CustomVersionDirectory = customVersionDir;
            VersionData.CustomJarPath = isFabric ? VersionData.VanillaJarPath : Path.Combine(customVersionDir, $"{customVersionName}.jar");
            VersionData.CustomJsonPath = Path.Combine(customVersionDir, $"{customVersionName}.json");
            VersionData.GameDir = hasCustomGameDir ? Path.Combine(customVersionDir, "game") : GameDetails.CustomGameDirectory!;
            VersionData.NativesDir = hasCustomGameDir ? Path.Combine(customVersionDir, "natives") : Path.Combine(VersionData.GameDir, "natives");
        }
        else
        {
            VersionData.GameDir = hasCustomGameDir ? Path.Combine(vanillaVersionDir, "game") : GameDetails.CustomGameDirectory!;
            VersionData.NativesDir = hasCustomGameDir ? Path.Combine(vanillaVersionDir, "natives") : Path.Combine(VersionData.GameDir, "natives");
        }
    }

    /// <summary>
    /// Starts the Minecraft installation and launches the game.
    /// </summary>
    /// <returns>A <see cref="Process"/> object representing the launched game, or null if the process fails.</returns>
    public async Task<Process?> StartAsync(CancellationToken cancellationToken = default)
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "konkordlauncher_" + Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        Directory.CreateDirectory(VersionData.VanillaVersionDirectory);

        try
        {
            _logger.Debug("Downloading core files...");
            DateTime startTime = DateTime.Now;
            await DownloadCoreFilesAsync(cancellationToken);
            DateTime endTime = DateTime.Now;
            _logger.Info($"Core files downloaded in {(endTime - startTime).TotalMilliseconds}ms.");

            
            ArgumentBuilder = new ArgumentBuilder(
                version: MinecraftVersion.Id,
                versionName: GetVersionName(null),
                nativesDir: VersionData.NativesDir,
                gameDir: VersionData.GameDir,
                assetIndexId: MinecraftVersionMeta.Index.Id,
                versionMeta: MinecraftVersionMeta,
                launcherDetails: _launcherDetails,
                gameDetails: GameDetails,
                clientDetails: _client,
                pathDetails: PathDetails,
                resolution: Resolution);

            if (MinecraftVersionMeta.JavaVersionMeta.MajorVersion >= 9)
            {
                ArgumentBuilder.UseClasspathFile = true;
                ArgumentBuilder.ClasspathFilePath = Path.Combine(VersionData.GameDir, "classpath.txt");
            }

            startTime = DateTime.Now;
            var moddedData = await InstallModdedAsync(tempDir, cancellationToken);
            endTime = DateTime.Now;
            _logger.Info($"Modded data installation completed in {(endTime - startTime).TotalMilliseconds}ms.");
            string mainClass = moddedData?.MainClass ?? MinecraftVersionMeta.MainClass;

            // Force update main class
            ArgumentBuilder.AddGameArgument(new LaunchArg(mainClass + " ", 101));
            ArgumentBuilder.AddPlaceholder("${version_name}", GetVersionName(VersionData.CustomVersion));
            
            Directory.CreateDirectory(VersionData.GameDir);

            var libraries = GetCombinedLibraries(moddedData);
            startTime = DateTime.Now;
            await DownloadDependenciesAsync(VersionData, libraries, cancellationToken);
            endTime = DateTime.Now;
            _logger.Info($"Dependencies downloaded in {(endTime - startTime).TotalMilliseconds}ms.");
            
            ArgumentBuilder.AddClass(moddedData != null ? VersionData.CustomJarPath! : VersionData.VanillaJarPath);

            var arguments = ArgumentBuilder.Build();
            await Task.Delay(250, cancellationToken); // Ensure the progress reporter has time to update before launching
            _progressReporter?.CloseReporter();
            
            // Copy custom natives if specified
            startTime = DateTime.Now;
            foreach (string nativePath in PathDetails.CustomNativeFiles)
            {
                if (!File.Exists(nativePath))
                    continue;
                string destPath = Path.Combine(VersionData.NativesDir, Path.GetFileName(nativePath));
                File.Copy(nativePath, destPath, true);
            }
            endTime = DateTime.Now;
            _logger.Info($"Custom native files copied in {(endTime - startTime).TotalMilliseconds}ms.");

            // Execute pre-launch command if specified
            if (!string.IsNullOrEmpty(GameDetails.PreLaunchCommand))
            {
               var preLaunchProc = JavaProcessLauncher.StartCommand(GameDetails.PreLaunchCommand);
               if (preLaunchProc != null)
               {
                   startTime = DateTime.Now;
                   await preLaunchProc.WaitForExitAsync(cancellationToken);
                   endTime = DateTime.Now;
                   _logger.Info($"Pre-launch command executed in {(endTime - startTime).TotalMilliseconds}ms.");
               }
            }
            
            // Below 1.7 there is no dedicated logs directory
            // so this fixes this issue
            string? logsFilePath = null;
            try
            {
                Version minecraftVersion = new Version(GameDetails.MinecraftVersion);
                Version seven = new Version(1, 7);
                if (minecraftVersion < seven)
                {
                    string logsDir = Path.Combine(VersionData.GameDir, "logs");
                    if (!Directory.Exists(logsDir))
                        Directory.CreateDirectory(logsDir);
                    string latestLogFile = Path.Combine(logsDir, "latest.log");
                    if (File.Exists(latestLogFile))
                    {
                        DateTime lastEditDate = File.GetLastWriteTime(latestLogFile);
                        File.Move(latestLogFile, Path.Combine(logsDir, $"{lastEditDate:yyyy-MM-dd_HH-mm-ss}.log"),
                            true);
                    }

                    logsFilePath = latestLogFile;

                    // Make a file watcher to remove sensitive data from logs
                    _watcher = new FileSystemWatcher(logsDir, "latest.log")
                    {
                        NotifyFilter = NotifyFilters.LastWrite,
                        EnableRaisingEvents = true
                    };
                    _watcher.Changed += HandleFileWatcherChanged;
                }
            }
            catch (Exception)
            {
                // Ignore any errors with the log file watcher
            }

            // Launch the Minecraft game process with the constructed arguments
            var process = JavaProcessLauncher.StartJava(GameDetails.JavaPath, arguments.jvmArgs, arguments.gameArgs, logsFilePath, GameDetails.WrapperCommand,
                GameDetails.EnvironmentVariables);
            
            // Execute post-exit command if specified
            // Make sure to dispose the file watcher when the game process exits
            if (process != null)
                process.Exited += (_, _) =>
                {
                    if (!string.IsNullOrEmpty(GameDetails.PostExitCommand))
                        JavaProcessLauncher.StartCommand(GameDetails.PostExitCommand);
                    
                    if (_watcher == null)
                        return;
                    
                    _watcher.Changed -= HandleFileWatcherChanged;
                    _watcher?.Dispose();
                };
            
            return process;
        }
        finally
        {
            FileSystemHelper.DeleteDirectory(tempDir);
        }
    }

    /// <summary>
    /// Downloads the core files required for the Minecraft installation.
    /// </summary>
    private async Task DownloadCoreFilesAsync(CancellationToken cancellationToken = default)
    {
        var localVersionMeta = await MinecraftFileService.DownloadVersionAsync(VersionData, MinecraftVersion, _progressReporter, cancellationToken);
        MinecraftVersionMeta = localVersionMeta ?? throw new InvalidOperationException("Failed to download the version meta data. Please check your internet connection and try again.");

        // Change the required Java version if necessary
        if (GameDetails.Kind == EMinecraftKind.FORGE)
        {
            Version forgeMinecraftVersion = new Version(GameDetails.MinecraftVersion);
            // Set the required Java version to 7 for Forge versions 1.7.2 and below
            if (forgeMinecraftVersion.Major == 1 &&
                (forgeMinecraftVersion.Minor < 7 || forgeMinecraftVersion is { Minor: 7, Build: < 10 }))
                MinecraftVersionMeta.JavaVersionMeta.MajorVersion = 7;
        }
        
        if (GameDetails.JavaPath == "LAUNCH_ME_FIRST" || string.IsNullOrEmpty(GameDetails.JavaPath))
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract - It can be null if the event has no subscribers
            OnSetupDefaultJava?.Invoke(MinecraftVersionMeta);
        
        await MinecraftFileService.DownloadMappingsAsync(MinecraftVersionMeta, VersionData, _progressReporter);
        await MinecraftFileService.DownloadAssetsAsync(MinecraftVersionMeta, PathDetails.AssetsDir, VersionData.GameDir, _progressReporter, cancellationToken);
    }

    /// <summary>
    /// Combines the libraries required for the installation, including modded libraries if applicable.
    /// </summary>
    /// <param name="moddedData">Optional modded data for the installation.</param>
    /// <returns>A list of combined library metadata.</returns>
    private List<LibraryMeta> GetCombinedLibraries(ModdedData? moddedData)
    {
        var libraries = new List<LibraryMeta>(MinecraftVersionMeta.Libraries);
        if (moddedData?.Libraries.Count > 0)
            libraries.InsertRange(0, moddedData.Libraries);
        return libraries;
    }

    /// <summary>
    /// Downloads the dependencies required for the Minecraft installation.
    /// </summary>
    /// <param name="versionDetails">The version details of the installation.</param>
    /// <param name="libraries">The list of libraries to download.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A list of native libraries required for the installation.</returns>
    private async Task DownloadDependenciesAsync(VersionDetails versionDetails, List<LibraryMeta> libraries, CancellationToken cancellationToken = default)
    {
        if (ArgumentBuilder == null)
            throw new InvalidOperationException($"{nameof(ArgumentBuilder)} cannot be null.");
        
        var loggingArg = await MinecraftFileService.DownloadLoggingAsync(MinecraftVersionMeta, VersionData.CustomVersionDirectory ?? VersionData.VanillaVersionDirectory, versionDetails.GameDir, _progressReporter, cancellationToken);
        if (loggingArg != null)
            ArgumentBuilder.AddJvmArgumentBeforeClassPath(loggingArg);

        var classPath = await MinecraftFileService.DownloadLibrariesAsync(GameDetails.Kind, VersionData, libraries, ArgumentBuilder.ClassPath, PathDetails.CacheDir, PathDetails.LibrariesDir, _progressReporter, cancellationToken);
        foreach (var cp in classPath)
            ArgumentBuilder.AddClass(cp);

        /*string? result = await MinecraftFileService.ExtractLaunchWrapperAsync(PathDetails.LibrariesDir, cancellationToken);
        if (!string.IsNullOrEmpty(result))
            ArgumentBuilder.AddClass(result);
        ArgumentBuilder.AddJvmArgument(new LaunchArg("io.github.tavstaldev.launchWrapper.Launch", 1));*/
    }

    /// <summary>
    /// Installs modded data for the Minecraft installation. This method can be overridden by derived classes.
    /// </summary>
    /// <param name="tempDir">The temporary directory used for installation.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, returning modded data if applicable.</returns>
    protected virtual Task<ModdedData?> InstallModdedAsync(string tempDir, CancellationToken cancellationToken = default)
    {
        // Vanilla installer, do nothing
        return Task.FromResult<ModdedData?>(null);
    }

    /// <summary>
    /// Gets the version name based on the game kind and mod version.
    /// </summary>
    /// <param name="modVersion">Optional mod version string.</param>
    /// <returns>The version name as a string.</returns>
    private string GetVersionName(string? modVersion)
    {
        return GameDetails.Kind switch
        {
            EMinecraftKind.VANILLA => VersionData.MinecraftVersion,
            EMinecraftKind.FABRIC => $"fabric-loader-{modVersion}-{VersionData.MinecraftVersion}",
            EMinecraftKind.QUILT => $"quilt-loader-{modVersion}-{VersionData.MinecraftVersion}",
            EMinecraftKind.FORGE => $"{VersionData.MinecraftVersion}-forge-{modVersion}",
            EMinecraftKind.NEOFORGE => $"{VersionData.MinecraftVersion}-neoforge-{modVersion}",
            _ => VersionData.MinecraftVersion
        };
    }
    
    #region  Events

    /// <summary>
    /// Delegate for handling the setup of the default Java path based on the provided version metadata.
    /// </summary>
    /// <param name="versionMeta">The metadata of the Minecraft version used to determine the default Java path.</param>
    public delegate void SetupDefaultJavaEventHandler(VersionMeta versionMeta);

    /// <summary>
    /// Event triggered when the default Java path needs to be set up.
    /// Subscribers can handle this event to configure the Java path based on the provided version metadata.
    /// </summary>
    public event SetupDefaultJavaEventHandler OnSetupDefaultJava;

    /// <summary>
    /// Updates the Java path used by the game and logs the change.
    /// </summary>
    /// <param name="javaPath">The new Java path to be used by the game.</param>
    public void UpdateJavaPath(string javaPath)
    {
        GameDetails.JavaPath = javaPath;
        _logger.Debug($"Java path updated to: {javaPath}");
    }

    /// <summary>
    /// Handles changes to the log file being watched by the file system watcher.
    /// Replaces sensitive information such as the access token and UUID in the log file with masked values.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data containing information about the file change.</param>
    private void HandleFileWatcherChanged(object sender, FileSystemEventArgs e)
    {
        // Impossible but the IDE complains
        if (_watcher == null)
            return;
        
        lock (_watcherLock)
        {
            if (_isSanitizingLogFile)
                return;
            _isSanitizingLogFile = true;
        }
        
        try
        {
            _watcher.EnableRaisingEvents = false;
            
            string logsDir = Path.Combine(VersionData.GameDir, "logs");
            string latestLogFile = Path.Combine(logsDir, "latest.log");
            if (!File.Exists(latestLogFile))
            {
                _logger.Error("Latest log file not found for sanitization.");
                return;
            }
            
            string[] lines = File.ReadAllLines(latestLogFile);
            for (int i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrEmpty(_client.AccessToken) && lines[i].Contains(_client.AccessToken))
                    lines[i] = lines[i].Replace(_client.AccessToken, "****");
                
                if (lines[i].Contains(_client.UUID))
                    lines[i] = lines[i].Replace(_client.UUID, "****");
            }
            File.WriteAllLines(latestLogFile, lines);
        }
        catch (IOException)
        {
            // File is being used by another process, ignore
        }
        finally
        {
            lock (_watcherLock)
                _isSanitizingLogFile = false;
            _watcher.EnableRaisingEvents = true;
        }
    }
    #endregion
}