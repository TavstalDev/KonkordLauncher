using System.IO.Compression;
using Newtonsoft.Json.Linq;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Common.Models.InstanceConfig;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Network;
using Tavstal.KonkordLauncher.Core.Helpers.Serialization;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Common.Models.Package;

public class ModrinthPackageHandler: IInstancePackageHandler
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(ModrinthPackageHandler));
    /*
     * ARCHIVE LAYOUT:
     *  overrides - containing the game directory 
     *  modrinth.index.json - info about downloaded content from modrinth
     */
    
    public async Task<Instance?> ImportAsync(string sourcePath, Resolution resolution, string? customName = null, string? customGroup = null, IProgressReporter? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(sourcePath) ||  Path.GetExtension(sourcePath) != ".mrpack")
                return null;

            var settings = await LauncherHelper.GetLauncherSettingsAsync(cancellationToken);
            var instances = await LauncherHelper.GetInstancesAsync(cancellationToken);
            Instance result = new Instance
            {
                Id = Guid.NewGuid().ToString(),
                Type = EProfileType.CUSTOM,
                Kind = EMinecraftKind.VANILLA,
                Config = new InstanceConfig.InstanceConfig
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
            
            string tempDir = Path.Combine(PathHelper.TempDir, "import");
            try
            {
                Directory.CreateDirectory(tempDir);
                await ZipFile.ExtractToDirectoryAsync(sourcePath, tempDir, cancellationToken);

                string indexJsonPath = Path.Combine(tempDir, "modrinth.index.json");
                if (!File.Exists(indexJsonPath))
                {
                    _logger.Error("modrinth.index.json not found in package");
                    return null;
                }
                
                JObject indexJson = JObject.Parse(await File.ReadAllTextAsync(indexJsonPath, cancellationToken));
                result.Name = customName ?? indexJson["name"]?.ToString() ?? "Unnamed Instance";
                result.Group = customGroup ?? null;
                
                var dependenices = indexJson["dependencies"];
                if (dependenices == null)
                {
                    _logger.Error("'dependencies' not found in modrinth.index.json");
                    return null;
                }

                if (dependenices["minecraft"] == null)
                {
                    _logger.Error("'minecraft' dependency not found in modrinth.index.json");
                    return null;
                }
                
                result.MinecraftVersion = dependenices["minecraft"]?.ToString();
                if (dependenices["fabric-loader"] != null)
                {
                    result.Kind = EMinecraftKind.FABRIC;
                    result.CustomVersion = dependenices["fabric-loader"]?.ToString();
                } 
                else if (dependenices["forge"] != null)
                {
                    result.Kind = EMinecraftKind.FORGE;
                    result.CustomVersion = dependenices["forge"]?.ToString();
                }
                else if (dependenices["neoforge"] != null)
                {
                    result.Kind = EMinecraftKind.NEOFORGE;
                    result.CustomVersion = dependenices["neoforge"]?.ToString();
                }
                else if (dependenices["quilt-loader"] != null)
                {
                    result.Kind = EMinecraftKind.QUILT;
                    result.CustomVersion = dependenices["quilt-loader"]?.ToString();
                }

                result.GameDirectory = Path.Combine(settings.Launcher.InstancesDirectoryPath, result.Name);
                Directory.CreateDirectory(result.GameDirectory);
                
                // Copy overrides
                string overridesDir = Path.Combine(tempDir, "overrides");
                if (Directory.Exists(overridesDir))
                    FileSystemHelper.MoveDirectory(overridesDir, result.GameDirectory, true);

                string iconPath = Path.Combine(result.GameDirectory, "icon.png");
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
                        _logger.Warn("No download URL found for a file in modrinth.index.json");
                        return Task.CompletedTask;
                    }
                    
                    string? path = f["path"]?.ToString();
                    if (string.IsNullOrEmpty(path))
                    {
                        _logger.Warn("No path found for a file in modrinth.index.json");
                        return Task.CompletedTask;
                    }
                    
                    string finalPath = Path.Combine(result.GameDirectory, path);
                    string? directory = Path.GetDirectoryName(finalPath);
                    
                    var prog = new Progress<double>(p =>
                    {
                        progress?.ReportProgress(p);
                        progress?.UpdateStatusTranslated("instance.download.file", path, p.ToString("0.00"));
                    });

                    return Task.Run(async () =>
                    {
                        if (!string.IsNullOrEmpty(directory))
                            Directory.CreateDirectory(directory);

                        _logger.Info($"Downloading {url} to {finalPath}");
                        await HttpHelper.DownloadFileAsync(url, finalPath, prog, cancellationToken);
                    }, cancellationToken);
                });
                
                if (tasks != null)
                    await Task.WhenAll(tasks);
                
                instances.Add(result);
                await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherInstancesPath, instances, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error processing modrinth package: {ex}");
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
            _logger.Error($"Failed to import modrinth package: {ex}");
            return null;
        }
    }

    public async Task<bool> ExportAsync(Instance instance, string targetPath, IProgressReporter? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {

            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to export modrinth package: {ex}");
            return false;
        }
    }
}