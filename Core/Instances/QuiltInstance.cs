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
/// Represents a Quilt instance, handling installation, configuration, and launching of Quilt-based Minecraft versions.
/// </summary>
public class QuiltInstance(
    GameDetails gameDetails,
    PathDetails pathDetails,
    LauncherDetails launcherDetails,
    ClientDetails clientDetails,
    Resolution? resolution = null,
    IProgressReporter? progressReporter = null)
    : MinecraftInstance(gameDetails, pathDetails, launcherDetails, clientDetails, resolution, progressReporter)
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(QuiltInstance));

    /// <summary>
    /// Installs the Quilt modded environment asynchronously.
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
            _logger.Error("Quilt manifest file not found at path: " + PathDetails.CustomManifestPath);
            return null;
        }

        VersionDetails quiltVersion = GameHelper.GetVersionDetails(PathDetails.VersionsDir, MinecraftVersion.Id, EMinecraftKind.QUILT, GameDetails.CustomVersion, GameDetails.CustomGameDirectory);

        // Create versionDir in the versions folder
        if (!Directory.Exists(quiltVersion.VersionDirectory))
            Directory.CreateDirectory(quiltVersion.VersionDirectory);

        // Download version json
        FabricVersionMeta? quiltVersionMeta;
        List<LibraryMeta> localLibraries = [];

        if (!File.Exists(quiltVersion.VersionJsonPath))
        {
            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (_, e) =>
            {
                //_progressReporter?.SetProgress(e);
                _progressReporter?.UpdateStatusTranslated("instance.downloading.version_json", "quilt", e.ToString("0.00"));
            };

            string quiltVersionJsonUrl = string.Format(QuiltEndpoints.LoaderJsonUrl, quiltVersion.MinecraftVersion,
                quiltVersion.CustomVersion);

            var resultJson = await HttpHelper.GetStringAsync(quiltVersionJsonUrl, progress, cancellationToken);
            if (resultJson == null)
                return null;
                
            await File.WriteAllTextAsync(quiltVersion.VersionJsonPath, resultJson, cancellationToken);

            // Add the libraries
            _progressReporter?.UpdateStatusTranslated("instance.reading.version_json");
            quiltVersionMeta = JsonConvert.DeserializeObject<FabricVersionMeta>(resultJson);
            if (quiltVersionMeta == null)
            { 
                 FileSystemHelper.DeleteFile(quiltVersion.VersionJsonPath); // Delete it because this if part won't be executed again if it exists
                _logger.Error("Quilt version meta is null after deserialization. Invalid JSON format.");
                return null;
            }
            
            foreach (var lib in quiltVersionMeta.Libraries)
                localLibraries.Add(new LibraryMeta(lib.Name, new LibraryDownloads(new Artifact(lib.GetPath(), lib.Sha1, lib.Size, lib.GetURL()), null), []));
        }
        else
        {
            _progressReporter?.UpdateStatusTranslated("instance.reading.version_json");
            quiltVersionMeta = JsonConvert.DeserializeObject<FabricVersionMeta>(await File.ReadAllTextAsync(quiltVersion.VersionJsonPath, cancellationToken));
            if (quiltVersionMeta == null)
            {
                _logger.Error("Quilt version meta is null after deserialization. Invalid JSON format.");
                return null;
            }

            foreach (var lib in quiltVersionMeta.Libraries)
            {
                localLibraries.Add(new LibraryMeta(lib.Name, new LibraryDownloads(new Artifact(lib.GetPath(), lib.Sha1, lib.Size, lib.GetURL()), null), []));
            }
        }


        // Download Loader
        string loaderDirPath = Path.Combine(PathDetails.LibrariesDir, "net", "quiltmc", "quilt-loader", quiltVersion.CustomVersion);
        string loaderJarPath = Path.Combine(loaderDirPath, $"quilt-loader-{quiltVersion.CustomVersion}.jar");
        if (!Directory.Exists(loaderDirPath))
            Directory.CreateDirectory(loaderDirPath);

        if (!File.Exists(loaderJarPath))
        {
            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (_, e) =>
            {
                _progressReporter?.ReportProgress(e);
                _progressReporter?.UpdateStatusTranslated("instance.downloading.loader", "quilt", e.ToString("0.00"));
            };
            _logger.Debug("Downloading quilt loader jar...");
            await HttpHelper.DownloadFileAsync(string.Format(QuiltEndpoints.LoaderJarUrl, quiltVersion.CustomVersion), loaderJarPath, progress, cancellationToken);
        }
        

        ModdedData moddedData = new ModdedData(quiltVersionMeta.MainClass, quiltVersion, localLibraries);

        foreach (var arg in quiltVersionMeta.Arguments.GetGameArgs())
            ArgumentBuilder.AddGameArgument(new LaunchArg(arg, 1));
        
        foreach (var arg in quiltVersionMeta.Arguments.GetJvmArgs())
        {
            // Fixes -DFabricMcEmu arg, without this Quilt does not load and instead the vanilla client will launch
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