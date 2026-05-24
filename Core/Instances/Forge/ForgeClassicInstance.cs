using System.IO.Compression;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Network;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints.Modding;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.Instance;
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
        
        // Download Installer
        string universalFormatName = universalFormat.Replace("${version}", forgeVersionName);
        string installerJarPath = Path.Combine(tempDir, "installer.jar");
        string installerDir = Path.Combine(tempDir, $"installer-{forgeVersionName}");
        string installerProfilePath = Path.Combine(VersionData.CustomVersionDirectory!, "install_profile.json");
        string forgeUniversalDir = Path.Combine(PathDetails.LibrariesDir, "net", "minecraftforge", "forge",
            forgeVersionName);
        string forgeUniversalPath = Path.Combine(forgeUniversalDir,
            universalFormatName);
        
        if (!File.Exists(VersionData.CustomJarPath))
        {
            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (_, e) =>
            {
                _progressReporter?.UpdateStatusTranslated("instance.downloading.installer", "forge",
                    e.ToString("0.00"));
            };
       
            await HttpHelper.DownloadFileAsync(
                string.Format(ForgeEndpoints.InstallerJarUrl, forgeVersionName), installerJarPath,
                progress, cancellationToken);
       
            // Extract Installer
            _progressReporter?.UpdateStatusTranslated("instance.extracting.installer", "forge");
            await ZipFile.ExtractToDirectoryAsync(installerJarPath, installerDir, cancellationToken);
            
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
                    await ZipFile.ExtractToDirectoryAsync(universalJarPath, universalDir, cancellationToken);

                // COPY UNIVERSAL
                if (!Directory.Exists(forgeUniversalDir))
                    Directory.CreateDirectory(forgeUniversalDir);
                
                if (!File.Exists(forgeUniversalPath))
                    File.Copy(universalJarPath, forgeUniversalPath, true);
            
                // VERSION
                var versionJsonPath = Path.Combine(universalDir, "version.json");
                if (!File.Exists(VersionData.CustomJsonPath) && File.Exists(versionJsonPath))
                    File.Move(versionJsonPath, VersionData.CustomJsonPath!, true);
            }
            else
                _logger.Warn("Forge universal jar not found in the installer directory. This may indicate an issue with the installer.");
            
            // Maven directory does not exist in this version
        }
        
        // Include Forge Universal Jar Classpath
        ArgumentBuilder.AddClass(forgeUniversalPath);
        
        // Read Forge Install Profile
        var rawInstallProfile = await File.ReadAllTextAsync(installerProfilePath, cancellationToken);
        var installProfile = JsonConvert.DeserializeObject<ForgeProfile>(rawInstallProfile);
        if (installProfile == null)
            throw new FileNotFoundException("Failed to get the forge install profile meta.");

        // Fix 1.6.1 Forge Version
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (GameDetails.MinecraftVersion == "1.6.1" && !File.Exists(VersionData.CustomJsonPath) && installProfile.VersionInfo != null)
            await File.WriteAllTextAsync(VersionData.CustomJsonPath!, JsonConvert.SerializeObject(installProfile.VersionInfo), cancellationToken);

        // Read Forge Version Meta
        string? mainClass = null;
        if (File.Exists(VersionData.CustomJsonPath))
        {
            var rawForgeVersionMeta = await File.ReadAllTextAsync(VersionData.CustomJsonPath, cancellationToken);
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
            
            _progressReporter?.UpdateStatusTranslated("instance.reading.libraries");
            // Add launch arguments
            _progressReporter?.UpdateStatusTranslated("instance.building.arguments");
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
            if (!File.Exists(VersionData.CustomJarPath))
            {
                //ReportProgress(0, $"ui_copying_jar", "vanilla");
                File.Copy(VersionData.VanillaJarPath, VersionData.CustomJarPath!);
            }
        }
        else
        {
            if (!File.Exists(VersionData.CustomJarPath))
            {
                string vanillaExtractDir = Path.Combine(tempDir, "vanilla_extract");
                if (!Directory.Exists(vanillaExtractDir))
                    Directory.CreateDirectory(vanillaExtractDir);

                // Extract vanilla jar
                await ZipFile.ExtractToDirectoryAsync(VersionData.VanillaJarPath, vanillaExtractDir, cancellationToken);
                var vanillaMetaDir = Path.Combine(vanillaExtractDir, "META-INF");
                if (Directory.Exists(vanillaMetaDir))
                    FileSystemHelper.DeleteDirectory(vanillaMetaDir);
                else
                    _logger.Warn(
                        "META-INF directory not found in the vanilla jar. This may indicate an issue with the jar file.");

                // Extract universal jar
                await ZipFile.ExtractToDirectoryAsync(forgeUniversalPath, vanillaExtractDir, true, cancellationToken);
                string patchedVanillaJarPath = Path.Combine(tempDir, "patched_vanilla.jar");
                await ZipFile.CreateFromDirectoryAsync(vanillaExtractDir, patchedVanillaJarPath, cancellationToken);

                File.Copy(patchedVanillaJarPath, VersionData.CustomJarPath);
            }
        }

        var legacyLibraries = ForgeInstance.GetLegacyLibraries(GameDetails.MinecraftVersion);
        ModdedData moddedData = new ModdedData(mainClass, legacyLibraries.Count > 0 ? [] : localLibraries);
        if (legacyLibraries.Count == 0)
            return moddedData;
        
        string forgeLibDir = Path.Combine(VersionData.GameDir, "lib");
        if (!Directory.Exists(forgeLibDir))
            Directory.CreateDirectory(forgeLibDir);
        
        // Copy legacy libraries to the forge lib directory
        foreach (var library in legacyLibraries)
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