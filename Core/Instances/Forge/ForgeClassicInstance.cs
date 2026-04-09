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

namespace Tavstal.KonkordLauncher.Core.Instances.Forge;

// 1.5.2 - 1.7.2
public class ForgeClassicInstance(string forgeVersionName,
    string universalFormat,
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

        List<LibraryMeta> localLibraries = [];
        VersionDetails forgeVersion = GameHelper.GetVersionDetails(PathDetails.VersionsDir, MinecraftVersion.Id, EMinecraftKind.FORGE, GameDetails.CustomVersion, GameDetails.CustomGameDirectory);
        
        // Create versionDir in the versions folder
        if (!Directory.Exists(forgeVersion.VersionDirectory))
            Directory.CreateDirectory(forgeVersion.VersionDirectory);
        
        // Download Installer
        string universalFormatName = universalFormat.Replace("${version}", forgeVersionName);
        string installerJarPath = Path.Combine(tempDir, "installer.jar");
        string installerDir = Path.Combine(tempDir, $"installer-{forgeVersionName}");
        string installerProfilePath = Path.Combine(forgeVersion.VersionDirectory, "install_profile.json");
        string forgeUniversalDir = Path.Combine(PathDetails.LibrariesDir, "net", "minecraftforge", "forge",
            forgeVersionName);
        string forgeUniversalPath = Path.Combine(forgeUniversalDir,
            universalFormatName);
        
        if (!File.Exists(forgeVersion.VersionJarPath))
        {
            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (_, e) =>
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
            string universalJarPath = Path.Combine(installerDir, universalFormatName);
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
            
                // VERSION
                var versionJsonPath = Path.Combine(universalDir, "version.json");
                if (!File.Exists(forgeVersion.VersionJsonPath) && File.Exists(versionJsonPath))
                    File.Move(versionJsonPath, forgeVersion.VersionJsonPath, true);
            }
            else
                _logger.Warn("Forge universal jar not found in the installer directory. This may indicate an issue with the installer.");
            
            // Maven directory does not exist in this version
        }
        
        // Include Forge Universal Jar Classpath
        _classPath.Add(forgeUniversalPath);
        
        // Read Forge Install Profile
        var rawInstallProfile = await File.ReadAllTextAsync(installerProfilePath);
        var installProfile = JsonConvert.DeserializeObject<ForgeProfile>(rawInstallProfile);
        if (installProfile == null)
            throw new FileNotFoundException("Failed to get the forge install profile meta.");

        // Fix 1.6.1 Forge Version
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (GameDetails.MinecraftVersion == "1.6.1" && !File.Exists(forgeVersion.VersionJsonPath) && installProfile.VersionInfo != null)
            await File.WriteAllTextAsync(forgeVersion.VersionJsonPath, JsonConvert.SerializeObject(installProfile.VersionInfo));

        // Read Forge Version Meta
        string? mainClass = null;
        if (File.Exists(forgeVersion.VersionJsonPath))
        {
            var rawForgeVersionMeta = await File.ReadAllTextAsync(forgeVersion.VersionJsonPath);
            var forgeVersionMeta = JsonConvert.DeserializeObject<ForgeVersionMeta>(rawForgeVersionMeta);
            if (forgeVersionMeta == null)
                throw new FileNotFoundException("Failed to get the forge version meta.");
            mainClass = forgeVersionMeta.MainClass;

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
            
            _progressReporter?.SetStatusTranslated("instance.reading.libraries");
            // Add launch arguments
            _progressReporter?.SetStatusTranslated("instance.building.arguments");
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (forgeVersionMeta.MinecraftArguments != null)
            {
                MinecraftVersionMeta.ArgumentsLegacy = forgeVersionMeta.MinecraftArguments;
            }
        }

        // Patch vanilla jar
        // Fixes java 8 compatibility issues with Forge 1.5.2 - 1.7.2
        if (mainClass != null)
        {
            if (!File.Exists(forgeVersion.VersionJarPath))
            {
                //ReportProgress(0, $"ui_copying_jar", "vanilla");
                File.Copy(forgeVersion.VanillaJarPath, forgeVersion.VersionJarPath);
            }
        }
        else
        {
            if (!File.Exists(forgeVersion.VersionJarPath))
            {
                string vanillaExtractDir = Path.Combine(tempDir, "vanilla_extract");
                if (!Directory.Exists(vanillaExtractDir))
                    Directory.CreateDirectory(vanillaExtractDir);

                // Extract vanilla jar
                ZipFile.ExtractToDirectory(forgeVersion.VanillaJarPath, vanillaExtractDir);
                var vanillaMetaDir = Path.Combine(vanillaExtractDir, "META-INF");
                if (Directory.Exists(vanillaMetaDir))
                    FileSystemHelper.DeleteDirectory(vanillaMetaDir);
                else
                    _logger.Warn(
                        "META-INF directory not found in the vanilla jar. This may indicate an issue with the jar file.");

                // Extract universal jar
                ZipFile.ExtractToDirectory(forgeUniversalPath, vanillaExtractDir, true);
                string patchedVanillaJarPath = Path.Combine(tempDir, "patched_vanilla.jar");
                ZipFile.CreateFromDirectory(vanillaExtractDir, patchedVanillaJarPath);

                File.Copy(patchedVanillaJarPath, forgeVersion.VersionJarPath);
            }
        }

        var legacyLibraries = ForgeInstance.GetLegacyLibraries(GameDetails.MinecraftVersion);
        ModdedData moddedData = new ModdedData(mainClass, forgeVersion, legacyLibraries.Count > 0 ? [] : localLibraries);
        if (legacyLibraries.Count == 0)
            return moddedData;
        
        string forgeLibDir = Path.Combine(forgeVersion.GameDir, "lib");
        if (!Directory.Exists(forgeLibDir))
            Directory.CreateDirectory(forgeLibDir);
        
        // Copy legacy libraries to the forge lib directory
        foreach (var library in legacyLibraries)
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