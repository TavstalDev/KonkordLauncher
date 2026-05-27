using System.IO.Compression;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints.Modding;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.Instance;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge.Legacy;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta.Library;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Core.Instances.Forge;

// 1.7.10-1.12.1
public class ForgeLegacyInstance(string forgeVersionName,
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
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(ForgeLegacyInstance));

    public override async Task<ModdedData?> InstallModdedAsync(string tempDir, IHttpService httpService, CancellationToken cancellationToken = default)
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
        string installerJarPath = Path.Combine(tempDir, "installer.jar");
        string installerDir = Path.Combine(tempDir, $"installer-{forgeVersionName}");
        string installerProfilePath = Path.Combine(VersionData.CustomVersionDirectory!, "install_profile.json");
        string forgeUniversalDir = Path.Combine(PathDetails.LibrariesDir, "net", "minecraftforge", "forge",
            forgeVersionName);
        string forgeUniversalPath = Path.Combine(forgeUniversalDir,
            $"forge-{forgeVersionName}-universal.jar");
        if (!File.Exists(VersionData.CustomJsonPath))
        {
            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (_, e) =>
            {
                _progressReporter?.UpdateStatusTranslated("instance.downloading.installer", "forge",
                    e.ToString("0.00"));
            };
       
            await httpService.DownloadFileAsync(
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
            string universalJarPath = Path.Combine(installerDir, $"forge-{forgeVersionName}-universal.jar");
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
                if (!File.Exists(VersionData.CustomJsonPath))
                    File.Move(Path.Combine(universalDir, "version.json"), VersionData.CustomJsonPath!, true);
            }
            else
                _logger.Warn("Forge universal jar not found in the installer directory. This may indicate an issue with the installer.");
            
            // Maven directory does not exist in this version
        }
        
        // Add Forge Universal Jar to classpath
        ArgumentBuilder.AddClass(forgeUniversalPath);

        // Read Forge Version Meta
        var rawForgeVersionMeta = await File.ReadAllTextAsync(VersionData.CustomJsonPath!, cancellationToken);
        var forgeVersionMeta = JsonConvert.DeserializeObject<ForgeVersionMeta>(rawForgeVersionMeta);
        if (forgeVersionMeta == null)
            throw new FileNotFoundException("Failed to get the forge version meta.");
        
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
        var rawInstallProfile = await File.ReadAllTextAsync(installerProfilePath, cancellationToken);
        var installProfile = JsonConvert.DeserializeObject<ForgeProfile>(rawInstallProfile);
        if (installProfile == null)
            throw new FileNotFoundException("Failed to get the forge install profile meta.");
        
        _progressReporter?.UpdateStatusTranslated("instance.reading.libraries");
        // Add launch arguments
        _progressReporter?.UpdateStatusTranslated("instance.building.arguments");
        
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (forgeVersionMeta.MinecraftArguments != null)
        {
            MinecraftVersionMeta.ArgumentsLegacy = forgeVersionMeta.MinecraftArguments;
            string[] args =  forgeVersionMeta.MinecraftArguments.Split(' ');
            int tweakIndex = args.IndexOf("--tweakClass");
            ArgumentBuilder.AddGameArgument(new LaunchArg($"--tweakClass {args[tweakIndex + 1]}", 1));
        }

        // Copy vanilla jar
        if (!File.Exists(VersionData.CustomJarPath))
        {
            //ReportProgress(0, $"ui_copying_jar", "vanilla");
            File.Copy(VersionData.VanillaJarPath, VersionData.CustomJarPath!, true);
        }
        return new ModdedData(forgeVersionMeta.MainClass, localLibraries);
    }
}