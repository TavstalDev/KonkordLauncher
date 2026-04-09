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

namespace Tavstal.KonkordLauncher.Core.Instances;

public class NeoForgeInstance(
    GameDetails gameDetails,
    PathDetails pathDetails,
    LauncherDetails launcherDetails,
    ClientDetails clientDetails,
    Resolution? resolution = null,
    IProgressReporter? progressReporter = null)
    : ForgeInstanceBase(gameDetails, pathDetails, launcherDetails, clientDetails, resolution, progressReporter)
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(NeoForgeInstance));
    
    protected override async Task<ModdedData?> InstallModdedAsync(string tempDir, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(PathDetails.CustomManifestPath))
        {
            _logger.Error("NeoForge manifest file does not exist. Please ensure the manifest is downloaded.");
            return null;
        }

        List<LibraryMeta> localLibraries = [];
        VersionDetails forgeVersion = GameHelper.GetVersionDetails(PathDetails.VersionsDir, MinecraftVersion.Id, EMinecraftKind.NEOFORGE, GameDetails.CustomVersion, GameDetails.CustomGameDirectory);
        
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
                _progressReporter?.SetStatusTranslated("instance.downloading.installer", "neoforge",
                    e.ToString("0.00"));
            };
       
            await HttpHelper.DownloadFileAsync(
                string.Format(NeoForgeEndpoints.InstallerJarUrl, forgeVersion.CustomVersion), installerJarPath,
                progress);
       
            // Extract Installer
            _progressReporter?.SetStatusTranslated("instance.extracting.installer", "neoforge");
            ZipFile.ExtractToDirectory(installerJarPath, installerDir);
            
            // Move install_profile.json
            var source = Path.Combine(installerDir, "install_profile.json");
            if (File.Exists(source))
                File.Move(source, installerProfilePath);
            else
                _logger.Error("Install profile JSON file not found in the neoforge installer directory.");
            
            // Move version.json
            source = Path.Combine(installerDir, "version.json");
            if (File.Exists(source))
                File.Move(source, forgeVersion.VersionJsonPath);
            else
                _logger.Error("Install version JSON file not found in the neoforge installer directory.");
            
            // Extract Maven
            source = Path.Combine(installerDir, "maven");
            if (Directory.Exists(source))
            {
                string[] content = Directory.GetDirectories(source);
                foreach (string dir in content)
                {
                    string newDirPath = dir.Replace(source, PathDetails.LibrariesDir);
                    if (!Directory.Exists(newDirPath))
                        Directory.CreateDirectory(newDirPath);
                }

                content = Directory.GetFiles(source);
                foreach (string file in content)
                {
                    string newFilePath = file.Replace(source, PathDetails.LibrariesDir);
                    if (!File.Exists(newFilePath))
                        File.Copy(file, newFilePath, true);
                }
            }
            else
                _logger.Warn("Maven directory not found in the neoforge installer directory.");
        }
        
        // Read Forge Version Meta
        var rawForgeVersionMeta = await File.ReadAllTextAsync(forgeVersion.VersionJsonPath);
        var forgeVersionMeta = JsonConvert.DeserializeObject<ForgeVersionMeta>(rawForgeVersionMeta);
        if (forgeVersionMeta == null)
            throw new FileNotFoundException("Failed to get the neoforge version meta.");
        
        // Install libraries from Forge Version Meta
        localLibraries.AddRange(forgeVersionMeta.Libraries);

        // Read Forge Install Profile
        var rawInstallProfile = await File.ReadAllTextAsync(installerProfilePath);
        var installProfile = JsonConvert.DeserializeObject<ForgeVersionProfile>(rawInstallProfile);
        if (installProfile == null)
            throw new FileNotFoundException("Failed to get the neoforge install profile meta.");
        
        // Install Libraries From Install Profile
        _progressReporter?.SetStatusTranslated("instance.reading.libraries");
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
                _progressReporter?.SetStatusTranslated("instance.downloading.libraries", libMeta.Name,
                    e.ToString("0.00"));
            };

            await HttpHelper.DownloadFileAsync(libMeta.Downloads.Artifact.Url, libraryPath, libProgress);
        }

        // Map and start processors
        _progressReporter?.SetStatusTranslated("instance.building", "neoforge", 0);
        if (File.Exists(installerJarPath))
            await MapAndStartProcessors(installProfile, installerDir);
        
        // Copy Version Files
        string jarSourcePath = Path.Combine(installerDir, "maven", "net", "neoforged", "neoforge", $"{forgeVersion.MinecraftVersion}-{forgeVersion.CustomVersion}", 
            $"neoforge-{forgeVersion.CustomVersion}-universal.jar");
        _logger.Debug("Source jar path: " + jarSourcePath);
        if (File.Exists(jarSourcePath))
        {
            string targetJarPath = Path.Combine(forgeVersion.VersionDirectory, $"neoforge-{forgeVersion.MinecraftVersion}-{forgeVersion.CustomVersion}.jar");
            File.Copy(jarSourcePath, targetJarPath);
        }

        // Add launch arguments
        _progressReporter?.SetStatusTranslated("instance.building.arguments");
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
        
        ModdedData moddedData = new ModdedData(forgeVersionMeta.MainClass, forgeVersion, localLibraries);
        return moddedData;
    }
}