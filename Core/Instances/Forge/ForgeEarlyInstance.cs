using System.IO.Compression;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints.Modding;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.Instance;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Core.Instances.Forge;

// 1.1 - 1.5.2
public class ForgeEarlyInstance(string forgeVersionName, string universalName,
    string id,
    MinecraftVersion gameVersion,
    GameDetails gameDetails,
    PathDetails pathDetails,
    LauncherDetails launcherDetails,
    ClientDetails clientDetails,
    Resolution? resolution = null,
    IProgressReporter? progressReporter = null)
    : ForgeInstanceBase(id, gameVersion, gameDetails, pathDetails, launcherDetails, clientDetails, resolution, progressReporter)
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(ForgeClassicInstance));
    
    public override async Task<ModdedData?> InstallModdedAsync(string tempDir, IHttpService httpService, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(PathDetails.CustomManifestPath))
        {
            _logger.Error("Forge manifest file does not exist. Please ensure the manifest is downloaded.");
            return null;
        }
        
        // Create versionDir in the versions folder
        Directory.CreateDirectory(VersionData.CustomVersionDirectory!);
        
        // Download Universal Zip
        string universalZipPath = Path.Combine(tempDir, "universal.zip");
        //string universalExtractDir = Path.Combine(tempDir, $"universal-{forgeVersionName}");
        if (!File.Exists(VersionData.CustomJsonPath))
        {
            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (_, e) =>
            {
                _progressReporter?.UpdateStatusTranslated("instance.downloading.installer", "forge",
                    e.ToString("0.00"));
            };
       
            await httpService.DownloadFileAsync(
                string.Format(ForgeEndpoints.LoaderUniversalZipUrl, forgeVersionName, universalName), universalZipPath,
                progress, cancellationToken);
            
            // Create vanilla extract directory
            string vanillaExtractDir = Path.Combine(tempDir, "vanilla_extract");
            if (!Directory.Exists(vanillaExtractDir))
                Directory.CreateDirectory(vanillaExtractDir);
            
            // Extract vanilla jar
            await ZipFile.ExtractToDirectoryAsync(VersionData.VanillaJarPath, vanillaExtractDir, cancellationToken);
            var vanillaMetaDir = Path.Combine(vanillaExtractDir, "META-INF");
            if (Directory.Exists(vanillaMetaDir))
                FileSystemHelper.DeleteDirectory(vanillaMetaDir);
            else
                _logger.Warn("META-INF directory not found in the vanilla jar. This may indicate an issue with the jar file.");
            
            // Extract universal jar
            await ZipFile.ExtractToDirectoryAsync(universalZipPath, vanillaExtractDir, true, cancellationToken);
            string patchedVanillaJarPath = Path.Combine(tempDir, "patched_vanilla.jar");
            await ZipFile.CreateFromDirectoryAsync(vanillaExtractDir, patchedVanillaJarPath, cancellationToken);
            
            File.Copy(patchedVanillaJarPath, VersionData.CustomJarPath!, true);
        }

        ModdedData moddedData = new ModdedData(null, []);
        var libraries = ForgeInstance.GetLegacyLibraries(GameDetails.MinecraftVersion);
        if (libraries.Count == 0)
            return moddedData;
        
        string forgeLibDir = Path.Combine(VersionData.GameDir, "lib");
        if (!Directory.Exists(forgeLibDir))
            Directory.CreateDirectory(forgeLibDir);
        
        // Copy legacy libraries to the forge lib directory
        foreach (var library in libraries)
        {
            string libraryFileName = library.Replace("Tavstal.KonkordLauncher.Core.Assets.Fmllib.", "");
            string libraryTargetPath = Path.Combine(forgeLibDir, libraryFileName);
            
            if (File.Exists(libraryTargetPath))
                continue;
            
            var stream = GetType().Assembly.GetManifestResourceStream(library);
            if (stream == null)
            {
                _logger.Error($"Failed to get resource stream for {library}");
                continue;
            }

            await using FileStream outFile = new FileStream(libraryTargetPath, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(outFile, cancellationToken);
        }
       
        return moddedData;
    }
}