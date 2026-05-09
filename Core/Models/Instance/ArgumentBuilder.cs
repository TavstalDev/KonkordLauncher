using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.Platform;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;

namespace Tavstal.KonkordLauncher.Core.Models.Instance;

/// <summary>
/// Builds JVM and game argument strings for launching Minecraft instances.
/// </summary>
public class ArgumentBuilder
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(ArgumentBuilder));
    private readonly List<string> _classPath = [];
    public List<string> ClassPath => _classPath;
    private readonly List<LaunchArg> _jvmArguments = [];
    private readonly List<LaunchArg> _jvmArgumentsBeforeClassPath = [];
    private readonly List<LaunchArg> _gameArguments = [];
    private readonly Dictionary<string, string> _placeholders = new();
    private readonly string _classPathSeparator = OSHelper.GetOperatingSystem() == EOperatingSystem.Windows ? ";" : ":";
    
    public bool UseClasspathFile { get; set; }
    
    public string? ClasspathFilePath { get; set; }

    
    /// <summary>
    /// Adds a classpath entry to the builder.
    /// Duplicate entries are ignored.
    /// </summary>
    /// <param name="classPath">A single classpath entry to include.</param>
    public void AddClass(string classPath)
    {
        if (_classPath.Contains(classPath))
            return;
        _classPath.Add(classPath);
    }

    /// <summary>
    /// Adds a JVM argument that will be placed after the pre-classpath JVM args.
    /// Duplicate argument values (by Arg) are ignored.
    /// </summary>
    /// <param name="arg">LaunchArg containing the argument string and priority for ordering.</param>
    public void AddJvmArgument(LaunchArg arg)
    {
        _jvmArguments.Add(arg);
    }

    /// <summary>
    /// Adds a JVM argument that should appear before the classpath (e.g., memory flags).
    /// Duplicate argument values (by Arg) are ignored.
    /// </summary>
    /// <param name="arg">LaunchArg containing the argument string and priority for ordering.</param>
    public void AddJvmArgumentBeforeClassPath(LaunchArg arg)
    {
        _jvmArgumentsBeforeClassPath.Add(arg);
    }

    /// <summary>
    /// Adds a game argument (argument passed to the Minecraft process).
    /// Duplicate arguments (by Arg) are ignored.
    /// </summary>
    /// <param name="arg">LaunchArg containing the argument string and priority for ordering.</param>
    public void AddGameArgument(LaunchArg arg)
    {
        if (_gameArguments.Any(x => x.Arg == arg.Arg))
            return;
        _gameArguments.Add(arg);
    }

    /// <summary>
    /// Adds or updates a placeholder token that will be replaced in the final argument strings.
    /// Use tokens like "${auth_access_token}" or "${classpath}".
    /// </summary>
    /// <param name="key">Placeholder key (e.g. "${auth_access_token}").</param>
    /// <param name="value">Replacement value for the placeholder.</param>
    public void AddPlaceholder(string key, string value)
    {
        _placeholders[key] = value;
    }

    
    /// <summary>
    /// Initializes a new ArgumentBuilder using version metadata and launcher/game/client/path details.
    /// This constructor prepares default placeholders, memory settings and populates JVM and game arguments
    /// based on the provided VersionMeta and GameDetails.
    /// </summary>
    /// <param name="version">Minecraft version identifier (used for version-specific adjustments).</param>
    /// <param name="versionName">Readable version name.</param>
    /// <param name="nativesDir">Directory containing native libraries.</param>
    /// <param name="gameDir">The instance game directory.</param>
    /// <param name="assetIndexId">Asset index id used by the version.</param>
    /// <param name="versionMeta">Version metadata providing JVM/game argument templates.</param>
    /// <param name="launcherDetails">Information about the launcher (name, version).</param>
    /// <param name="gameDetails">Per-instance game settings (min/max memory, additional jvm args, server to join).</param>
    /// <param name="clientDetails">Authenticated client details (tokens, uuid, display name).</param>
    /// <param name="pathDetails">Resolved path details (assets dir, libraries dir).</param>
    /// <param name="resolution">Optional resolution to set width/height game args.</param>
    public ArgumentBuilder(string version, string versionName, string nativesDir, string gameDir, string assetIndexId, VersionMeta versionMeta, LauncherDetails launcherDetails, GameDetails gameDetails, ClientDetails clientDetails, PathDetails pathDetails, Resolution? resolution)
    {
        string gameAssetsDir = Path.Combine(pathDetails.AssetsDir, "virtual", "legacy");
        string userType = clientDetails.IsOffline ? "offline" : "msa";
        string token = string.IsNullOrEmpty(clientDetails.AccessToken) ? "none" : clientDetails.AccessToken;
        
        var replacements = new Dictionary<string, string>
        {
            { "${launcher_name}", launcherDetails.LauncherName },
            { "${launcher_version}", launcherDetails.LauncherVersion },
            { "${version_name}", versionName },
            { "${version_type}", "release" },
            { "${arch}", Environment.Is64BitOperatingSystem ? "64" : "32" },
            { "${natives_directory}", QuoteIfNeeded(nativesDir) },
            { "${game_directory}", QuoteIfNeeded(gameDir ) },
            { "${game_assets}", QuoteIfNeeded(gameAssetsDir) },
            { "${assets_root}", QuoteIfNeeded(pathDetails.AssetsDir) },
            { "${library_directory}", pathDetails.LibrariesDir },
            { "${assets_index_name}", assetIndexId },
            { "${auth_uuid}", clientDetails.UUID },
            { "${auth_player_name}", clientDetails.DisplayName },
            { "${auth_access_token}", token },
            { "${auth_session}", token },
            { "${clientid}",clientDetails.ClientId },
            { "${auth_xuid}", clientDetails.Xuid },
            { "${user_type}", userType },
            { "${user_properties}", "{}" },
            { "${classpath_separator}", _classPathSeparator }
        };

        foreach (var replacement in replacements)
            _placeholders.TryAdd(replacement.Key, replacement.Value);

        uint minMemory = 512;
        if (gameDetails.MinMemory > minMemory)
            minMemory = gameDetails.MinMemory;
        uint maxMemory = 2048;
        if (gameDetails.MaxMemory > maxMemory && gameDetails.MaxMemory > minMemory)
            maxMemory = gameDetails.MaxMemory;
        
        _jvmArgumentsBeforeClassPath.Add(new LaunchArg($"-Xms{minMemory}M", 100));
        _jvmArgumentsBeforeClassPath.Add(new LaunchArg($"-Xmx{maxMemory}M", 100));
        _jvmArgumentsBeforeClassPath.Add(new LaunchArg($"-Dminecraft.applet.TargetDirectory=\"{gameDir}\"", 99));
        
        if (version.StartsWith("1.16") && clientDetails.IsOffline)
        {
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Dminecraft.api.auth.host=https://nope.invalid ", 99));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Dminecraft.api.account.host=https://nope.invalid", 99));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Dminecraft.api.session.host=https://nope.invalid", 99));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Dminecraft.api.services.host=https://nope.invalid", 99));
        }
        
        if (string.IsNullOrEmpty(gameDetails.JvmArgs))
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg(gameDetails.JvmArgs, 98));

        var metaArgs = versionMeta.GetJvmArguments();
        foreach (var arg in metaArgs)
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg(arg, 97));

        var gameArg = versionMeta.GetGameArgumentString();
        _gameArguments.Add(new LaunchArg(gameArg, 100));
        
        if (resolution is { X: > 0 })
            _gameArguments.Add(new LaunchArg($"--width {resolution.X}", 99));
        if (resolution is { Y: > 0 })
            _gameArguments.Add(new LaunchArg($"--height {resolution.Y}", 99));
        
        if (!string.IsNullOrEmpty(gameDetails.ServerAddressToJoin))
            _gameArguments.Add(new LaunchArg($"--quickPlayMultiplayer {gameDetails.ServerAddressToJoin}", 98));
    }
    
    /// <summary>
    /// Builds and returns the final JVM and game argument strings.
    /// It computes the classpath string (or writes a classpath file), replaces placeholders,
    /// and orders arguments according to configured priorities.
    /// </summary>
    /// <returns>
    /// A tuple where Item1 is the JVM argument string and Item2 is the game argument string.
    /// </returns>
    public (string jvmArgs, string gameArgs) Build()
    {
        string classpath = string.Join(_classPathSeparator, _classPath);
        if (OSHelper.GetOperatingSystem() == EOperatingSystem.Windows)
            classpath = classpath.Replace(@"\", @"\\");

        if (UseClasspathFile)
        {
            if (string.IsNullOrEmpty(ClasspathFilePath))
                ClasspathFilePath = Path.Combine(Path.GetTempPath(), "konkordlauncher_classpath.txt");
            File.WriteAllText(ClasspathFilePath, classpath);
            _placeholders.Add("${classpath}", $"\"@{ClasspathFilePath}\"");
        }
        else
            _placeholders.Add("${classpath}", QuoteIfNeeded(classpath));
        
        string jvmArgString = string.Join(' ', BuildJvmArguments());
        foreach (var placeholder in _placeholders)
            jvmArgString = jvmArgString.Replace(placeholder.Key, placeholder.Value);
        
        string gameArgString = string.Join(' ', BuildGameArguments());
        foreach (var placeholder in _placeholders)
            gameArgString = gameArgString.Replace(placeholder.Key, placeholder.Value);
        
        return (jvmArgString, gameArgString);
    }
    
    /// <summary>
    /// Constructs the ordered list of JVM arguments (pre-classpath then post-classpath).
    /// Ensures a classpath argument is present as a fallback if none is specified.
    /// </summary>
    /// <returns>List of JVM argument strings in the correct order.</returns>
    private List<string> BuildJvmArguments()
    {
        var jvmArgs = new List<string>();
        
        var argsToAdd = _jvmArgumentsBeforeClassPath.OrderByDescending(x => x.Priority).Select(a => a.Arg);
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
    /// Constructs the ordered and deduplicated list of game argument strings.
    /// Arguments are ordered by their priority and duplicates are removed.
    /// </summary>
    /// <returns>List of game argument strings.</returns>
    private List<string> BuildGameArguments()
    {
        var gameArgs = new List<string>();
        var gameArguments = _gameArguments.OrderByDescending(x => x.Priority).Select(a => a.Arg);
        foreach (var arg in gameArguments)
        {
            if (gameArgs.Contains(arg))
                continue;
            
            gameArgs.Add(arg);
        }

        return gameArgs;
    }
    
    /// <summary>
    /// Ensures a path or value is wrapped in double quotes if necessary.
    /// If the supplied string is null or empty the method returns an empty quoted string "\"\"".
    /// </summary>
    /// <param name="path">The path or argument value to quote if needed.</param>
    /// <returns>A quoted string value safe for use in command-line arguments.</returns>
    private static string QuoteIfNeeded(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "\"\"";
        return path.StartsWith('"') && path.EndsWith('"') ? path : $"\"{path}\"";
    }
}