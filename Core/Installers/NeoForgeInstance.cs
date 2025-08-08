using System.IO.Compression;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge.New;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;

namespace Tavstal.KonkordLauncher.Core.Installers;

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
    
    protected override async Task<ModdedData?> InstallModdedAsync(string tempDir)
    {
        if (!File.Exists(PathDetails.CustomManifestPath))
        {
            _logger.Error("NeoForge manifest file does not exist. Please ensure the manifest is downloaded.");
            return null;
        }

        VersionDetails forgeVersion = GameHelper.GetVersionDetails(PathDetails.VersionsDir, this.MinecraftVersion.Id, EMinecraftKind.NEOFORGE, this.GameDetails.CustomVersion, this.GameDetails.CustomGameDirectory);
        
        // Create versionDir in the versions folder
        if (!Directory.Exists(forgeVersion.VersionDirectory))
            Directory.CreateDirectory(forgeVersion.VersionDirectory);

        // Check libsizes dir
        string librarySizeCacheDir = Path.Combine(PathDetails.CacheDir, "libsizes");
        if (!Directory.Exists(librarySizeCacheDir))
            Directory.CreateDirectory(librarySizeCacheDir);

        // Download Installer
        string installerJarPath = Path.Combine(tempDir, "installer.jar");
        string installerDir = Path.Combine(tempDir, "installer");

        if (!File.Exists(installerJarPath))
        {
            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (sender, e) =>
            {
                _progressReporter?.SetStatusTranslated("instance.downloading.installer", "neoforge",
                    e.ToString("0.00"));
            };
            
            await HttpHelper.DownloadFileAsync(
                string.Format(NeoForgeEndpoints.InstallerJarUrl, forgeVersion.CustomVersion), installerJarPath,
                progress);
        }

        // Extract Installer
        _progressReporter?.SetStatusTranslated("instance.extracting.installer", "neoforge");
        ZipFile.ExtractToDirectory(installerJarPath, installerDir);

        // Move version.json and profile.json 
        string installProfileJson = Path.Combine(forgeVersion.VersionDirectory, "install_profile.json");
        // INSTALL PROFILE
        if (!File.Exists(installProfileJson))
            File.Move(Path.Combine(installerDir, "install_profile.json"), installProfileJson);
        // VERSION
        if (!File.Exists(forgeVersion.VersionJsonPath))
            File.Move(Path.Combine(installerDir, "version.json"), forgeVersion.VersionJsonPath);

        // COPY MAVEN IF EXISTS
        string mavenTempDir = Path.Combine(installerDir, "maven");
        if (Directory.Exists(mavenTempDir))
        {
            string[] files = Directory.GetFiles(mavenTempDir, "*.jar", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string newPath = file.Replace(mavenTempDir, PathDetails.LibrariesDir);
                string newDir = Path.GetDirectoryName(newPath) ?? throw new NullReferenceException("fix me - newDir is null");

                if (!Directory.Exists(newDir))
                    Directory.CreateDirectory(newDir);

                if (!File.Exists(newPath))
                    File.Copy(file, newPath, false);
            }
        }

        ForgeVersionMeta? forgeVersionMeta = JsonConvert.DeserializeObject<ForgeVersionMeta>(await File.ReadAllTextAsync(forgeVersion.VersionJsonPath));
        if (forgeVersionMeta == null)
            throw new FileNotFoundException("Failed to get the forge version meta.");

        ForgeVersionProfile? installProfile = JsonConvert.DeserializeObject<ForgeVersionProfile>(await File.ReadAllTextAsync(installProfileJson));
        if (installProfile == null)
            throw new FileNotFoundException("Failed to get the forge install profile meta.");

        _progressReporter?.SetStatusTranslated("instance.reading.libraries");
        List<LibraryMeta> localLibraries = [];
        localLibraries.AddRange(forgeVersionMeta.Libraries);

        // Download installer libraries
        /*string librarySizeCachePath = Path.Combine(librarySizeCacheDir, $"{forgeVersion.MinecraftVersion}-forge-installer-{forgeVersion.CustomVersion}.json");

        int downloadedSize = 0;
        int toDownloadSize = 0;
        if (!File.Exists(librarySizeCachePath))
        {
            foreach (LibraryMeta lib in installProfile.Libraries)
            {
                if (lib.Downloads.Artifact == null)
                    continue;
                
                toDownloadSize += lib.Downloads.Artifact.Size;
            }
            await File.WriteAllTextAsync(librarySizeCachePath, toDownloadSize.ToString());
        }*/

        foreach (LibraryMeta lib in installProfile.Libraries)
        {
            if (lib.Downloads.Artifact == null)
                continue;
            
            string localPath = lib.Downloads.Artifact.Path;
            string libDirPath = Path.Combine(PathDetails.LibrariesDir, localPath.Remove(localPath.LastIndexOf('/'), localPath.Length - localPath.LastIndexOf('/')));
            
            if (!Directory.Exists(libDirPath))
                Directory.CreateDirectory(libDirPath);
            
            string libFilePath = Path.Combine(PathDetails.LibrariesDir, localPath);
            if (!File.Exists(libFilePath))
            {
                if (!string.IsNullOrEmpty(lib.Downloads.Artifact.Url))
                {
                    Progress<double> libProgress = new Progress<double>();
                    libProgress.ProgressChanged += (_, e) =>
                    {
                        _progressReporter?.SetStatusTranslated("instance.downloading.libraries", lib.Name, e.ToString("0.00"));
                    };

                    await HttpHelper.DownloadFileAsync(lib.Downloads.Artifact.Url, libFilePath, libProgress);
                    //downloadedSize += lib.Downloads.Artifact.Size;
                }
            }
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
                        _jvmArguments.Add(new LaunchArg('"' + arg.Replace("${library_directory}", PathDetails.LibrariesDir) + '"', 1));
                        continue;
                    }
                    
                    _jvmArguments.Add(new LaunchArg(arg, 1));
                }
        }

        _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-DMcEmu=net.minecraft.client.main.Main", 2));
        _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Dlog4j2.formatMsgNoLookups=true", 2));
        _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Djava.rmi.server.useCodebaseOnly=true", 2));
        _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Dcom.sun.jndi.rmi.object.trustURLCodebase=false", 2));

        _progressReporter?.SetStatusTranslated("instance.building", "neoforge", 0);
        // Generate client libs
        await MapAndStartProcessors(installProfile, installerDir);

        #region GET minecraftforge client libs
        string forgeUniversalUrl = string.Format(NeoForgeEndpoints.LoaderUniversalJarUrl, forgeVersion.CustomVersion);

        string forgeUniversalDir = Path.Combine(PathDetails.LibrariesDir, "net", "neoforged", "neoforge", forgeVersion.CustomVersion);
        string forgeUniversalPath = Path.Combine(forgeUniversalDir, $"neoforge-{forgeVersion.CustomVersion}-universal.jar");
        if (!Directory.Exists(forgeUniversalDir))
            Directory.CreateDirectory(forgeUniversalDir);

        _progressReporter?.SetStatusTranslated("instance.reading.universal", "neoforge");
        if (!File.Exists(forgeUniversalPath))
        {
            Progress<double> univProgress = new Progress<double>();
            univProgress.ProgressChanged += (_, e) =>
            {
                _progressReporter?.SetStatusTranslated("instance.downloading.universal", "neoforge", e.ToString("0.00"));
            };

            await HttpHelper.DownloadFileAsync(forgeUniversalUrl, forgeUniversalPath, univProgress);
        }
        #endregion

        ModdedData moddedData = new ModdedData(forgeVersionMeta.MainClass, forgeVersion, localLibraries);
        return moddedData;
    }
}