using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints.Modding;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.Instance;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Fabric;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta.Library;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Core.Instances;

/// <summary>
/// An implementation of <see cref="MinecraftInstance"/> that installs and configures Quilt-based modded instances.
/// </summary>
/// <param name="id">Unique identifier for the instance.</param>
/// <param name="gameVersion">The vanilla <see cref="MinecraftVersion"/> used as the base for this instance.</param>
/// <param name="gameDetails">Details about the game/runtime configuration.</param>
/// <param name="pathDetails">Filesystem paths used by the instance (libraries, assets, etc.).</param>
/// <param name="launcherDetails">Launcher metadata and settings.</param>
/// <param name="clientDetails">Client-specific data (e.g. credentials).</param>
/// <param name="logger">Logger used for diagnostic messages.</param>
/// <param name="resolution">Optional default resolution for the instance.</param>
/// <param name="progressReporter">Optional progress reporter used to surface download/install progress to callers.</param>
public class QuiltInstance(
    string id,
    MinecraftVersion gameVersion,
    GameDetails gameDetails,
    PathDetails pathDetails,
    LauncherDetails launcherDetails,
    ClientDetails clientDetails,
    ICustomLogger logger,
    Resolution? resolution = null,
    IProgressReporter? progressReporter = null)
    : MinecraftInstance(id, gameVersion, gameDetails, pathDetails, launcherDetails, clientDetails, logger, resolution, progressReporter)
{
    
    /// <inheritdoc/>
    public override async Task<ModdedData?> InstallModdedAsync(string tempDir, IHttpService httpService, CancellationToken cancellationToken = default)
    {
        if (ArgumentBuilder == null)
            throw new InvalidOperationException($"{nameof(ArgumentBuilder)} is null.");
        
        _progressReporter?.UpdateStatusTranslated("instance.reading.manifest");
        if (!File.Exists(PathDetails.CustomManifestPath))
        {
            _logger.LogError("Quilt manifest file not found at path: " + PathDetails.CustomManifestPath);
            return null;
        }
        
        // Create versionDir in the versions folder
        Directory.CreateDirectory(VersionData.CustomVersionDirectory!);

        // Download version json
        FabricVersionMeta? quiltVersionMeta;
        List<LibraryMeta> localLibraries = [];

        if (!File.Exists(VersionData.CustomJsonPath))
        {
            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (_, e) =>
            {
                //_progressReporter?.SetProgress(e);
                _progressReporter?.UpdateStatusTranslated("instance.downloading.version_json", "quilt", e.ToString("0.00"));
            };

            string quiltVersionJsonUrl = string.Format(QuiltEndpoints.LoaderJsonUrl, VersionData.MinecraftVersion,
                VersionData.CustomVersion);

            var resultJson = await httpService.GetStringAsync(quiltVersionJsonUrl, progress, cancellationToken);
            if (resultJson == null)
                return null;
                
            await File.WriteAllTextAsync(VersionData.CustomJsonPath!, resultJson, cancellationToken);

            // Add the libraries
            _progressReporter?.UpdateStatusTranslated("instance.reading.version_json");
            quiltVersionMeta = JsonConvert.DeserializeObject<FabricVersionMeta>(resultJson);
            if (quiltVersionMeta == null)
            { 
                 FileSystemHelper.DeleteFile(VersionData.CustomJsonPath!); // Delete it because this if part won't be executed again if it exists
                _logger.LogError("Quilt version meta is null after deserialization. Invalid JSON format.");
                return null;
            }
            
            foreach (var lib in quiltVersionMeta.Libraries)
                localLibraries.Add(new LibraryMeta(lib.Name, new LibraryDownloads(new Artifact(lib.GetPath(), lib.Sha1, lib.Size, lib.GetURL()), null), []));
        }
        else
        {
            _progressReporter?.UpdateStatusTranslated("instance.reading.version_json");
            quiltVersionMeta = JsonConvert.DeserializeObject<FabricVersionMeta>(await File.ReadAllTextAsync(VersionData.CustomJsonPath, cancellationToken));
            if (quiltVersionMeta == null)
            {
                _logger.LogError("Quilt version meta is null after deserialization. Invalid JSON format.");
                return null;
            }

            foreach (var lib in quiltVersionMeta.Libraries)
            {
                localLibraries.Add(new LibraryMeta(lib.Name, new LibraryDownloads(new Artifact(lib.GetPath(), lib.Sha1, lib.Size, lib.GetURL()), null), []));
            }
        }


        // Download Loader
        string loaderDirPath = Path.Combine(PathDetails.LibrariesDir, "net", "quiltmc", "quilt-loader", VersionData.CustomVersion!);
        string loaderJarPath = Path.Combine(loaderDirPath, $"quilt-loader-{VersionData.CustomVersion}.jar");
        Directory.CreateDirectory(loaderDirPath);

        if (!File.Exists(loaderJarPath))
        {
            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (_, e) =>
            {
                _progressReporter?.ReportProgress(e);
                _progressReporter?.UpdateStatusTranslated("instance.downloading.loader", "quilt", e.ToString("0.00"));
            };
            _logger.LogDebug("Downloading quilt loader jar...");
            await httpService.DownloadFileAsync(string.Format(QuiltEndpoints.LoaderJarUrl, VersionData.CustomVersion), loaderJarPath, progress, cancellationToken);
        }
        
        
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
        
        return new ModdedData(quiltVersionMeta.MainClass, localLibraries);
    }
}