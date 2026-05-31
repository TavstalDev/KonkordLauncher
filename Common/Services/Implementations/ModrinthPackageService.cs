using System.IO.Compression;
using Newtonsoft.Json.Linq;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Common.Models.InstanceConfig;
using Tavstal.KonkordLauncher.Common.Models.Package;
using Tavstal.KonkordLauncher.Common.Models.Package.Modrinth;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Serialization;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Instance;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Common.Services.Implementations;

/// <summary>
/// Implements Modrinth package import and export operations for <c>.mrpack</c> archives.
/// </summary>
public class ModrinthPackageService : IPackageService
{
    private readonly ICustomLogger _logger;
    private readonly IHttpService _httpService;
    private readonly ILauncherStore _launcherStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModrinthPackageService"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for recording diagnostic, warning, and error messages.</param>
    /// <param name="httpService">Service for performing HTTP operations such as downloading files from URLs.</param>
    /// <param name="launcherStore">Service for accessing and modifying launcher data such as instances and settings.</param>
    public ModrinthPackageService(ICustomLogger<ModrinthPackageService> logger, IHttpService httpService, ILauncherStore launcherStore)
    {
        _logger = logger;
        _httpService = httpService;
        _launcherStore = launcherStore;
    }
    
    /// <inheritdoc/>
    public async Task<Instance?> ImportAsync(string sourcePath, Resolution resolution, string? customName = null, string? customGroup = null,
        string? customIconUrl = null, IProgressReporter? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(sourcePath) || Path.GetExtension(sourcePath) != ".mrpack")
            {
                _logger.LogError("Source path does not exist or is not a .mrpack file");
                return null;
            }

