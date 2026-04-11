using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.Domain;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Network;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints.Modding;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Fabric;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta.Library;

namespace Tavstal.KonkordLauncher.Core.Instances;

/// <summary>
/// Represents a Fabric instance, handling installation, configuration, and launching of Fabric-based Minecraft versions.
/// </summary>
public class FabricInstance(
    GameDetails gameDetails,
    PathDetails pathDetails,
    LauncherDetails launcherDetails,
    ClientDetails clientDetails,
    Resolution? resolution = null,
    IProgressReporter? progressReporter = null)
    : MinecraftInstance(gameDetails, pathDetails, launcherDetails, clientDetails, resolution, progressReporter)
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(FabricInstance));

    /// <summary>
    /// Installs the Fabric modded environment asynchronously.
    /// </summary>
    /// <param name="tempDir">The temporary directory used during installation.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the modded data if successful, or null if an error occurs.</returns>
    protected override async Task<ModdedData?> InstallModdedAsync(string tempDir, CancellationToken cancellationToken = default)
    {
        if (ArgumentBuilder == null)
            throw new InvalidOperationException($"{nameof(ArgumentBuilder)} is null.");
        
        _progressReporter?.UpdateStatusTranslated("instance.reading.manifest");
        if (!File.Exists(PathDetails.CustomManifestPath))
        {
            _logger.Error("Fabric manifest file not found at path: " + PathDetails.CustomManifestPath);
            return null;
        }

        VersionDetails fabricVersion = GameHelper.GetVersionDetails(PathDetails.VersionsDir, MinecraftVersion.Id, EMinecraftKind.FABRIC, GameDetails.CustomVersion, GameDetails.CustomGameDirectory);

        // Create versionDir in the versions folder
        if (!Directory.Exists(fabricVersion.VersionDirectory))
            Directory.CreateDirectory(fabricVersion.VersionDirectory);

        // Download version json
        FabricVersionMeta? fabricVersionMeta;
        List<LibraryMeta> localLibraries = new List<LibraryMeta>();

        if (!File.Exists(fabricVersion.VersionJsonPath))
        {
            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (_, e) =>
            {
                //_progressReporter?.SetProgress(e);
                _progressReporter?.UpdateStatusTranslated("instance.downloading.version_json", "fabric", e.ToString("0.00"));
            };

            string fabricVersionJsonUrl = string.Format(FabricEndpoints.LoaderJsonUrl, fabricVersion.MinecraftVersion,
                fabricVersion.CustomVersion);

            var resultJson = await HttpHelper.GetStringAsync(fabricVersionJsonUrl, progress, cancellationToken);
            if (resultJson == null)
                return null;
                
            await File.WriteAllTextAsync(fabricVersion.VersionJsonPath, resultJson, cancellationToken);

            // Add the libraries
            _progressReporter?.UpdateStatusTranslated("instance.reading.version_json");
            fabricVersionMeta = JsonConvert.DeserializeObject<FabricVersionMeta>(resultJson);
            if (fabricVersionMeta == null)
            {
                FileSystemHelper.DeleteFile(fabricVersion.VersionJsonPath); // Delete it because this if part won't be executed again if it exists
                _logger.Error("Fabric version meta is null after deserialization. Invalid JSON format.");
                return null;
            }
            
            foreach (var lib in fabricVersionMeta.Libraries)
                localLibraries.Add(new LibraryMeta(lib.Name, new LibraryDownloads(new Artifact(lib.GetPath(), lib.Sha1, lib.Size, lib.GetURL()), null), []));
        }
        else
        {
            _progressReporter?.UpdateStatusTranslated("instance.reading.version_json");
            fabricVersionMeta = JsonConvert.DeserializeObject<FabricVersionMeta>(await File.ReadAllTextAsync(fabricVersion.VersionJsonPath, cancellationToken));
            if (fabricVersionMeta == null)
            {
                _logger.Error("Fabric version meta is null after deserialization. Invalid JSON format.");
                return null;
            }

            foreach (var lib in fabricVersionMeta.Libraries)
                localLibraries.Add(new LibraryMeta(lib.Name, new LibraryDownloads(new Artifact(lib.GetPath(), lib.Sha1, lib.Size, lib.GetURL()), null), []));
        }


        // Download Loader
        string loaderDirPath = Path.Combine(PathDetails.LibrariesDir, "net", "fabricmc", "fabric-loader", fabricVersion.CustomVersion);
        string loaderJarPath = Path.Combine(loaderDirPath, $"fabric-loader-{fabricVersion.CustomVersion}.jar");
        if (!Directory.Exists(loaderDirPath))
            Directory.CreateDirectory(loaderDirPath);

        if (!File.Exists(loaderJarPath))
        {
            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (_, e) =>
            {
                _progressReporter?.ReportProgress(e);
                _progressReporter?.UpdateStatusTranslated("instance.downloading.loader", "fabric", e.ToString("0.00"));
            };
            _logger.Debug("Downloading fabric loader jar...");
            await HttpHelper.DownloadFileAsync(string.Format(FabricEndpoints.LoaderJarUrl, fabricVersion.CustomVersion), loaderJarPath, progress, cancellationToken);
        }
        
        if (!File.Exists(fabricVersion.VersionJarPath))
            File.Copy(fabricVersion.VanillaJarPath, fabricVersion.VersionJarPath);

        ModdedData moddedData = new ModdedData(fabricVersionMeta.MainClass, fabricVersion, localLibraries);

        foreach (var arg in fabricVersionMeta.Arguments.GetGameArgs())
            ArgumentBuilder.AddGameArgument(new LaunchArg(arg, 1));
        
        foreach (var arg in fabricVersionMeta.Arguments.GetJvmArgs())
        {
            // Fixes -DFabricMcEmu arg, without this Fabric does not load and instead the vanilla client will launch
            if (arg.StartsWith("-DFabricMcEmu="))
            {
                ArgumentBuilder.AddJvmArgument(new LaunchArg("-DFabricMcEmu=\"net.minecraft.client.main.Main\"", 1));
                continue;
            }

            ArgumentBuilder.AddJvmArgument(new LaunchArg(arg, 1));
        }
        
        return moddedData;
    }
}