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

namespace Tavstal.KonkordLauncher.Core.Installers.Forge
{
    /*
     * 1.13+ are new
     * 1.12.x - ok
     * 1.11.x - ok
     * 1.9.x - ok
     * 1.8.x - ok
     * 1.7.x - ok
     * 1.6 and below are old
    */
    public class ForgeInstLegacy : ForgeInstallerBase
    {
        private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(ForgeInstLegacy));
        private string _extraVersion {  get; set; }

        public ForgeInstLegacy(string javaPath, string minecraftVersion, int memory, LauncherDetails launcherDetails, ClientDetails clientDetails, 
            EMinecraftKind kind = EMinecraftKind.VANILLA, string? gameDirectory = null, Resolution? resolution = null, 
            string? jvmArgs = "-XX:+UnlockExperimentalVMOptions -XX:+UseG1GC -XX:G1NewSizePercent=20 -XX:G1ReservePercent=20 -XX:MaxGCPauseMillis=50 -XX:G1HeapRegionSize=16M -Djava.net.preferIPv4Stack=true", 
            string? customVersion = null, IProgressReporter? progressReporter = null, bool isDebug = false) 
            : base(javaPath, minecraftVersion, memory, launcherDetails, clientDetails, kind, gameDirectory, resolution, jvmArgs, customVersion, progressReporter, isDebug)
        {
        }

        protected override async Task<ModdedData?> InstallModedAsync(string tempDir)
        {
            ReportProgress(0, "ui_finding_recommended_java");
            ReportProgress(0, "ui_reading_manifest", "forge");
            if (!File.Exists(PathHelper.ForgeManifestPath))
            {
                _logger.Error("Forge manifest file does not exist. Please ensure the manifest is downloaded.");
                return null;
            }

            VersionDetails forgeVersion = GameHelper.GetVersionDetails(PathHelper.VersionsDir, this.MinecraftVersion.Id, EMinecraftKind.FORGE, this.VersionData.CustomVersion, this.VersionData.GameDir);

            ReportProgress(0, $"ui_creating_directories");
            // Create versionDir in the versions folder
            if (!Directory.Exists(forgeVersion.VersionDirectory))
                Directory.CreateDirectory(forgeVersion.VersionDirectory);

            // Check libsizes dir
            string librarySizeCacheDir = Path.Combine(PathHelper.CacheDir, "libsizes");
            if (!Directory.Exists(librarySizeCacheDir))
                Directory.CreateDirectory(librarySizeCacheDir);

            // Download Installer
            ReportProgress(0, $"ui_downloading_installer", "forge", 0);
            string installerJarPath = Path.Combine(tempDir, "installer.jar");
            string installerDir = Path.Combine(tempDir, "installer");

            byte[]? bytes;
            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (sender, e) =>
            {
                ReportProgress(e, "ui_downloading_installer", MinecraftVersion.Id, e.ToString("0.00"));
            };

            try
            {
                bytes = await HttpHelper.GetByteArrayAsync(string.Format(ForgeEndpoints.InstallerJarUrl, $"{forgeVersion.MinecraftVersion}-{forgeVersion.CustomVersion}"), progress);
            }
            catch
            {
                int length = forgeVersion.MinecraftVersion.Split('.').Length;
                if (length == 3)
                {
                    bytes = await HttpHelper.GetByteArrayAsync(string.Format(ForgeEndpoints.InstallerJarUrl, $"{forgeVersion.MinecraftVersion}-{forgeVersion.CustomVersion}-{forgeVersion.MinecraftVersion}"), progress);
                    _extraVersion = $"-{forgeVersion.MinecraftVersion}";
                }
                else
                {
                    bytes = await HttpHelper.GetByteArrayAsync(string.Format(ForgeEndpoints.InstallerJarUrl, $"{forgeVersion.MinecraftVersion}-{forgeVersion.CustomVersion}-{forgeVersion.MinecraftVersion}.0"), progress);
                    _extraVersion = $"-{forgeVersion.MinecraftVersion}.0";
                }
            }
            if (bytes == null)
                return null;

            await File.WriteAllBytesAsync(installerJarPath, bytes);

            // Extract Installer
            ReportProgress(0, $"ui_extracting_installer", "forge");
            ZipFile.ExtractToDirectory(installerJarPath, installerDir);

            // Move version.json and profile.json 
            string installProfileJson = Path.Combine(forgeVersion.VersionDirectory, "install_profile.json");
            // INSTALL PROFILE
            if (!File.Exists(installProfileJson))
                File.Move(Path.Combine(installerDir, "install_profile.json"), installProfileJson);

            // EXTRACT UNIVERSAL
            string universalJarPath = Path.Combine(installerDir, $"forge-{forgeVersion.MinecraftVersion}-{forgeVersion.CustomVersion}{_extraVersion}-universal.jar");
            string universalDir = Path.Combine(installerDir, $"forge-{forgeVersion.MinecraftVersion}-{forgeVersion.CustomVersion}{_extraVersion}-universal");
            if (!Directory.Exists(universalDir) && File.Exists(universalJarPath))
            {
                ZipFile.ExtractToDirectory(universalJarPath, universalDir);
            }

            // COPY UNIVERSAL
            string forgeUniversalDir = Path.Combine(PathHelper.LibrariesDir, $"net\\minecraftforge\\forge\\{forgeVersion.MinecraftVersion}-{forgeVersion.CustomVersion}");
            string forgeUniversalPath = Path.Combine(forgeUniversalDir, $"forge-{forgeVersion.MinecraftVersion}-{forgeVersion.CustomVersion}{_extraVersion}-universal.jar");
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

            ReportProgress(0, $"ui_checking_installer_libraries", "forge");
            List<LibraryMeta> localLibraries = new List<LibraryMeta>();
            foreach (var lib in forgeVersionMeta.Libraries)
            {
                string? url = lib.GetUrl(true);
                if (url == null)
                    continue;

                localLibraries.Add(new LibraryMeta()
                {
                    Name = lib.Name,
                    Downloads = new LibraryDownloads()
                    {
                        Artifact = new Artifact()
                        {
                            Path = lib.GetPath(),
                            Sha1 = string.Empty,
                            Size = 0,
                            Url = url,
                        },
                        Classifiers = null
                    },
                    Natives = null,
                    Rules = new List<Rule>()
                });
            }

            // Add launch arguments
            ReportProgress(0, $"ui_adding_arguments", "forge");
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
                ReportProgress(0, $"ui_copying_jar", "vanilla");
                File.Copy(forgeVersion.VanillaJarPath, forgeVersion.VersionJarPath);
            }
            //_classPath += $"{forgeVersion.VersionJarPath};"; - not needed


            ModdedData moddedData = new ModdedData(forgeVersionMeta.MainClass, forgeVersion, localLibraries);
            return moddedData;
        }
    }
}