            var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);
            var instances = await _launcherStore.GetInstancesAsync(cancellationToken);
            Instance result = new Instance
            {
                Id = Guid.NewGuid().ToString(),
                Name = string.Empty,
                MinecraftVersion = string.Empty,
                Kind = EMinecraftKind.VANILLA,
                Config = new InstanceConfig
                {
                    Game = new InstanceGameConfig
                    {
                        StartMaximized = settings.Minecraft.StartMaximized,
                        WindowHeight = resolution.Y,
                        WindowWidth = resolution.X,
                        ShowConsoleWhenGameCrashes = true,
                        ShowConsoleWhileGameRunning = false,
                        CloseConsoleOnGameExit = false,
                        EnableFeralGameMode = settings.Misc.EnableFeralGameMode,
                        EnableMangoHud = settings.Misc.EnableMangoHud,
                        UseDedicatedGpu = settings.Misc.UseDedicatedGpu 
                    },
                    Java = new JavaConfig
                    {
                        JvmArguments = string.IsNullOrEmpty(settings.Java.JvmArguments) ? Instance.GetDefaultJVMArgs() : settings.Java.JvmArguments,
                        JavaPath = "LAUNCH_ME_FIRST",
                        MinMemory = settings.Java.MinMemory,
                        MaxMemory = settings.Java.MaxMemory,
                        PermaGen = settings.Java.PermaGen,
                    },
                    Commands = new InstanceCommandsConfig(),
                    EnableEnvironment = false,
                    Environment = [],
                    Misc =new InstanceMiscConfig()
                }
            };
            
            string tempDir = Path.Combine(PathHelper.TempDir, "import-" + Guid.NewGuid());
            try
            {
                Directory.CreateDirectory(tempDir);
                await ZipFile.ExtractToDirectoryAsync(sourcePath, tempDir, cancellationToken);

                string indexJsonPath = Path.Combine(tempDir, "modrinth.index.json");
                if (!File.Exists(indexJsonPath))
                {
                    _logger.LogError("modrinth.index.json not found in package");
                    return null;
                }
                
                JObject indexJson = JObject.Parse(await File.ReadAllTextAsync(indexJsonPath, cancellationToken));
                result.Name = customName ?? indexJson["name"]?.ToString() ?? "Unnamed Instance";
                result.Group = customGroup ?? null;
                
                var dependenices = indexJson["dependencies"];
                if (dependenices == null)
                {
                    _logger.LogError("'dependencies' not found in modrinth.index.json");
                    return null;
                }

                if (dependenices["minecraft"] == null)
                {
                    _logger.LogError("'minecraft' dependency not found in modrinth.index.json");
                    return null;
                }
                
                result.MinecraftVersion = dependenices["minecraft"]?.ToString()!;
                if (dependenices["fabric-loader"] != null)
                {
                    result.Kind = EMinecraftKind.FABRIC;
                    result.CustomVersion = dependenices["fabric-loader"]?.ToString()!;
                } 
                else if (dependenices["forge"] != null)
                {
                    result.Kind = EMinecraftKind.FORGE;
                    result.CustomVersion = dependenices["forge"]?.ToString()!;
                }
                else if (dependenices["neoforge"] != null)
                {
                    result.Kind = EMinecraftKind.NEOFORGE;
                    result.CustomVersion = dependenices["neoforge"]?.ToString()!;
                }
                else if (dependenices["quilt-loader"] != null)
                {
                    result.Kind = EMinecraftKind.QUILT;
                    result.CustomVersion = dependenices["quilt-loader"]?.ToString()!;
                }

                result.GameDirectory = Path.Combine(settings.Launcher.InstancesDirectoryPath, result.Name);
                Directory.CreateDirectory(result.GameDirectory);
                string resourceFile = result.GetResourceConfigPath();
                List<InstanceResource> resources = [];
                
                // Copy overrides
                string overridesDir = Path.Combine(tempDir, "overrides");
                if (Directory.Exists(overridesDir))
                    FileSystemHelper.MoveDirectory(overridesDir, result.GameDirectory, true);

                string iconPath = Path.Combine(result.GameDirectory, "icon.png");
                if (!File.Exists(iconPath) && !string.IsNullOrEmpty(customIconUrl))
                    await _httpService.DownloadFileAsync(customIconUrl, iconPath, null, cancellationToken);
                result.IconPath = File.Exists(iconPath) ? iconPath : string.Empty;
                
                // Download Mods
                var tasks = indexJson["files"]?.Select(f =>
                {
                    string? env = f["env"]?["client"]?.ToString();
                    if (!string.IsNullOrEmpty(env) && env != "required")
                        return Task.CompletedTask;
                    
                    string? url = f["downloads"]?.FirstOrDefault()?.Value<string>();
                    if (string.IsNullOrEmpty(url))
                    {
                        _logger.LogWarning("No download URL found for a file in modrinth.index.json");
                        return Task.CompletedTask;
                    }
                    
                    string? path = f["path"]?.ToString();
                    if (string.IsNullOrEmpty(path))
                    {
                        _logger.LogWarning("No path found for a file in modrinth.index.json");
                        return Task.CompletedTask;
                    }
                    
                    string finalPath = Path.Combine(result.GameDirectory, path);
                    string fileName =  Path.GetFileName(finalPath);
                    string? directory = Path.GetDirectoryName(finalPath);
                    
                    var prog = new Progress<double>(p =>
                    {
                        progress?.ReportProgress(p);
                        progress?.UpdateStatusTranslated("instance.download.file", path, p.ToString("0.00"));
                    });
                    
                    EResourceType resourceType = EResourceType.RESOURCE_PACK;
                    if (path.StartsWith("mods"))
                        resourceType = EResourceType.MOD;
                    else if (path.StartsWith("shader"))
                        resourceType = EResourceType.SHADER_PACK;
                    
                    resources.Add(new InstanceResource
                    {
                        ProjectId = url.Replace("https://cdn.modrinth.com/data/", "").Split('/').FirstOrDefault() ?? string.Empty,
                        Name = fileName,
                        Path = path,
                        Url = url,
                        Type = resourceType,
                        Platform = EPlatformType.MODRINTH,
                        FileSize = f["size"]?.ToObject<long>() ?? 0,
                        Sha1 = f["hashes"]?["sha1"]?.ToString() ?? string.Empty,
                        Sha512 = f["hashes"]?["sha512"]?.ToString() ?? string.Empty,
                        Client = f["env"]?["client"]?.ToString() ?? string.Empty,
                        Server = f["env"]?["server"]?.ToString() ?? string.Empty,
                    });
                    
                    if (!string.IsNullOrEmpty(directory))
                        Directory.CreateDirectory(directory);
                    
                    return _httpService.DownloadFileAsync(url, finalPath, prog, cancellationToken);
                });
                
                if (tasks != null)
                    await Task.WhenAll(tasks);

                instances.Add(result);
                await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherInstancesPath, instances, cancellationToken);
                if (resources.Count > 0)
                    await JsonHelper.WriteJsonFileAsync(resourceFile, resources, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing modrinth package:");
                return null;
            }
            finally
            {
                FileSystemHelper.DeleteDirectory(tempDir);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, $"Failed to import modrinth package:");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ExportAsync(Instance instance, List<FileNode> fileNodes, string targetPath, string exportVersion = "1.0.0",
        string summary = "", IProgressReporter? progress = null, CancellationToken cancellationToken = default)
    {
        string resourceFile = instance.GetResourceConfigPath();
        List<InstanceResource> resources = [];
        List<FileNode> localNodes = new (fileNodes);
        if (File.Exists(resourceFile))
        {
            var res = await JsonHelper.ReadJsonFileAsync<List<InstanceResource>>(resourceFile);
            if (res != null)
                resources = res;
        }
        
        string tmpDir = Path.Combine(PathHelper.TempDir, "export-" + Guid.NewGuid());
        Directory.CreateDirectory(tmpDir);
        try
        {   
            string indexJson = Path.Combine(tmpDir, "modrinth.index.json");
            string overridesDir = Path.Combine(tmpDir, "overrides");
            Directory.CreateDirectory(overridesDir);

            ModrinthPackageIndex packageIndex = new()
            {
                Name = instance.Name,
                VersionId = exportVersion,
                Summary = summary,
                Dependencies =
                {
                    ["minecraft"] = instance.MinecraftVersion
                }
            };
            
            if (!string.IsNullOrEmpty(instance.CustomVersion))
            {
                switch (instance.Kind)
                {
                    case EMinecraftKind.FABRIC:
                        packageIndex.Dependencies["fabric-loader"] = instance.CustomVersion;
                        break;
                    case EMinecraftKind.FORGE:
                        packageIndex.Dependencies["forge"] = instance.CustomVersion;
                        break;
                    case EMinecraftKind.NEOFORGE:
                        packageIndex.Dependencies["neoforge"] = instance.CustomVersion;
                        break;
                    case EMinecraftKind.QUILT:
                        packageIndex.Dependencies["quilt-loader"] = instance.CustomVersion;
                        break;
                }
            }

            foreach (var resource in resources)
            {
                // Remove the resource file node from localNodes to avoid copying it to overrides
                var localNode = localNodes.Find(x => !x.IsDirectory && x.Path.EndsWith(resource.Path));
                if (localNode != null)
                    localNodes.Remove(localNode);
                
                packageIndex.Files.Add(new PackageFile
                {
                    Path = resource.Path,
                    Downloads = [resource.Url],
                    FileSize = resource.FileSize,
                    Hashes =
                    {
                        ["sha1"] = resource.Sha1!,
                        ["sha512"] = resource.Sha512!
                    },
                    Env =
                    {
                        ["client"] = resource.Client!,
                        ["server"] = resource.Server!
                    }
                });
            }

            // Copy overrides
            foreach (var node in localNodes)
                await CopyNodeToOverridesAsync(overridesDir, instance.GameDirectory!, node, cancellationToken);
            
            // Write package index
            await JsonHelper.WriteJsonFileAsync(indexJson, packageIndex, cancellationToken);
            
            await ZipFile.CreateFromDirectoryAsync(tmpDir, targetPath, CompressionLevel.Optimal, false, cancellationToken);
            _logger.LogDebug($"Exported modrinth package to {targetPath}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to export modrinth package:");
            return false;
        }
        finally
        {
            FileSystemHelper.DeleteDirectory(tmpDir);
        }
    }
    
    /// <summary>
    /// Recursively copies a file node and its children from the game directory to the package's overrides directory.
    /// </summary>
    /// <param name="overridesDir">The root overrides directory inside the temporary package layout where files are copied.</param>
    /// <param name="gameDir">The base game directory path used to compute relative override paths.</param>
    /// <param name="fileNode">The file or directory node to copy. It may be a file or directory with children.</param>
    /// <param name="cancellationToken">Cancellation token observed during the copy operation.</param>
    /// <returns>A task that completes when the node and all its children have been copied.</returns>
    private static async Task CopyNodeToOverridesAsync(string overridesDir, string gameDir, FileNode fileNode, CancellationToken cancellationToken  = default)
    {
        string relativePath = fileNode.Path.Replace(gameDir + Path.DirectorySeparatorChar, "");
        string overridePath = Path.Combine(overridesDir, relativePath);

        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException();

        if (fileNode.IsDirectory)
        {
            Directory.CreateDirectory(overridePath);
            foreach (var child in fileNode.Children)
                await CopyNodeToOverridesAsync(overridesDir, gameDir, child, cancellationToken);
            return;
        }
        
        string? parentDir = Path.GetDirectoryName(overridePath);
        if (!string.IsNullOrEmpty(parentDir))
            Directory.CreateDirectory(parentDir);
        File.Copy(fileNode.Path, overridePath, true);
    }
}