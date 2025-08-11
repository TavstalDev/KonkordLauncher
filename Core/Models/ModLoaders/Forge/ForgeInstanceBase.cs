using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Instances;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge.New;

namespace Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge;

/// <summary>
/// Represents the base class for a Forge instance, inheriting from MinecraftInstance.
/// </summary>
/// <param name="gameDetails">Details about the game, such as version and paths.</param>
/// <param name="pathDetails">Details about the file paths used by the instance.</param>
/// <param name="launcherDetails">Details about the launcher configuration.</param>
/// <param name="clientDetails">Details about the client configuration.</param>
/// <param name="resolution">Optional screen resolution settings for the instance.</param>
/// <param name="progressReporter">Optional progress reporter for tracking installation or setup progress.</param>
public abstract class ForgeInstanceBase(
    GameDetails gameDetails,
    PathDetails pathDetails,
    LauncherDetails launcherDetails,
    ClientDetails clientDetails,
    Resolution? resolution = null,
    IProgressReporter? progressReporter = null)
    : MinecraftInstance(gameDetails, pathDetails, launcherDetails, clientDetails, resolution, progressReporter)
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(ForgeInstanceBase));

    // The following methods are originally from https://github.com/CmlLib
    // but have been adapted to fit the current code structure and conventions.
    
    /// <summary>
    /// Maps processor data and starts the processors for a Forge installation.
    /// </summary>
    /// <param name="installProfile">The Forge version profile containing installation data.</param>
    /// <param name="installerDir">The directory where the installer is located.</param>
    protected async Task MapAndStartProcessors(ForgeVersionProfile installProfile, string installerDir)
    {
        // Maps the processor data for the client side.
        Dictionary<string, string?> mapData = [];
        if (installProfile.Data != null)
            mapData = MapProcessorData(installProfile.Data, VersionData.VanillaJarPath, installerDir);
        // Starts the processors using the mapped data.
        await StartProcessors(installProfile.Processors, mapData);
    }
    
    /// <summary>
    /// Maps processor data to a dictionary for use during installation.
    /// </summary>
    /// <param name="data">The JSON object containing processor data.</param>
    /// <param name="minecraftJar">The path to the Minecraft jar file.</param>
    /// <param name="installDir">The directory where the installation is taking place.</param>
    /// <returns>A dictionary containing the mapped processor data.</returns>
    protected Dictionary<string, string?> MapProcessorData(JObject data, string minecraftJar, string installDir)
    {
        var dataMapping = new Dictionary<string, string?>();
        // Iterates through the data and maps each item to its full path
        foreach (var item in data)
        {
            string? value = item.Value?["client"]?.ToString();

            if (string.IsNullOrEmpty(value))
                continue;

            var fullPath = ForgeMapper.ToFullPath(value, PathDetails.LibrariesDir);
            dataMapping[item.Key] = fullPath == value 
                ? Path.Combine(installDir, value.Trim('/')) 
                : fullPath;
        }

        // Adds additional required mappings.
        dataMapping.Add("SIDE", "client");
        dataMapping.Add("MINECRAFT_JAR", minecraftJar);
        dataMapping.Add("INSTALLER", Path.Combine(installDir, "installer.jar"));

        return dataMapping;
    }
    
    /// <summary>
    /// Starts the processors for the Forge installation.
    /// </summary>
    /// <param name="processors">The array of processors to execute.</param>
    /// <param name="mapData">The mapped processor data.</param>
    protected async Task StartProcessors(JArray? processors, Dictionary<string, string?> mapData)
    {
        if (processors == null || processors.Count == 0)
            return;

        // Iterates through each processor and starts it if necessary.
        for (int i = 0; i < processors.Count; i++)
        {
            JToken item = processors[i];

            // Checks if the processor outputs are valid.
            JObject? outputs = item["outputs"] as JObject;
            if (outputs == null || !CheckProcessorOutputs(outputs, mapData))
            {
                // Skips server-side processors.
                JArray? sides = item["sides"] as JArray;
                if (sides?.FirstOrDefault() == null || sides.FirstOrDefault()?.ToString() == "client") //skip server side
                    await StartProcessor(item, mapData);
            }
            // Updates the progress reporter with the current progress.
            double percent = (double)i / (double)processors.Count * 100d;
            _progressReporter?.SetStatusTranslated("instance.building", "forge", percent.ToString("0.00"));
        }
    }
    
    /// <summary>
    /// Checks if the processor outputs are valid by verifying file existence and SHA1 hashes.
    /// </summary>
    /// <param name="outputs">The JSON object containing the processor outputs.</param>
    /// <param name="mapData">The mapped processor data.</param>
    /// <returns>True if all outputs are valid; otherwise, false.</returns>
    private bool CheckProcessorOutputs(JObject outputs, Dictionary<string, string?> mapData)
    {
        foreach (var item in outputs)
        {
            if (item.Value == null)
                continue;

            // Interpolates the key and value using the mapped data.
            string key = ForgeMapper.Interpolation(item.Key, mapData, true);
            string value = ForgeMapper.Interpolation(item.Value.ToString(), mapData, true);

            // Verifies the file existence and SHA1 hash.
            if (!File.Exists(key) || !FileSystemHelper.CheckSHA1(key, value))
                return false;
        }

        return true;
    }
        
    /// <summary>
    /// Starts a single processor by executing its associated jar file.
    /// </summary>
    /// <param name="processor">The JSON token representing the processor.</param>
    /// <param name="mapData">The mapped processor data.</param>
    private async Task StartProcessor(JToken processor, Dictionary<string, string?> mapData)
    {
        string? name = processor["jar"]?.ToString();
        if (name == null)
            return;

        var jarPath = Path.Combine(PathDetails.LibrariesDir, PackageName.Parse(name).GetPath());
        var jarManifest = new ProcessorJarFile(jarPath).GetManifest();

        if (jarManifest == null || !jarManifest.TryGetValue("Main-Class", out var mainClass) || string.IsNullOrEmpty(mainClass))
            return;

        // Constructs the classpath for the processor.
        var classpath = (processor["classpath"] as JArray)?
            .Select(libName => Path.Combine(PathDetails.LibrariesDir, PackageName.Parse(libName.ToString()).GetPath()))
            .ToList() ?? [];
        classpath.Add(jarPath);

        // Constructs the arguments for the processor.
        string[] args = [];
        if (processor["args"] is JArray jarray)
        {
            var rawArgs = jarray.Select(arg => arg.ToString()).ToArray();
            args = ForgeMapper.Map(rawArgs, mapData, PathDetails.LibrariesDir);
        }

        await StartJava(classpath.ToArray(), mainClass, args);
    }
    
    /// <summary>
    /// Starts a Java process with the specified classpath, main class, and arguments.
    /// </summary>
    /// <param name="classpath">The array of classpath entries.</param>
    /// <param name="mainClass">The main class to execute.</param>
    /// <param name="args">The arguments to pass to the Java process.</param>
    private async Task StartJava(string[] classpath, string mainClass, string[]? args)
    {
        string defaultJava = string.IsNullOrEmpty(GameDetails.JavaPath) ? "java" : GameDetails.JavaPath;
        if (string.IsNullOrEmpty(defaultJava))
            throw new InvalidOperationException("JavaPath was empty");

        _logger.Debug($"Forge processor java: {defaultJava}");
        
        // Constructs the combined classpath string.
        string combinedPath = string.Join(Path.PathSeparator.ToString(),
            classpath.Select(x =>
            {
                string path = Path.GetFullPath(x);
                if (path.Contains(' '))
                    return "\"" + path + "\"";
                return path;
            }));

        // Constructs the argument string for the Java process.
        _logger.Debug("Combined classpath: " + combinedPath);
        _logger.Debug("Main class: " + mainClass);
        string arg = $"-cp {combinedPath} {mainClass}";

        if (args is { Length: > 0 })
        {
            arg += " " + string.Join(" ", args);
            _logger.Debug("Arguments: " + arg.Replace(combinedPath, "[classpath_placeholder]"));
        }
        
        Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = defaultJava,
                Arguments = arg,
                RedirectStandardError = true
            }
        };
        process.Start();
        await process.WaitForExitAsync();
        
#if DEBUG
        string o = await process.StandardError.ReadToEndAsync();
        if (!string.IsNullOrEmpty(o))
            _logger.Error("Forge processor error: " + o);
#endif
    }
}