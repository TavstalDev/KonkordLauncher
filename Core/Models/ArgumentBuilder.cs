using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.Platform;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;

namespace Tavstal.KonkordLauncher.Core.Models;

public class ArgumentBuilder
{
    private readonly List<string> _classPath = [];
    public List<string> ClassPath => _classPath;
    private readonly List<LaunchArg> _jvmArguments = [];
    private readonly List<LaunchArg> _jvmArgumentsBeforeClassPath = [];
    private readonly List<LaunchArg> _gameArguments = [];
    private readonly Dictionary<string, string> _placeholders = new();
    
    public bool UseClasspathFile { get; set; }
    
    public string? ClasspathFilePath { get; set; }

    public void AddClass(string classPath)
    {
        if (_classPath.Contains(classPath))
            return;
        _classPath.Add(classPath);
    }

    public void AddJvmArgument(LaunchArg arg)
    {
        if (_jvmArguments.Any(x => x.Arg == arg.Arg))
            return;
        _jvmArguments.Add(arg);
    }

    public void AddJvmArgumentBeforeClassPath(LaunchArg arg)
    {
        if (_jvmArgumentsBeforeClassPath.Any(x => x.Arg == arg.Arg))
            return;
        _jvmArgumentsBeforeClassPath.Add(arg);
    }

    public void AddGameArgument(LaunchArg arg)
    {
        if (_gameArguments.Any(x => x.Arg == arg.Arg))
            return;
        _gameArguments.Add(arg);
    }

    public void AddPlaceholder(string key, string value)
    {
        _placeholders[key] = value;
    }

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
            { "${library_directory}", QuoteIfNeeded(pathDetails.LibrariesDir) },
            { "${assets_index_name}", assetIndexId },
            { "${auth_uuid}", clientDetails.UUID },
            { "${auth_player_name}", clientDetails.DisplayName },
            { "${auth_access_token}", token },
            { "${auth_session}", token },
            { "${clientid}",clientDetails.ClientId },
            { "${auth_xuid}", clientDetails.Xuid },
            { "${user_type}", userType },
            { "${user_properties}", "{}" }
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
    
    public (string jvmArgs, string gameArgs) Build()
    {
        string classpath;
        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression - It is more readable this way
        if (OSHelper.GetOperatingSystem() == EOperatingSystem.Windows)
            classpath = string.Join(";", _classPath).Replace(@"\", @"\\");
        else
            classpath = string.Join(":", _classPath);

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
    
    private IEnumerable<string> BuildJvmArguments()
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
    
    private IEnumerable<string> BuildGameArguments()
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
    
    private static string QuoteIfNeeded(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "\"\"";
        return path.StartsWith('"') && path.EndsWith('"') ? path : $"\"{path}\"";
    }
}