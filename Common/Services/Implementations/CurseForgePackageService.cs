using System.IO.Compression;
using System.Text.Json.Nodes;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Common.Models.InstanceConfig;
using Tavstal.KonkordLauncher.Common.Models.Json;
using Tavstal.KonkordLauncher.Common.Models.Package;
using Tavstal.KonkordLauncher.Common.Models.Package.CurseForge;
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
/// Implements CurseForge package import and export operations for <c>.zip</c> archives.
/// </summary>
public class CurseForgePackageService : IPackageService
{
    private readonly ICustomLogger _logger;
    private readonly IHttpService _httpService;
    private readonly ILauncherStore _launcherStore;
    
    public CurseForgePackageService(ICustomLogger<CurseForgePackageService> logger, IHttpService httpService, ILauncherStore launcherStore)
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
            if (!File.Exists(sourcePath) || Path.GetExtension(sourcePath) != ".zip")
            {
                _logger.LogError("Source path does not exist or is not a .zip file");
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
                        JavaPath = string.Empty,
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

                string manifestJson = Path.Combine(tempDir, "manifest.json");
                if (!File.Exists(manifestJson))
                {
                    _logger.LogError("manifest.json not found in package");
                    return null;
                }
                
                var curseForgeManifest = await JsonHelper.ReadJsonFileAsync(manifestJson, CurseForgeJsonContext.Default.CurseForgeManifest, cancellationToken);
                if (curseForgeManifest == null)
                    throw new InvalidOperationException("Failed to parse manifest.json");
                
                result.Name = customName ?? curseForgeManifest.Name;
                result.Group = customGroup ?? null;

                result.GameDirectory = Path.Combine(settings.Launcher.InstancesDirectoryPath, result.Name);
                Directory.CreateDirectory(result.GameDirectory);
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
                /* TODO: Implement 
                 var tasks = curseForgeManifest.Files.Select(f =>
                {
                    resources.Add(new InstanceResource
                    {
                        ProjectId = f.ProjectId.ToString(),
                        Name = "",
                        Path = "",
                        Url = $"https://api.curseforge.com/v1/mods/{f.ProjectId}/files/{f.FileId}",
                        Type = resourceType,
                        Platform = EPlatformType.MODRINTH,
                    });
                    
                    if (!string.IsNullOrEmpty(directory))
                        Directory.CreateDirectory(directory);
                    
                    var prog = new Progress<double>(p =>
                    {
                        progress?.ReportProgress(p);
                        progress?.UpdateStatusTranslated("instance.download.file", path, p.ToString("0.00"));
                    });
                    return _httpService.DownloadFileAsync(url, finalPath, prog, cancellationToken);
                });
                
                await Task.WhenAll(tasks);*/

                instances.Add(result);
                await _launcherStore.SaveInstancesAsync(instances, cancellationToken);
                if (resources.Count > 0)
                    await _launcherStore.SaveInstanceResourcesAsync(result, resources, cancellationToken);
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
            _logger.LogCritical(ex, $"Failed to import curse forge package:");
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
            var res = await JsonHelper.ReadJsonFileAsync<List<InstanceResource>>(resourceFile, CommonJsonContex.Default.ListInstanceResource, cancellationToken);
            if (res != null)
                resources = res;
        }
        
        string tmpDir = Path.Combine(PathHelper.TempDir, "export-" + Guid.NewGuid());
        Directory.CreateDirectory(tmpDir);
        try
        {
            string manifestJson = Path.Combine(tmpDir, "manifest.json");
            string modListHtml = Path.Combine(tmpDir, "modlist.html");
            string overridesDir = Path.Combine(tmpDir, "overrides");
            Directory.CreateDirectory(overridesDir);
            
            CurseForgeManifest curseForgeManifest = new()
            {
                Name = instance.Name,
                Files = [],
                Version = exportVersion,
                Overrides = "overrides",
                Minecraft = new CurseForgeMinecraft
                {
                    Version = instance.MinecraftVersion,
                    ModLoaders = []
                }
            };
            if (instance.Kind != EMinecraftKind.VANILLA)
                curseForgeManifest.Minecraft.ModLoaders.Add(new CurseForgeModLoader
                {
                    Id = instance.Kind.ToString().ToLower() + "-" + instance.CustomVersion,
                    IsPrimary = true
                });
            
            // Write manifest json
            await JsonHelper.WriteJsonFileAsync(manifestJson, curseForgeManifest, CurseForgeJsonContext.Default.CurseForgeManifest, cancellationToken);

            // Write mod-list HTML
            List<string> modList = [];
            foreach (var resource in resources)
            {
                if (resource.Platform != EPlatformType.CURSE_FORGE)
                    continue;
                
                // Remove the resource file node from localNodes to avoid copying it to overrides
                var localNode = localNodes.Find(x => !x.IsDirectory && (x.Path.EndsWith(resource.Path) || x.Path.EndsWith(resource.Path + ".dis")));
                bool required = true;
                if (localNode != null)
                {
                    required = !localNode.Path.EndsWith(".dis");
                    localNodes.Remove(localNode);
                }
                
                modList.Add(
                    $"<li><a href=\"https://www.curseforge.com/projects/{resource.ProjectId}\">{resource.Name}</a></li>");

                if (resource.FileId == null)
                    continue;
                
                curseForgeManifest.Files.Add(new CurseForgeFile
                {
                    ProjectId = ulong.Parse(resource.ProjectId),
                    FileId = ulong.Parse(resource.FileId),
                    Required = required
                });
            }
            await File.WriteAllTextAsync(modListHtml, $"<ul>{string.Join("\n", modList)}</ul>", cancellationToken);

            // Copy overrides
            foreach (var node in localNodes)
                await CopyNodeToOverridesAsync(overridesDir, instance.GameDirectory!, node, cancellationToken);
            
            await ZipFile.CreateFromDirectoryAsync(tmpDir, targetPath, CompressionLevel.Optimal, false, cancellationToken);
            _logger.LogDebug($"Exported modrinth package to {targetPath}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to export curse forge package:");
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