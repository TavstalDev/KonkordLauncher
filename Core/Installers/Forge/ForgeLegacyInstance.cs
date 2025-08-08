using System.IO.Compression;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge.Legacy;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta.Library;

namespace Tavstal.KonkordLauncher.Core.Installers.Forge;

// 1.7.10-1.12.x
public class ForgeLegacyInstance(
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

        VersionDetails forgeVersion = GameHelper.GetVersionDetails(PathDetails.VersionsDir, this.MinecraftVersion.Id, EMinecraftKind.FORGE, this.GameDetails.CustomVersion, this.GameDetails.CustomGameDirectory);
        
        // Create versionDir in the versions folder
        if (!Directory.Exists(forgeVersion.VersionDirectory))
            Directory.CreateDirectory(forgeVersion.VersionDirectory);
        
        // Download Installer
        string installerJarPath = Path.Combine(tempDir, "installer.jar");
        string installerDir = Path.Combine(tempDir, "installer");

        string extraVersion = string.Empty;
        if (!File.Exists(installerJarPath))
        {
            _progressReporter?.SetStatusTranslated("instance.downloading.installer", "forge");
            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (sender, e) =>
            {
                _progressReporter?.SetStatusTranslated("instance.downloading.installer", "forge",
                    e.ToString("0.00"));
            };
            
            try
            {
                await HttpHelper.DownloadFileAsync(string.Format(ForgeEndpoints.InstallerJarUrl,
                    $"{forgeVersion.MinecraftVersion}-{forgeVersion.CustomVersion}"), installerJarPath, progress);
            }
            catch
            {
                int length = forgeVersion.MinecraftVersion.Split('.').Length;
                if (length == 3)
                {
                    await HttpHelper.DownloadFileAsync(string.Format(ForgeEndpoints.InstallerJarUrl,
                        $"{forgeVersion.MinecraftVersion}-{forgeVersion.CustomVersion}-{forgeVersion.MinecraftVersion}"), installerJarPath, progress);
                    extraVersion = $"-{forgeVersion.MinecraftVersion}";
                }
                else
                {
                    await HttpHelper.DownloadFileAsync(string.Format(ForgeEndpoints.InstallerJarUrl,
                        $"{forgeVersion.MinecraftVersion}-{forgeVersion.CustomVersion}-{forgeVersion.MinecraftVersion}.0"), installerJarPath, progress);
                    extraVersion = $"-{forgeVersion.MinecraftVersion}.0";
                }
            }
        }

        // Extract Installer
        _progressReporter?.SetStatusTranslated("instance.extracting.installer", "forge");
        ZipFile.ExtractToDirectory(installerJarPath, installerDir);

        // Move version.json and profile.json 
        string installProfileJson = Path.Combine(forgeVersion.VersionDirectory, "install_profile.json");
        // INSTALL PROFILE
        if (!File.Exists(installProfileJson))
            File.Move(Path.Combine(installerDir, "install_profile.json"), installProfileJson);

        // EXTRACT UNIVERSAL
        string universalJarPath = Path.Combine(installerDir, $"forge-{forgeVersion.MinecraftVersion}-{forgeVersion.CustomVersion}{extraVersion}-universal.jar");
        string universalDir = Path.Combine(installerDir, $"forge-{forgeVersion.MinecraftVersion}-{forgeVersion.CustomVersion}{extraVersion}-universal");
        if (!Directory.Exists(universalDir) && File.Exists(universalJarPath))
        {
            ZipFile.ExtractToDirectory(universalJarPath, universalDir);
        }

        // COPY UNIVERSAL
        string forgeUniversalDir = Path.Combine(PathDetails.LibrariesDir, "net", "minecraftforge", "forge", $"{forgeVersion.MinecraftVersion}-{forgeVersion.CustomVersion}");
        string forgeUniversalPath = Path.Combine(forgeUniversalDir, $"forge-{forgeVersion.MinecraftVersion}-{forgeVersion.CustomVersion}{extraVersion}-universal.jar");
        if (!Directory.Exists(forgeUniversalDir))
            Directory.CreateDirectory(forgeUniversalDir);

        if (!File.Exists(forgeUniversalPath))
            File.Copy(universalJarPath, forgeUniversalPath);
        _classPath += $"{forgeUniversalPath};";

        // VERSION
        if (!File.Exists(forgeVersion.VersionJsonPath))
            File.Move(Path.Combine(universalDir, "version.json"), forgeVersion.VersionJsonPath);

        ForgeProfile? installProfile = JsonConvert.DeserializeObject<ForgeProfile>(await File.ReadAllTextAsync(installProfileJson));
        if (installProfile == null)
            throw new FileNotFoundException("Failed to get the forge install profile meta.");

        ForgeVersionMeta? forgeVersionMeta = installProfile.VersionInfo;
        if (forgeVersionMeta == null)
            throw new FileNotFoundException("Failed to get the forge version meta.");

        _progressReporter?.SetStatusTranslated("instance.reading.libraries");
        List<LibraryMeta> localLibraries = new List<LibraryMeta>();
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

        // Add launch arguments
        _progressReporter?.SetStatusTranslated("instance.building.arguments");
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (forgeVersionMeta.MinecraftArguments != null)
        {
            MinecraftVersionMeta.ArgumentsLegacy = forgeVersionMeta.MinecraftArguments;
        }

        _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-DMcEmu=net.minecraft.client.main.Main", 2));
        _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Dlog4j2.formatMsgNoLookups=true", 2));
        _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Djava.rmi.server.useCodebaseOnly=true", 2));
        _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Dcom.sun.jndi.rmi.object.trustURLCodebase=false", 2));
        _jvmArgumentsBeforeClassPath.Add(new LaunchArg($"-Dminecraft.client.jar={forgeVersion.VersionJarPath}", 2));

        // Copy vanilla jar
        if (!File.Exists(forgeVersion.VersionJarPath))
        {
            //ReportProgress(0, $"ui_copying_jar", "vanilla");
            File.Copy(forgeVersion.VanillaJarPath, forgeVersion.VersionJarPath);
        }
        //_classPath += $"{forgeVersion.VersionJarPath};"; - not needed


        ModdedData moddedData = new ModdedData(forgeVersionMeta.MainClass, forgeVersion, localLibraries);
        return moddedData;
    }
}