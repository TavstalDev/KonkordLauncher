using System.IO.Compression;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Network;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints.Modding;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.Instance;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge.Modern;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;

namespace Tavstal.KonkordLauncher.Core.Instances.Forge;

// 1.12.2+
public class ForgeModernInstance(
    string id,
   GameDetails gameDetails,
   PathDetails pathDetails,
   LauncherDetails launcherDetails,
   ClientDetails clientDetails,
   Resolution? resolution = null,
   IProgressReporter? progressReporter = null)
   : ForgeInstanceBase(id, gameDetails, pathDetails, launcherDetails, clientDetails, resolution, progressReporter)
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(ForgeModernInstance));

    public override async Task<ModdedData?> InstallModdedAsync(string tempDir, CancellationToken cancellationToken = default)
    {
        if (ArgumentBuilder == null)
            throw new InvalidOperationException($"{nameof(ArgumentBuilder)} is null.");
        
        if (!File.Exists(PathDetails.CustomManifestPath))
        {
            _logger.Error("Forge manifest file does not exist. Please ensure the manifest is downloaded.");
            return null;
        }

        List<LibraryMeta> localLibraries = [];

        // Create versionDir in the versions folder
        Directory.CreateDirectory(VersionData.CustomVersionDirectory!);

        // Download & Extract Installer
        string installerJarPath = Path.Combine(tempDir, "installer.jar");
        string installerDir = Path.Combine(tempDir, "installer");
        string installerProfilePath = Path.Combine(VersionData.CustomVersionDirectory!, "install_profile.json");
        if (!File.Exists(VersionData.CustomJsonPath))
        {
            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (_, e) =>
            {
                _progressReporter?.UpdateStatusTranslated("instance.downloading.installer", "forge",
                    e.ToString("0.00"));
            };
       
            await HttpHelper.DownloadFileAsync(
                string.Format(ForgeEndpoints.InstallerJarUrl, $"{VersionData.MinecraftVersion}-{VersionData.CustomVersion}"), installerJarPath,
                progress, cancellationToken);
       
            // Extract Installer
            _progressReporter?.UpdateStatusTranslated("instance.extracting.installer", "forge");
            await ZipFile.ExtractToDirectoryAsync(installerJarPath, installerDir, cancellationToken);
            
            // Move install_profile.json
            var source = Path.Combine(installerDir, "install_profile.json");
            if (File.Exists(source))
                File.Move(source, installerProfilePath);
            else
                _logger.Error("Install profile JSON file not found in the forge installer directory.");
            
            // Move version.json
            source = Path.Combine(installerDir, "version.json");
            if (File.Exists(source))
                File.Move(source, VersionData.CustomJsonPath!);
            else
                _logger.Error("Install version JSON file not found in the forge installer directory.");
            
            // Extract Maven
            source = Path.Combine(installerDir, "maven");
            if (Directory.Exists(source))
                FileSystemHelper.MoveDirectory(source, PathDetails.LibrariesDir, true, false);
        }
        
        // Read Forge Version Meta
        var rawForgeVersionMeta = await File.ReadAllTextAsync(VersionData.CustomJsonPath!, cancellationToken);
        var forgeVersionMeta = JsonConvert.DeserializeObject<ForgeVersionMeta>(rawForgeVersionMeta);
        if (forgeVersionMeta == null)
            throw new FileNotFoundException("Failed to get the forge version meta.");
        
        // Install libraries from Forge Version Meta
        localLibraries.AddRange(forgeVersionMeta.Libraries);

        // Read Forge Install Profile
        var rawInstallProfile = await File.ReadAllTextAsync(installerProfilePath, cancellationToken);
        var installProfile = JsonConvert.DeserializeObject<ForgeVersionProfile>(rawInstallProfile);
        if (installProfile == null)
            throw new FileNotFoundException("Failed to get the forge install profile meta.");
        
        // Install Libraries From Install Profile
        _progressReporter?.UpdateStatusTranslated("instance.reading.libraries");
        foreach (var libMeta in installProfile.Libraries)
        {
            if (libMeta.Downloads.Artifact == null)
            {
                _logger.Warn($"Library {libMeta.Name} does not have an artifact to download.");
                continue;
            }

            string localPath = libMeta.Downloads.Artifact.Path;
            string libraryDir = Path.Combine(PathDetails.LibrariesDir,
                localPath.Remove(localPath.LastIndexOf('/'), localPath.Length - localPath.LastIndexOf('/')));
            
            if (!Directory.Exists(libraryDir))
                Directory.CreateDirectory(libraryDir);

            string libraryPath = Path.Combine(PathDetails.LibrariesDir, localPath);
            if (File.Exists(libraryPath))
                continue;
            
            if (string.IsNullOrEmpty(libMeta.Downloads.Artifact.Url))
            {
                _logger.Warn($"Library {libMeta.Name} does not have a download URL.");
                continue;
            }
            
            Progress<double> libProgress = new Progress<double>();
            libProgress.ProgressChanged += (_, e) =>
            {
                _progressReporter?.UpdateStatusTranslated("instance.downloading.libraries", libMeta.Name,
                    e.ToString("0.00"));
            };

            await HttpHelper.DownloadFileAsync(libMeta.Downloads.Artifact.Url, libraryPath, libProgress, cancellationToken);
        }

        // Map and start processors
        _progressReporter?.UpdateStatusTranslated("instance.building", "forge", 0);
        if (File.Exists(installerJarPath))
            await MapAndStartProcessors(installProfile, installerDir);
        
        // Copy Vanilla Jar
        if (!File.Exists(VersionData.CustomJarPath))
            File.Copy(VersionData.VanillaJarPath, VersionData.CustomJarPath!, true);

        // Add launch arguments
        _progressReporter?.UpdateStatusTranslated("instance.building.arguments");
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (forgeVersionMeta.Arguments != null)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (forgeVersionMeta.Arguments.Game != null)
                foreach (var arg in forgeVersionMeta.Arguments.GetGameArgs())
                    ArgumentBuilder.AddGameArgument(new LaunchArg(arg, 1));
            
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (forgeVersionMeta.Arguments.Jvm != null)
                foreach (var arg in forgeVersionMeta.Arguments.GetJvmArgs())
                    ArgumentBuilder.AddJvmArgument(new LaunchArg(arg, 1));
        }

        if (!string.IsNullOrEmpty(forgeVersionMeta.MinecraftArguments))
        {
            // Only 1.12.2 has this field, and
            // it contains many duplicate vanilla arguments
            // so this is the easiest way to handle it
            ArgumentBuilder.AddGameArgument(new LaunchArg("--tweakClass net.minecraftforge.fml.common.launcher.FMLTweaker", 1));
        }
        
        ArgumentBuilder.AddJvmArgumentBeforeClassPath(new LaunchArg($"-DlibraryDirectory={PathDetails.LibrariesDir}", 1));
        
        return new ModdedData(forgeVersionMeta.MainClass, localLibraries);
    }
}