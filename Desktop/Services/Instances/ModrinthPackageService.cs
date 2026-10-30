using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Tavstal.BeaconSDK.Enums;
using Tavstal.BeaconSDK.Helpers.IO;
using Tavstal.BeaconSDK.Helpers.Serialization;
using Tavstal.BeaconSDK.Models.Instance;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs.Config;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs.InstanceConfig;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs.Package;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs.Package.Modrinth;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Serialization;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;
using ModrinthJsonContext = Tavstal.KonkordLauncher.Desktop.Serialization.ModrinthJsonContext;

namespace Tavstal.KonkordLauncher.Desktop.Services.Instances;

/// <summary>
///     Implements Modrinth package import and export operations for <c>.mrpack</c> archives.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class ModrinthPackageService : IPackageService
{
    private readonly IHttpService _httpService;
    private readonly ILauncherStore _launcherStore;
    private readonly IBeaconLogger _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ModrinthPackageService" /> class.
    /// </summary>
    /// <param name="logger">Logger instance for recording diagnostic, warning, and error messages.</param>
    /// <param name="httpService">Service for performing HTTP operations such as downloading files from URLs.</param>
    /// <param name="launcherStore">Service for accessing and modifying launcher data such as instances and settings.</param>
    public ModrinthPackageService(IBeaconLogger<ModrinthPackageService> logger, IHttpService httpService,
        ILauncherStore launcherStore)
    {
        _logger = logger;
        _httpService = httpService;
        _launcherStore = launcherStore;
    }

    /// <inheritdoc />
    public async Task<Instance?> ImportAsync(string sourcePath, Resolution resolution, string? customName = null,
        string? customGroup = null,
        string? customIconUrl = null, IKonkordProgress? progress = null, CancellationToken cancellationToken = default)
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
            var result = new Instance
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
                        JvmArguments =
                            string.IsNullOrEmpty(settings.Java.JvmArguments)
                                ? Instance.GetDefaultJVMArgs()
                                : settings.Java.JvmArguments,
                        JavaPath = string.Empty,
                        MinMemory = settings.Java.MinMemory,
                        MaxMemory = settings.Java.MaxMemory,
                        PermaGen = settings.Java.PermaGen
                    },
                    Commands = new InstanceCommandsConfig(),
                    EnableEnvironment = false,
                    Environment = [],
                    Misc = new InstanceMiscConfig()
                }
            };

            var tempDir = Path.Combine(_launcherStore.GetTempDir(), "import-" + Guid.NewGuid());
            try
            {
                Directory.CreateDirectory(tempDir);
                await ZipFile.ExtractToDirectoryAsync(sourcePath, tempDir, cancellationToken);

                var indexJsonPath = Path.Combine(tempDir, "modrinth.index.json");
                if (!File.Exists(indexJsonPath))
                {
                    _logger.LogError("modrinth.index.json not found in package");
                    return null;
                }

                var indexJson = JsonNode.Parse(await File.ReadAllTextAsync(indexJsonPath, cancellationToken));
                if (indexJson == null)
                    throw new InvalidOperationException("Failed to parse index.json");

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
                List<InstanceResource> resources = [];

                // Copy overrides
                var overridesDir = Path.Combine(tempDir, "overrides");
                if (Directory.Exists(overridesDir))
                    FileSystemHelper.MoveDirectory(overridesDir, result.GameDirectory, true);

                var iconPath = Path.Combine(result.GameDirectory, "icon.png");
                if (!File.Exists(iconPath) && !string.IsNullOrEmpty(customIconUrl))
                    await _httpService.DownloadFileAsync(customIconUrl, iconPath, null, null, cancellationToken);
                result.IconPath = File.Exists(iconPath) ? iconPath : string.Empty;

                // Download Mods
                var validFilesData = indexJson["files"]?.AsArray()
                    .Where(x => x != null)
                    .Select(x =>
                    {
                        if (x == null)
                            return null;

                        var env = x["env"]?["client"]?.ToString();
                        if (!string.IsNullOrEmpty(env) && env != "required")
                            return null;

                        var url = x["downloads"]?.AsArray().FirstOrDefault()?.ToString();
                        if (string.IsNullOrEmpty(url))
                            return null;

                        var path = x["path"]?.ToString();
                        if (string.IsNullOrEmpty(path))
                            return null;

                        var urlParts = url.Replace("https://", "").Split('/');
                        var projetId = urlParts.Length - 1 >= 2 ? urlParts[2] : string.Empty;
                        var versionId = urlParts.Length - 1 >= 4 ? urlParts[4] : string.Empty;

                        return new
                        {
                            JsonNode = x,
                            Url = url,
                            Path = path,
                            ProjectId = projetId,
                            VersionId = versionId
                        };
                    })
                    .Where(x => x != null)
                    .ToList();

                List<Task> tasks = [];
                foreach (var node in validFilesData!)
                {
                    if (node == null)
                        continue;

                    var jsonNode = node.JsonNode;
                    var path = node.Path;
                    var url = node.Url;
                    var projetId = node.ProjectId;
                    var versionId = node.VersionId;

                    var finalPath = Path.Combine(result.GameDirectory, path);
                    var fileName = Path.GetFileName(finalPath);
                    var directory = Path.GetDirectoryName(finalPath);

                    var resourceType = EResourceType.RESOURCE_PACK;
                    if (path.StartsWith("mods"))
                        resourceType = EResourceType.MOD;
                    else if (path.StartsWith("shader"))
                        resourceType = EResourceType.SHADER_PACK;

                    resources.Add(new InstanceResource
                    {
                        ProjectId = projetId,
                        VersionId = versionId,
                        Name = fileName,
                        Path = path,
                        Url = url,
                        Type = resourceType,
                        Platform = EPlatformType.MODRINTH,
                        FileSize = long.Parse(jsonNode["fileSize"]?.ToString() ?? "0"),
                        Sha1 = jsonNode["hashes"]?["sha1"]?.ToString() ?? string.Empty,
                        Sha512 = jsonNode["hashes"]?["sha512"]?.ToString() ?? string.Empty,
                        Client = jsonNode["env"]?["client"]?.ToString() ?? string.Empty,
                        Server = jsonNode["env"]?["server"]?.ToString() ?? string.Empty
                    });

                    if (!string.IsNullOrEmpty(directory))
                        Directory.CreateDirectory(directory);

                    var prog = new Progress<double>(p =>
                    {
                        progress?.Report(p, "instance.download.file", path, p.ToString("0.00"));
                    });
                    tasks.Add(_httpService.DownloadFileAsync(url, finalPath, null, prog, cancellationToken));
                }

                await Task.WhenAll(tasks);

                instances.Add(result);
                await _launcherStore.SaveInstancesAsync(instances, cancellationToken);
                if (resources.Count > 0)
                    await _launcherStore.SaveInstanceResourcesAsync(result, resources, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing modrinth package:");
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
            _logger.LogDebug(ex, "Failed to import modrinth package:");
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExportAsync(Instance instance, List<FileNode> fileNodes, string targetPath,
        string exportVersion = "1.0.0",
        string summary = "", IKonkordProgress? progress = null, CancellationToken cancellationToken = default)
    {
        var resourceFile = instance.GetResourceConfigPath();
        List<InstanceResource> resources = [];
        List<FileNode> localNodes = [.. fileNodes];
        if (File.Exists(resourceFile))
        {
            var res = await JsonHelper.ReadJsonFileAsync<List<InstanceResource>>(resourceFile,
                CommonJsonContext.Default.ListInstanceResource, cancellationToken);
            if (res != null)
                resources = res;
        }

        var tmpDir = Path.Combine(_launcherStore.GetTempDir(), "export-" + Guid.NewGuid());
        Directory.CreateDirectory(tmpDir);
        try
        {
            var indexJson = Path.Combine(tmpDir, "modrinth.index.json");
            var overridesDir = Path.Combine(tmpDir, "overrides");
            Directory.CreateDirectory(overridesDir);

            ModrinthPackageIndex packageIndex = new()
            {
                Name = instance.Name,
                VersionId = exportVersion,
                Summary = summary,
                Dependencies = { ["minecraft"] = instance.MinecraftVersion }
            };

            if (!string.IsNullOrEmpty(instance.CustomVersion))
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

            foreach (var resource in resources)
            {
                if (resource.Platform != EPlatformType.MODRINTH)
                    continue;

                // Remove the resource file node from localNodes to avoid copying it to overrides
                var localNode = localNodes.Find(x =>
                    !x.IsDirectory && (x.Path.EndsWith(resource.Path) || x.Path.EndsWith(resource.Path + ".dis")));
                if (localNode != null)
                    localNodes.Remove(localNode);

                packageIndex.Files.Add(new PackageFile
                {
                    Path = resource.Path,
                    Downloads = [resource.Url],
                    FileSize = resource.FileSize,
                    Hashes = { ["sha1"] = resource.Sha1!, ["sha512"] = resource.Sha512! },
                    Env = { ["client"] = resource.Client!, ["server"] = resource.Server! }
                });
            }

            // Copy overrides
            foreach (var node in localNodes)
                await CopyNodeToOverridesAsync(overridesDir, instance.GameDirectory!, node, cancellationToken);

            // Write package index
            await JsonHelper.WriteJsonFileAsync(indexJson, packageIndex,
                ModrinthJsonContext.Default.ModrinthPackageIndex, cancellationToken);

            await ZipFile.CreateFromDirectoryAsync(tmpDir, targetPath, CompressionLevel.Optimal, false,
                cancellationToken);
            _logger.LogDebug($"Exported modrinth package to {targetPath}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to export modrinth package:");
            return false;
        }
        finally
        {
            FileSystemHelper.DeleteDirectory(tmpDir);
        }
    }

    /// <summary>
    ///     Recursively copies a file node and its children from the game directory to the package's overrides directory.
    /// </summary>
    /// <param name="overridesDir">The root overrides directory inside the temporary package layout where files are copied.</param>
    /// <param name="gameDir">The base game directory path used to compute relative override paths.</param>
    /// <param name="fileNode">The file or directory node to copy. It may be a file or directory with children.</param>
    /// <param name="cancellationToken">Cancellation token observed during the copy operation.</param>
    /// <returns>A task that completes when the node and all its children have been copied.</returns>
    private static async Task CopyNodeToOverridesAsync(string overridesDir, string gameDir, FileNode fileNode,
        CancellationToken cancellationToken = default)
    {
        var relativePath = fileNode.Path.Replace(gameDir + Path.DirectorySeparatorChar, "");
        var overridePath = Path.Combine(overridesDir, relativePath);

        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException();

        if (fileNode.IsDirectory)
        {
            Directory.CreateDirectory(overridePath);
            foreach (var child in fileNode.Children)
                await CopyNodeToOverridesAsync(overridesDir, gameDir, child, cancellationToken);
            return;
        }

        var parentDir = Path.GetDirectoryName(overridePath);
        if (!string.IsNullOrEmpty(parentDir))
            Directory.CreateDirectory(parentDir);
        File.Copy(fileNode.Path, overridePath, true);
    }
}
