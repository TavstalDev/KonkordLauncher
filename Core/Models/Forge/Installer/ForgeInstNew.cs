using System.IO.Compression;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models.Forge.New;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.Launcher;
using Tavstal.KonkordLauncher.Core.Models.Minecraft.Library;

namespace Tavstal.KonkordLauncher.Core.Models.Forge.Installer
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
        public ForgeInstNew() : base() { }

        public ForgeInstNew(Profile profile, Label label, ProgressBar progressBar, bool isDebug) : base(profile, label, progressBar, isDebug)
        {
        }

        internal override async Task<ModedData?> InstallModed(string tempDir)
        {
            UpdateProgressbarTranslated(0, "ui_finding_recommended_java");
            MinecraftVersionMeta.JavaVersion.FindOnSystem(out string? RecommendedJavaPath);
            if (!string.IsNullOrEmpty(RecommendedJavaPath) && string.IsNullOrEmpty(Profile.JavaPath))
            {
                if (Directory.Exists(RecommendedJavaPath))
                    JavaPath = Path.Combine(RecommendedJavaPath, "bin", IsDebug ? "java.exe" : "javaw.exe");
            }

            UpdateProgressbarTranslated(0, "ui_reading_manifest", new object[] { "forge" });
            if (!File.Exists(IOHelper.ForgeManifestJsonFile))
            {
                NotificationHelper.SendErrorTranslated("manifest_file_not_found", "messagebox_error", new object[] { "forge" });
                return null;
            }

            VersionDetails forgeVersion = GameHelper.GetProfileVersionDetails(EProfileKind.FORGE, Profile.VersionId, Profile.VersionVanillaId, Profile.GameDirectory);

            UpdateProgressbarTranslated(0, $"ui_creating_directories");
            // Create versionDir in the versions folder
            if (!Directory.Exists(forgeVersion.VersionDirectory))
                Directory.CreateDirectory(forgeVersion.VersionDirectory);

            // Check libsizes dir
            string librarySizeCacheDir = Path.Combine(IOHelper.CacheDir, "libsizes");
            if (!Directory.Exists(librarySizeCacheDir))
                Directory.CreateDirectory(librarySizeCacheDir);

            // Download Installer
            UpdateProgressbarTranslated(0, $"ui_downloading_installer", new object[] { "forge", 0 });
            string installerJarPath = Path.Combine(tempDir, "installer.jar");
            string installerDir = Path.Combine(tempDir, "installer");

            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (sender, e) =>
            {
                UpdateProgressbarTranslated(e, "ui_downloading_installer", new object[] { "forge", e.ToString("0.00") });
            };

            byte[]? bytes = await HttpHelper.GetByteArrayAsync(string.Format(ForgeInstallerJarUrl, $"{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}"), progress);
            if (bytes == null)
                return null;
            
            await File.WriteAllBytesAsync(installerJarPath, bytes);

            // Extract Installer
            UpdateProgressbarTranslated(0, $"ui_extracting_installer", new object[] { "forge" });
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
                    string newPath = file.Replace(mavenTempDir, IOHelper.LibrariesDir);
                    string newDir = IOHelper.GetDirectory(newPath);

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

            UpdateProgressbarTranslated(0, $"ui_checking_installer_libraries", new object[] { "forge" });
            List<MCLibrary> localLibraries = new List<MCLibrary>();
            localLibraries.AddRange(forgeVersionMeta.Libraries);

            // Download installer libraries
            string librarySizeCachePath = Path.Combine(librarySizeCacheDir, $"{forgeVersion.VanillaVersion}-forge-installer-{forgeVersion.InstanceVersion}.json");

                int downloadedSize = 0;
                int toDownloadSize = 0;
                if (!File.Exists(librarySizeCachePath))
                {
                    foreach (MCLibrary lib in installProfile.Libraries)
                        toDownloadSize += lib.Downloads.Artifact.Size;
                    UpdateProgressbarTranslated(0, $"ui_saving_lib_cache");
                    await File.WriteAllTextAsync(librarySizeCachePath, toDownloadSize.ToString());
                }
                else
                    toDownloadSize = int.Parse(await File.ReadAllTextAsync(librarySizeCachePath));

            foreach (MCLibrary lib in installProfile.Libraries)
            {
                string localPath = lib.Downloads.Artifact.Path;
                string libDirPath = Path.Combine(IOHelper.LibrariesDir, localPath.Remove(localPath.LastIndexOf('/'), localPath.Length - localPath.LastIndexOf('/')));
                if (!Directory.Exists(libDirPath))
                    Directory.CreateDirectory(libDirPath);
                string libFilePath = Path.Combine(IOHelper.LibrariesDir, localPath);
                if (!File.Exists(libFilePath))
                {
                    if (!string.IsNullOrEmpty(lib.Downloads.Artifact.Url))
                    {
                        Progress<double> libProgress = new Progress<double>();
                        libProgress.ProgressChanged += (sender, e) =>
                        {
                            UpdateProgressbarTranslated(e, "ui_library_download", new object[] { lib.Name, e.ToString("0.00") });
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
            UpdateProgressbarTranslated(0, $"ui_adding_arguments", new object[] { "forge" });
            if (forgeVersionMeta.Arguments != null)
            {
                if (forgeVersionMeta.Arguments.Game != null)
                    foreach (var arg in forgeVersionMeta.Arguments.GetGameArgs())
                        _gameArguments.Add(new LaunchArg(arg, 1));

                if (forgeVersionMeta.Arguments.JVM != null)
                    foreach (var arg in forgeVersionMeta.Arguments.GetJVMArgs())
                        _jvmArguments.Add(new LaunchArg(arg, 1));
            }

            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-DMcEmu=net.minecraft.client.main.Main", 2));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Dlog4j2.formatMsgNoLookups=true", 2));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Djava.rmi.server.useCodebaseOnly=true", 2));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Dcom.sun.jndi.rmi.object.trustURLCodebase=false", 2));

            UpdateProgressbarTranslated(0, $"ui_building", new object[] { "forge", "0" });
            // Generate client libs
            await MapAndStartProcessors(installProfile, installerDir);

            #region GET minecraftforge client libs
            string forgeUniversal = string.Format(ForgeLoaderUniversalJarUrl, $"{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}");

            string forgeUniversalDir = Path.Combine(IOHelper.LibrariesDir, $"net\\minecraftforge\\forge\\{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}");
            string forgeUniversalPath = Path.Combine(forgeUniversalDir, $"forge-{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}-universal.jar");
            if (!Directory.Exists(forgeUniversalDir))
                Directory.CreateDirectory(forgeUniversalDir);

            UpdateProgressbarTranslated(0, $"ui_checking_forge_universal");
            if (!File.Exists(forgeUniversalPath))
            {
                UpdateProgressbarTranslated(0, $"ui_downloading_forge_universal", new byte[] { 0 });

                Progress<double> univProgress = new Progress<double>();
                univProgress.ProgressChanged += (sender, e) =>
                {
                    UpdateProgressbarTranslated(e, "ui_downloading_forge_universal", new object[] { e.ToString("0.00") });
                };

                byte[]? univBytes = await HttpHelper.GetByteArrayAsync(forgeUniversal, univProgress);
                if (univBytes == null)
                    return null;
                await File.WriteAllBytesAsync(forgeUniversalPath, univBytes);
            }
            #endregion

            ModedData modedData = new ModedData(forgeVersionMeta.MainClass, forgeVersion, localLibraries);
            return modedData;
        }
    }
}
