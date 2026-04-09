using System.IO.Compression;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints.Modding;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge.Modern;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;

namespace Tavstal.KonkordLauncher.Core.Instances.Forge;

// 1.12.2+
public class ForgeModernInstance(
   GameDetails gameDetails,
   PathDetails pathDetails,
   LauncherDetails launcherDetails,
   ClientDetails clientDetails,
   Resolution? resolution = null,
   IProgressReporter? progressReporter = null)
   : ForgeInstanceBase(gameDetails, pathDetails, launcherDetails, clientDetails, resolution, progressReporter)
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(ForgeModernInstance));

    protected override async Task<ModdedData?> InstallModdedAsync(string tempDir, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(PathDetails.CustomManifestPath))
        {
            _logger.Error("Forge manifest file does not exist. Please ensure the manifest is downloaded.");
            return null;
        }

        List<LibraryMeta> localLibraries = [];
        VersionDetails forgeVersion = GameHelper.GetVersionDetails(PathDetails.VersionsDir, MinecraftVersion.Id, EMinecraftKind.FORGE, GameDetails.CustomVersion, GameDetails.CustomGameDirectory);
        
        // Create versionDir in the versions folder
        if (!Directory.Exists(forgeVersion.VersionDirectory))
            Directory.CreateDirectory(forgeVersion.VersionDirectory);

        // Download & Extract Installer
        string installerJarPath = Path.Combine(tempDir, "installer.jar");
        string installerDir = Path.Combine(tempDir, "installer");
        string installerProfilePath = Path.Combine(forgeVersion.VersionDirectory, "install_profile.json");
        if (!File.Exists(forgeVersion.VersionJsonPath))
        {
            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (_, e) =>
            {
                _progressReporter?.UpdateStatusTranslated("instance.downloading.installer", "forge",
                    e.ToString("0.00"));
            };
       
            await HttpHelper.DownloadFileAsync(
                string.Format(ForgeEndpoints.InstallerJarUrl, $"{forgeVersion.MinecraftVersion}-{forgeVersion.CustomVersion}"), installerJarPath,
                progress);
       
            // Extract Installer
            _progressReporter?.UpdateStatusTranslated("instance.extracting.installer", "forge");
            ZipFile.ExtractToDirectory(installerJarPath, installerDir);
            
            // Move install_profile.json
            var source = Path.Combine(installerDir, "install_profile.json");
            if (File.Exists(source))
                File.Move(source, installerProfilePath);
            else
                _logger.Error("Install profile JSON file not found in the forge installer directory.");
            
            // Move version.json
            source = Path.Combine(installerDir, "version.json");
            if (File.Exists(source))
                File.Move(source, forgeVersion.VersionJsonPath);
            else
                _logger.Error("Install version JSON file not found in the forge installer directory.");
            
            // Extract Maven
            source = Path.Combine(installerDir, "maven");
            if (Directory.Exists(source))
                FileSystemHelper.MoveDirectory(source, PathDetails.LibrariesDir, true, false);
        }
        
        // Read Forge Version Meta
        var rawForgeVersionMeta = await File.ReadAllTextAsync(forgeVersion.VersionJsonPath);
        var forgeVersionMeta = JsonConvert.DeserializeObject<ForgeVersionMeta>(rawForgeVersionMeta);
        if (forgeVersionMeta == null)
            throw new FileNotFoundException("Failed to get the forge version meta.");
        
        // Install libraries from Forge Version Meta
        localLibraries.AddRange(forgeVersionMeta.Libraries);

        // Read Forge Install Profile
        var rawInstallProfile = await File.ReadAllTextAsync(installerProfilePath);
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

            await HttpHelper.DownloadFileAsync(libMeta.Downloads.Artifact.Url, libraryPath, libProgress);
        }

        // Map and start processors
        _progressReporter?.UpdateStatusTranslated("instance.building", "forge", 0);
        if (File.Exists(installerJarPath))
            await MapAndStartProcessors(installProfile, installerDir);
        
        // Copy Vanilla Jar
        if (!File.Exists(forgeVersion.VersionJarPath))
            File.Copy(forgeVersion.VanillaJarPath, forgeVersion.VersionJarPath);

        // Add launch arguments
        _progressReporter?.UpdateStatusTranslated("instance.building.arguments");
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (forgeVersionMeta.Arguments != null)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (forgeVersionMeta.Arguments.Game != null)
                foreach (var arg in forgeVersionMeta.Arguments.GetGameArgs())
                    _gameArguments.Add(new LaunchArg(arg, 1));
            
            bool handlingParg = false;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (forgeVersionMeta.Arguments.Jvm != null)
                foreach (var arg in forgeVersionMeta.Arguments.GetJvmArgs())
                {
                    if (arg == "-p")
                    {
                        handlingParg = true;
                        _jvmArguments.Add(new LaunchArg(arg, 1));
                        continue;
                    }            
                    
                    if (handlingParg)
                    {
                        handlingParg = false;
                        _jvmArguments.Add(new LaunchArg(arg.Replace("${library_directory}", PathDetails.LibrariesDir), 1));
                        continue;
                    }
                    
                    _jvmArguments.Add(new LaunchArg(arg, 1));
                }
        }

        if (!string.IsNullOrEmpty(forgeVersionMeta.MinecraftArguments))
        {
            // Only 1.12.2 has this field, and
            // it contains many duplicate vanilla arguments
            // so this is the easiest way to handle it
            _gameArguments.Add(new LaunchArg("--tweakClass net.minecraftforge.fml.common.launcher.FMLTweaker", 1));
        }
        
        ModdedData moddedData = new ModdedData(forgeVersionMeta.MainClass, forgeVersion, localLibraries);
        return moddedData;
    }
}