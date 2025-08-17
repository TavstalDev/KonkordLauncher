using System.Diagnostics;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;
using Tavstal.KonkordLauncher.Core.Services;

namespace Tavstal.KonkordLauncher.Core.Instances;

/// <summary>
/// Represents a Minecraft instance, handling installation, configuration, and launching of the game.
/// </summary>
public class MinecraftInstance
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(MinecraftInstance));
    private readonly LauncherDetails _launcherDetails;
    private readonly ClientDetails _client;

    protected GameDetails GameDetails { get; }
    protected PathDetails PathDetails { get; }
    protected Resolution? Resolution { get; }
    public VersionDetails VersionData { get; }
    public VersionManifest VersionManifest { get; }
    public MinecraftVersion MinecraftVersion { get; }
    protected IProgressReporter? _progressReporter { get; }

    protected VersionMeta MinecraftVersionMeta { get; private set; }
    protected string _classPath = string.Empty;
    protected readonly List<LaunchArg> _jvmArguments = [];
    protected readonly List<LaunchArg> _gameArguments = [];
    protected readonly List<LaunchArg> _jvmArgumentsBeforeClassPath = [];

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

        VersionData = GameHelper.GetVersionDetails(PathDetails.VersionsDir, GameDetails.MinecraftVersion,
            EMinecraftKind.VANILLA, null, GameDetails.CustomGameDirectory);
    }

    /// <summary>
    /// Starts the Minecraft installation and launches the game.
    /// </summary>
    /// <returns>A <see cref="Process"/> object representing the launched game, or null if the process fails.</returns>
    public async Task<Process?> Start()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "konkordlauncher_" + Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        Directory.CreateDirectory(VersionData.VersionDirectory);

        try
        {
            await DownloadCoreFilesAsync();

            var moddedData = await InstallModdedAsync(tempDir);
            var (versionDetails, mainClass, customVersion) = GetLaunchParameters(moddedData);

            if (!Directory.Exists(versionDetails.GameDir))
                Directory.CreateDirectory(versionDetails.GameDir);

            var libraries = GetCombinedLibraries(moddedData);
            await DownloadDependenciesAsync(versionDetails, libraries);
            
            if (moddedData != null)
                _classPath += moddedData.VersionData.VersionJarPath;
            else
                _classPath += VersionData.VersionJarPath;

            string arguments = BuildArguments(versionDetails.GameDir, mainClass, versionDetails.NativesDir, customVersion);
            await Task.Delay(250); // Ensure the progress reporter has time to update before launching
            _progressReporter?.Hide();
            
            // Copy custom natives if specified
            foreach (string nativePath in PathDetails.CustomNativeFiles)
            {
                if (!File.Exists(nativePath))
                    continue;
                string destPath = Path.Combine(versionDetails.NativesDir, Path.GetFileName(nativePath));
                File.Copy(nativePath, destPath, true);
            }

            // Execute pre-launch command if specified
            if (!string.IsNullOrEmpty(GameDetails.PreLaunchCommand))
            {
               var preLaunchProc = JavaProcessLauncher.StartCommand(GameDetails.PreLaunchCommand);
               if (preLaunchProc != null)
                   await preLaunchProc.WaitForExitAsync();
            }
            
            // Below 1.7 there is no dedicated logs directory
            // so this fixes this issue
            // TODO: Remove sensitive data from logs
            Version minecraftVersion = new Version(GameDetails.MinecraftVersion);
            Version seven = new Version(1, 7);
            string? logsFilePath = null;
            if (minecraftVersion < seven)
            {
                string logsDir = Path.Combine(versionDetails.GameDir, "logs");
                if (!Directory.Exists(logsDir))
                    Directory.CreateDirectory(logsDir);
                string latestLogFile = Path.Combine(logsDir, "latest.log");
                if (File.Exists(latestLogFile))
                {
                    DateTime lastEditDate = File.GetLastWriteTime(latestLogFile);
                    File.Move(latestLogFile, Path.Combine(logsDir, $"{lastEditDate:yyyy-MM-dd_HH-mm-ss}.log"), true);
                }
                logsFilePath = latestLogFile;
            }
            
            // Launch the Minecraft game process with the constructed arguments
            var process = JavaProcessLauncher.StartJava(GameDetails.JavaPath, arguments, logsFilePath, GameDetails.WrapperCommand,
                GameDetails.EnvironmentVariables);
            
            // Execute post-exit command if specified
            if (!string.IsNullOrEmpty(GameDetails.PostExitCommand) && process != null)
            {
                process.Exited += (_, _) => JavaProcessLauncher.StartCommand(GameDetails.PostExitCommand);
            }
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
    private async Task DownloadCoreFilesAsync()
    {
        var localVersionMeta = await MinecraftFileService.DownloadVersionAsync(VersionData, MinecraftVersion, _progressReporter);
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
        
        if (GameDetails.JavaPath == "LAUNCH_ME_FIRST")
            OnSetupDefaultJava.Invoke(MinecraftVersionMeta);
        
        await MinecraftFileService.DownloadMappingsAsync(MinecraftVersionMeta, VersionData, _progressReporter);
        await MinecraftFileService.DownloadAssetsAsync(MinecraftVersionMeta, PathDetails.AssetsDir, VersionData.GameDir, _progressReporter);
    }

    /// <summary>
    /// Retrieves the launch parameters for the Minecraft process.
    /// </summary>
    /// <param name="moddedData">Optional modded data for the installation.</param>
    /// <returns>A tuple containing version details, the main class, and the custom version string.</returns>
    private (VersionDetails, string, string?) GetLaunchParameters(ModdedData? moddedData)
    {
        if (moddedData != null)
        {
            return (moddedData.VersionData, moddedData.MainClass ?? MinecraftVersionMeta.MainClass, moddedData.VersionData.CustomVersion);
        }
        return (VersionData, MinecraftVersionMeta.MainClass, null);
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
        {
            libraries.InsertRange(0, moddedData.Libraries);
        }
        return libraries;
    }

    /// <summary>
    /// Downloads the dependencies required for the Minecraft installation.
    /// </summary>
    /// <param name="versionDetails">The version details of the installation.</param>
    /// <param name="libraries">The list of libraries to download.</param>
    /// <returns>A list of native libraries required for the installation.</returns>
    private async Task DownloadDependenciesAsync(VersionDetails versionDetails, List<LibraryMeta> libraries)
    {
        var loggingArg = await MinecraftFileService.DownloadLoggingAsync(MinecraftVersionMeta, versionDetails.VersionDirectory, versionDetails.GameDir, _progressReporter);
        if (loggingArg != null)
            _jvmArgumentsBeforeClassPath.Add(loggingArg);

        var classPath = await MinecraftFileService.DownloadLibrariesAsync(GameDetails.Kind, VersionData, libraries, _classPath, PathDetails.CacheDir, PathDetails.LibrariesDir, _progressReporter);
        _classPath = classPath;
    }

    /// <summary>
    /// Installs modded data for the Minecraft installation. This method can be overridden by derived classes.
    /// </summary>
    /// <param name="tempDir">The temporary directory used for installation.</param>
    /// <returns>A task representing the asynchronous operation, returning modded data if applicable.</returns>
    protected virtual Task<ModdedData?> InstallModdedAsync(string tempDir)
    {
        // Vanilla installer, do nothing
        return Task.FromResult<ModdedData?>(null);
    }

    /// <summary>
    /// Builds the arguments for launching the Minecraft process.
    /// </summary>
    /// <param name="gameDir">The game directory.</param>
    /// <param name="mainClass">The main class to launch.</param>
    /// <param name="nativesDir">The directory containing native libraries.</param>
    /// <param name="modVersion">Optional mod version string.</param>
    /// <returns>A string containing the launch arguments.</returns>
    private string BuildArguments(string gameDir, string mainClass, string nativesDir, string? modVersion = null)
    {
        var arguments = new List<string>();

        arguments.AddRange(BuildJvmArguments(gameDir));
        arguments.AddRange(BuildGameArguments(mainClass));

        string argumentString = string.Join(' ', arguments);
        return ReplacePlaceholders(argumentString, gameDir, nativesDir, modVersion);
    }

    /// <summary>
    /// Builds the JVM arguments for the Minecraft process.
    /// </summary>
    /// <param name="gameDir">The game directory.</param>
    /// <returns>A collection of JVM arguments.</returns>
    private IEnumerable<string> BuildJvmArguments(string gameDir)
    {
        IEnumerable<string> argsToAdd;
        var jvmArgs = new List<string>
        {
            GameDetails.MinMemory > GameDetails.MaxMemory
                ? $"-Xms{GameDetails.MaxMemory}M"
                : $"-Xms{(GameDetails.MinMemory > 0 ? GameDetails.MinMemory : 256)}M",
            $"-Xmx{(GameDetails.MaxMemory > 0 ? GameDetails.MaxMemory : 4096)}M",
            $"-Dminecraft.applet.TargetDirectory=\"{gameDir}\""
        };
        
        if (!string.IsNullOrEmpty(GameDetails.JvmArgs))
            jvmArgs.Add(GameDetails.JvmArgs);

        // 1.16 offline mode fix
        if (VersionData.MinecraftVersion.StartsWith("1.16") && _client.IsOffline)
        {
            jvmArgs.Add("-Dminecraft.api.auth.host=https://nope.invalid ");
            jvmArgs.Add("-Dminecraft.api.account.host=https://nope.invalid");
            jvmArgs.Add("-Dminecraft.api.session.host=https://nope.invalid");
            jvmArgs.Add("-Dminecraft.api.services.host=https://nope.invalid");
        }
        
        argsToAdd = _jvmArgumentsBeforeClassPath.OrderByDescending(x => x.Priority).Select(a => a.Arg);
        foreach (var arg in argsToAdd)
            jvmArgs.Add(arg);
        
        argsToAdd = MinecraftVersionMeta.GetJvmArguments();
        foreach (var arg in argsToAdd)
            jvmArgs.Add(arg);
        

        argsToAdd = _jvmArguments.OrderByDescending(x => x.Priority).Select(a => a.Arg);
        foreach (var arg in argsToAdd)
            jvmArgs.Add(arg);
        
        // Classpath fallback
        if (!jvmArgs.Any(x => x.Contains("-cp")))
            jvmArgs.Add("-cp ${classpath}");
        return jvmArgs;
    }

    /// <summary>
    /// Builds the game arguments for the Minecraft process.
    /// </summary>
    /// <param name="mainClass">The main class to launch.</param>
    /// <returns>A collection of game arguments.</returns>
    private IEnumerable<string> BuildGameArguments(string mainClass)
    {
        var gameArgs = new List<string>
        {
            mainClass,
            MinecraftVersionMeta.GetGameArgumentString()
        };
        var gameArguments = _gameArguments.OrderByDescending(x => x.Priority).Select(a => a.Arg);
        foreach (var arg in gameArguments)
        {
            if (gameArgs.Contains(arg))
                continue;
            
            gameArgs.Add(arg);
        }

        if (Resolution is { X: > 0 })
            gameArgs.Add($"--width {Resolution.X}");
        if (Resolution is { Y: > 0 })
            gameArgs.Add($"--height {Resolution.Y}");

        if (!string.IsNullOrEmpty(GameDetails.ServerAddressToJoin))
            gameArgs.Add($"--quickPlayMultiplayer {GameDetails.ServerAddressToJoin}");

        return gameArgs;
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

    /// <summary>
    /// Replaces placeholders in the argument string with actual values.
    /// </summary>
    /// <param name="argumentString">The argument string containing placeholders.</param>
    /// <param name="gameDir">The game directory.</param>
    /// <param name="nativesDir">The directory containing native libraries.</param>
    /// <param name="modVersion">Optional mod version string.</param>
    /// <returns>The argument string with placeholders replaced.</returns>
    private string ReplacePlaceholders(string argumentString, string gameDir, string nativesDir, string? modVersion)
    {
        string classPathSeparator = ";";
        if (OSHelper.GetOperatingSystem() != EOperatingSystem.Windows)
            classPathSeparator = ":";

        string gameAssetsDir = Path.Combine(PathDetails.AssetsDir, "virtual", "legacy");
        gameAssetsDir = gameAssetsDir.StartsWith('"') ? gameAssetsDir : $"\"{gameAssetsDir}\"";
        string userType = _client.IsOffline ? "offline" : "msa";
        
        
        var replacements = new Dictionary<string, string?>
        {
            { "${natives_directory}", nativesDir.StartsWith('"') ? nativesDir : $"\"{nativesDir}\"" },
            { "${launcher_name}", _launcherDetails.LauncherName },
            { "${launcher_version}", _launcherDetails.LauncherVersion },
            { "${auth_player_name}", _client.DisplayName },
            { "${version_name}", GetVersionName(modVersion) },
            { "${game_directory}", gameDir.StartsWith('"') ? gameDir : $"\"{gameDir}\"" },
            { "${game_assets}", gameAssetsDir },
            { "${assets_root}", PathDetails.AssetsDir.StartsWith('"') ? PathDetails.AssetsDir : $"\"{PathDetails.AssetsDir}\"" },
            { "${assets_index_name}", MinecraftVersionMeta.Index.Id },
            { "${auth_uuid}", _client.UUID },
            { "${auth_access_token}", string.IsNullOrEmpty(_client.AccessToken) ? "none" : _client.AccessToken },
            { "${auth_session}", string.IsNullOrEmpty(_client.AccessToken) ? "none" : _client.AccessToken},
            { "${clientid}", _client.ClientId },
            { "${auth_xuid}", _client.Xuid },
            { "${user_type}", userType },
            { "${version_type}", "release" },
            { "${classpath}", _classPath.StartsWith('"') ? _classPath : $"\"{_classPath}\"" },
            { "${library_directory}", PathDetails.LibrariesDir.StartsWith('"') ? PathDetails.LibrariesDir : $"\"{PathDetails.LibrariesDir}\"" },
            { "${classpath_separator}", classPathSeparator},
            { "${user_properties}", "{}" },
            { "${arch}", Environment.Is64BitOperatingSystem ? "64" : "32" }
        };

        return replacements.Aggregate(argumentString, (current, replacement) => current.Replace(replacement.Key, replacement.Value));
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
    #endregion
}