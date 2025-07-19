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

namespace Tavstal.KonkordLauncher.Core.Installers.Forge
{
    /* 1.20 - ok
     * 1.19 - ok
     * 1.18 - ok
     * 1.17 - ok
     * 1.16 - ok #s java version should be changed
     * 1.15 - ok
     * 1.14 - ok
     * 1.13 - ok #e
     * 1.12.2 and below - LEGACY
    */
    public class ForgeInstNew : ForgeInstallerBase
    {
        private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(ForgeInstNew));
        public ForgeInstNew(string javaPath, string minecraftVersion, int memory, LauncherDetails launcherDetails, ClientDetails clientDetails, 
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

            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (sender, e) =>
            {
                ReportProgress(e, "ui_downloading_installer", "forge", e.ToString("0.00"));
            };

            byte[]? bytes = await HttpHelper.GetByteArrayAsync(string.Format(ForgeEndpoints.InstallerJarUrl, $"{forgeVersion.MinecraftVersion}-{forgeVersion.CustomVersion}"), progress);
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
                    string newPath = file.Replace(mavenTempDir, PathHelper.LibrariesDir);
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

            ReportProgress(0, $"ui_checking_installer_libraries", "forge");
            List<LibraryMeta> localLibraries = new List<LibraryMeta>();
            localLibraries.AddRange(forgeVersionMeta.Libraries);

            // Download installer libraries
            string librarySizeCachePath = Path.Combine(librarySizeCacheDir, $"{forgeVersion.MinecraftVersion}-forge-installer-{forgeVersion.CustomVersion}.json");

                int downloadedSize = 0;
                int toDownloadSize = 0;
                if (!File.Exists(librarySizeCachePath))
                {
                    foreach (LibraryMeta lib in installProfile.Libraries)
                        toDownloadSize += lib.Downloads.Artifact.Size;
                    ReportProgress(0, $"ui_saving_lib_cache");
                    await File.WriteAllTextAsync(librarySizeCachePath, toDownloadSize.ToString());
                }
                else
                    toDownloadSize = int.Parse(await File.ReadAllTextAsync(librarySizeCachePath));

            foreach (LibraryMeta lib in installProfile.Libraries)
            {
                string localPath = lib.Downloads.Artifact.Path;
                string libDirPath = Path.Combine(PathHelper.LibrariesDir, localPath.Remove(localPath.LastIndexOf('/'), localPath.Length - localPath.LastIndexOf('/')));
                if (!Directory.Exists(libDirPath))
                    Directory.CreateDirectory(libDirPath);
                string libFilePath = Path.Combine(PathHelper.LibrariesDir, localPath);
                if (!File.Exists(libFilePath))
                {
                    if (!string.IsNullOrEmpty(lib.Downloads.Artifact.Url))
                    {
                        Progress<double> libProgress = new Progress<double>();
                        libProgress.ProgressChanged += (_, e) =>
                        {
                            ReportProgress(e, "ui_library_download", lib.Name, e.ToString("0.00"));
                        };

                        byte[]? libBytes = await HttpHelper.GetByteArrayAsync(lib.Downloads.Artifact.Url, libProgress);
                        if (libBytes == null)
                            continue;
                        await File.WriteAllBytesAsync(libFilePath, libBytes);
                        downloadedSize += lib.Downloads.Artifact.Size;
                    }
                    /*double percent = (double)downloadedSize / (double)toDownloadSize * 100d;
                    UpdateProgressbarTranslated(percent, $"ui_library_download", new object[] { lib.Name, percent.ToString("0.00") });*/
                }
            }

            // Add launch arguments
            ReportProgress(0, $"ui_adding_arguments", "forge");
            if (forgeVersionMeta.Arguments != null)
            {
                if (forgeVersionMeta.Arguments.Game != null)
                    foreach (var arg in forgeVersionMeta.Arguments.GetGameArgs())
                        _gameArguments.Add(new LaunchArg(arg, 1));

                if (forgeVersionMeta.Arguments.Jvm != null)
                    foreach (var arg in forgeVersionMeta.Arguments.GetJvmArgs())
                        _jvmArguments.Add(new LaunchArg(arg, 1));
            }

            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-DMcEmu=net.minecraft.client.main.Main", 2));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Dlog4j2.formatMsgNoLookups=true", 2));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Djava.rmi.server.useCodebaseOnly=true", 2));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Dcom.sun.jndi.rmi.object.trustURLCodebase=false", 2));

            ReportProgress(0, $"ui_building", "forge", "0");
            // Generate client libs
            await MapAndStartProcessors(installProfile, installerDir);

            #region GET minecraftforge client libs
            string forgeUniversal = string.Format(ForgeEndpoints.LoaderUniversalJarUrl, $"{forgeVersion.MinecraftVersion}-{forgeVersion.CustomVersion}");

            string forgeUniversalDir = Path.Combine(PathHelper.LibrariesDir, $"net\\minecraftforge\\forge\\{forgeVersion.MinecraftVersion}-{forgeVersion.CustomVersion}");
            string forgeUniversalPath = Path.Combine(forgeUniversalDir, $"forge-{forgeVersion.MinecraftVersion}-{forgeVersion.CustomVersion}-universal.jar");
            if (!Directory.Exists(forgeUniversalDir))
                Directory.CreateDirectory(forgeUniversalDir);

            ReportProgress(0, $"ui_checking_forge_universal");
            if (!File.Exists(forgeUniversalPath))
            {
                ReportProgress(0, $"ui_downloading_forge_universal", new byte[] { 0 });

                Progress<double> univProgress = new Progress<double>();
                univProgress.ProgressChanged += (_, e) =>
                {
                    ReportProgress(e, "ui_downloading_forge_universal", e.ToString("0.00"));
                };

                byte[]? univBytes = await HttpHelper.GetByteArrayAsync(forgeUniversal, univProgress);
                if (univBytes == null)
                    return null;
                await File.WriteAllBytesAsync(forgeUniversalPath, univBytes);
            }
            #endregion

            ModdedData moddedData = new ModdedData(forgeVersionMeta.MainClass, forgeVersion, localLibraries);
            return moddedData;
        }
    }
}
