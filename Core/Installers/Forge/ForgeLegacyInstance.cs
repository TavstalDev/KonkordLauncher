using System.IO.Compression;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints.Modding;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge.Legacy;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta.Library;

namespace Tavstal.KonkordLauncher.Core.Installers.Forge;

// 1.7.10-1.12.1
public class ForgeLegacyInstance(string forgeVersionName,
    GameDetails gameDetails,
    PathDetails pathDetails,
    LauncherDetails launcherDetails,
    ClientDetails clientDetails,
    Resolution? resolution = null,
    IProgressReporter? progressReporter = null)
    : ForgeInstanceBase(gameDetails, pathDetails, launcherDetails, clientDetails, resolution, progressReporter)
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(ForgeLegacyInstance));

    protected override async Task<ModdedData?> InstallModdedAsync(string tempDir)
    {
        if (!File.Exists(PathDetails.CustomManifestPath))
        {
            _logger.Error("Forge manifest file does not exist. Please ensure the manifest is downloaded.");
            return null;
        }

        List<LibraryMeta> localLibraries = [];
        VersionDetails forgeVersion = GameHelper.GetVersionDetails(PathDetails.VersionsDir, this.MinecraftVersion.Id, EMinecraftKind.FORGE, this.GameDetails.CustomVersion, this.GameDetails.CustomGameDirectory);
        
        // Create versionDir in the versions folder
        if (!Directory.Exists(forgeVersion.VersionDirectory))
            Directory.CreateDirectory(forgeVersion.VersionDirectory);
        
        // Download Installer
        string installerJarPath = Path.Combine(tempDir, "installer.jar");
        string installerDir = Path.Combine(tempDir, $"installer-{forgeVersionName}");
        string installerProfilePath = Path.Combine(forgeVersion.VersionDirectory, "install_profile.json");
        string forgeUniversalDir = Path.Combine(PathDetails.LibrariesDir, "net", "minecraftforge", "forge",
            forgeVersionName);
        string forgeUniversalPath = Path.Combine(forgeUniversalDir,
            $"forge-{forgeVersionName}-universal.jar");
        if (!File.Exists(forgeVersion.VersionJsonPath))
        {
            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (sender, e) =>
            {
                _progressReporter?.SetStatusTranslated("instance.downloading.installer", "forge",
                    e.ToString("0.00"));
            };
       
            await HttpHelper.DownloadFileAsync(
                string.Format(ForgeEndpoints.InstallerJarUrl, forgeVersionName), installerJarPath,
                progress);
       
            // Extract Installer
            _progressReporter?.SetStatusTranslated("instance.extracting.installer", "forge");
            ZipFile.ExtractToDirectory(installerJarPath, installerDir);
            
            // Move install_profile.json
            var source = Path.Combine(installerDir, "install_profile.json");
            if (File.Exists(source))
                File.Move(source, installerProfilePath, true);
            else
                _logger.Error("Install profile JSON file not found in the forge installer directory.");
            
            // Extract universal jar
            string universalJarPath = Path.Combine(installerDir, $"forge-{forgeVersionName}-universal.jar");
            _logger.Debug("Checking for universal jar at: " + universalJarPath);
            if (File.Exists(universalJarPath))
            {
                string universalDir = Path.Combine(tempDir,
                    $"{forgeVersionName}-universal");
                if (!Directory.Exists(universalDir))
                    ZipFile.ExtractToDirectory(universalJarPath, universalDir);

                // COPY UNIVERSAL
                if (!Directory.Exists(forgeUniversalDir))
                    Directory.CreateDirectory(forgeUniversalDir);
                
                if (!File.Exists(forgeUniversalPath))
                    File.Copy(universalJarPath, forgeUniversalPath, true);
                _classPath += $"{forgeUniversalPath}${{classpath_separator}}";
            
                // VERSION
                if (!File.Exists(forgeVersion.VersionJsonPath))
                    File.Move(Path.Combine(universalDir, "version.json"), forgeVersion.VersionJsonPath, true);
            }
            else
                _logger.Warn("Forge universal jar not found in the installer directory. This may indicate an issue with the installer.");
            
            // Maven directory does not exist in this version
        }
        
        // Add Forge Universal Jar to classpath
        _classPath += $"{forgeUniversalPath}${{classpath_separator}}";

        // Read Forge Version Meta
        var rawForgeVersionMeta = await File.ReadAllTextAsync(forgeVersion.VersionJsonPath);
        var forgeVersionMeta = JsonConvert.DeserializeObject<ForgeVersionMeta>(rawForgeVersionMeta);
        if (forgeVersionMeta == null)
            throw new FileNotFoundException("Failed to get the forge version meta.");
        rawForgeVersionMeta = null; // Clear the raw meta to free memory
        
        // Install libraries from Forge Version Meta
        foreach (var lib in forgeVersionMeta.Libraries)
        {
            string? url = lib.GetUrl(true);
            if (url == null)
                continue;

            localLibraries.Add(new LibraryMeta
            {
                Name = lib.Name,
                Downloads = new LibraryDownloads
                {
                    Artifact = new Artifact
                    {
                        Path = lib.GetPath(),
                        Sha1 = string.Empty,
                        Size = 0,
                        Url = url,
                    },
                    Classifiers = null
                },
                Natives = null,
                Rules = []
            });
        }

        // Read Forge Install Profile
        var rawInstallProfile = await File.ReadAllTextAsync(installerProfilePath);
        var installProfile = JsonConvert.DeserializeObject<ForgeProfile>(rawInstallProfile);
        if (installProfile == null)
            throw new FileNotFoundException("Failed to get the forge install profile meta.");
        rawInstallProfile = null; // Clear the raw data to free memory
        
        _progressReporter?.SetStatusTranslated("instance.reading.libraries");
        // Add launch arguments
        _progressReporter?.SetStatusTranslated("instance.building.arguments");
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (forgeVersionMeta.MinecraftArguments != null)
        {
            MinecraftVersionMeta.ArgumentsLegacy = forgeVersionMeta.MinecraftArguments;
        }
        
        // Copy vanilla jar
        if (!File.Exists(forgeVersion.VersionJarPath))
        {
            //ReportProgress(0, $"ui_copying_jar", "vanilla");
            File.Copy(forgeVersion.VanillaJarPath, forgeVersion.VersionJarPath);
        }

        ModdedData moddedData = new ModdedData(forgeVersionMeta.MainClass, forgeVersion, localLibraries);
        return moddedData;
    }
}