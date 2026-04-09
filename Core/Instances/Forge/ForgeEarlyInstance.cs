using System.IO.Compression;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints.Modding;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge;

namespace Tavstal.KonkordLauncher.Core.Instances.Forge;

// 1.1 - 1.5.2
public class ForgeEarlyInstance(string forgeVersionName, string universalName,
    GameDetails gameDetails,
    PathDetails pathDetails,
    LauncherDetails launcherDetails,
    ClientDetails clientDetails,
    Resolution? resolution = null,
    IProgressReporter? progressReporter = null)
    : ForgeInstanceBase(gameDetails, pathDetails, launcherDetails, clientDetails, resolution, progressReporter)
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(ForgeClassicInstance));
    
    protected override async Task<ModdedData?> InstallModdedAsync(string tempDir, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(PathDetails.CustomManifestPath))
        {
            _logger.Error("Forge manifest file does not exist. Please ensure the manifest is downloaded.");
            return null;
        }

        VersionDetails forgeVersion = GameHelper.GetVersionDetails(PathDetails.VersionsDir, MinecraftVersion.Id,
            EMinecraftKind.FORGE, GameDetails.CustomVersion, GameDetails.CustomGameDirectory);

        // Create versionDir in the versions folder
        if (!Directory.Exists(forgeVersion.VersionDirectory))
            Directory.CreateDirectory(forgeVersion.VersionDirectory);
        
        // Download Universal Zip
        string universalZipPath = Path.Combine(tempDir, "universal.zip");
        //string universalExtractDir = Path.Combine(tempDir, $"universal-{forgeVersionName}");
        if (!File.Exists(forgeVersion.VersionJarPath))
        {
            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (_, e) =>
            {
                _progressReporter?.UpdateStatusTranslated("instance.downloading.installer", "forge",
                    e.ToString("0.00"));
            };
       
            await HttpHelper.DownloadFileAsync(
                string.Format(ForgeEndpoints.LoaderUniversalZipUrl, forgeVersionName, universalName), universalZipPath,
                progress);
            
            // Create vanilla extract directory
            string vanillaExtractDir = Path.Combine(tempDir, "vanilla_extract");
            if (!Directory.Exists(vanillaExtractDir))
                Directory.CreateDirectory(vanillaExtractDir);
            
            // Extract vanilla jar
            ZipFile.ExtractToDirectory(forgeVersion.VanillaJarPath, vanillaExtractDir);
            var vanillaMetaDir = Path.Combine(vanillaExtractDir, "META-INF");
            if (Directory.Exists(vanillaMetaDir))
                FileSystemHelper.DeleteDirectory(vanillaMetaDir);
            else
                _logger.Warn("META-INF directory not found in the vanilla jar. This may indicate an issue with the jar file.");
            
            // Extract universal jar
            ZipFile.ExtractToDirectory(universalZipPath, vanillaExtractDir, true);
            string patchedVanillaJarPath = Path.Combine(tempDir, "patched_vanilla.jar");
            ZipFile.CreateFromDirectory(vanillaExtractDir, patchedVanillaJarPath);
            
            File.Copy(patchedVanillaJarPath, forgeVersion.VersionJarPath);
        }

        ModdedData moddedData = new ModdedData(null, forgeVersion, []);
        var libraries = ForgeInstance.GetLegacyLibraries(GameDetails.MinecraftVersion);
        if (libraries.Count == 0)
            return moddedData;
        
        string forgeLibDir = Path.Combine(forgeVersion.GameDir, "lib");
        if (!Directory.Exists(forgeLibDir))
            Directory.CreateDirectory(forgeLibDir);
        
        // Copy legacy libraries to the forge lib directory
        foreach (var library in libraries)
        {
            string libraryFileName = library.Replace("Tavstal.KonkordLauncher.Core.Assets.Fmllib.", "");
            string libraryTargetPath = Path.Combine(forgeLibDir, libraryFileName);
            
            if (File.Exists(libraryTargetPath))
                continue;
            
            var stream = this.GetType().Assembly.GetManifestResourceStream(library);
            if (stream == null)
            {
                _logger.Error($"Failed to get resource stream for {library}");
                continue;
            }

            await using FileStream outFile = new FileStream(libraryTargetPath, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(outFile);
        }
       
        return moddedData;
    }
}