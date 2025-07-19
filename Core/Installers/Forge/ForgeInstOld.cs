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
     * 1.7.10+ are legacy
     * 1.6 - ok
     * 1.5.2 -
     * these below are zips
     * 1.5.1 -
     * 1.4 -
     * 1.3 -
     * 1.2 -
     * 1.1 - 
     * 1.0 - 
    */
    public class ForgeInstOld : ForgeInstallerBase
    {
        private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(ForgeInstOld));
        private string _extraVersion { get; set; }
        
        public ForgeInstOld(string javaPath, string minecraftVersion, int memory, LauncherDetails launcherDetails, ClientDetails clientDetails, 
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
            ReportProgress(0, $"ui_downloading_installer", "forge");

            // Check Minecraft Version
            string[] rawVersion = VersionData.MinecraftVersion.Split('.');
            int MajorVersion = int.Parse(rawVersion[1]);
            int MinorVersion = 0;
            if (rawVersion.Length == 3)
                MinorVersion = int.Parse(rawVersion[2]);

            string installerFormat = ".jar";
            string forgeInstallerUrl = ForgeEndpoints.InstallerJarUrl;
            bool isZipInstaller = false;
            if (MajorVersion == 5 && MinorVersion < 2 || MajorVersion < 5)
            {
                installerFormat = ".zip";
                forgeInstallerUrl = ForgeEndpoints.LoaderUniversalJarUrl.Replace(".jar", ".zip");
                isZipInstaller = true;
            }

            string installerJarPath = Path.Combine(tempDir, $"installer{installerFormat}");
            string installerDir = Path.Combine(tempDir, "installer");
            byte[]? bytes;
            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (sender, e) =>
            {
                ReportProgress(e, "ui_downloading_installer", MinecraftVersion.Id, e.ToString("0.00"));
            };

            try
            {
                bytes = await HttpHelper.GetByteArrayAsync(string.Format(forgeInstallerUrl, $"{forgeVersion.MinecraftVersion}-{forgeVersion.CustomVersion}"), progress);
            }
            catch
            {
                int length = forgeVersion.MinecraftVersion.Split('.').Length;
                if (length == 3)
                {
                    bytes = await HttpHelper.GetByteArrayAsync(string.Format(forgeInstallerUrl, $"{forgeVersion.MinecraftVersion}-{forgeVersion.CustomVersion}-{forgeVersion.MinecraftVersion}"), progress);
                    _extraVersion = $"-{forgeVersion.MinecraftVersion}";
                }
                else
                {
                    bytes = await HttpHelper.GetByteArrayAsync(string.Format(forgeInstallerUrl, $"{forgeVersion.MinecraftVersion}-{forgeVersion.CustomVersion}-{forgeVersion.MinecraftVersion}.0"), progress);
                    _extraVersion = $"-{forgeVersion.MinecraftVersion}.0";
                }
            }
            if (bytes == null)
                return null;
            await File.WriteAllBytesAsync(installerJarPath, bytes);

            // Extract Installer
            ReportProgress(0, $"ui_extracting_installer", "forge");
            ZipFile.ExtractToDirectory(installerJarPath, installerDir);

            ModdedData moddedData;
            if (isZipInstaller) // Old Zip Installer
            {
                // Extract Vanilla

                // Remove META-INF

                // Copy Installer to Vanilla.jar


                // Package vanilla.jar

                moddedData = new ModdedData("net.minecraft.client.main.Main", forgeVersion, new List<LibraryMeta>());
                throw new NotImplementedException($"The '{VersionData.MinecraftVersion}-{VersionData.CustomVersion}' forge version is not supported for now.");
            }
            else // Jar Installer
            {
                // Move version.json and profile.json 
                string installProfileJson = Path.Combine(forgeVersion.VersionDirectory, "install_profile.json");
                // INSTALL PROFILE
                if (!File.Exists(installProfileJson))
                    File.Move(Path.Combine(installerDir, "install_profile.json"), installProfileJson);

                // EXTRACT UNIVERSAL
                string universalJarPath = Path.Combine(installerDir, $"minecraftforge-universal-{forgeVersion.MinecraftVersion}-{forgeVersion.CustomVersion}{_extraVersion}.jar");
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

                ForgeProfile? installProfile = JsonConvert.DeserializeObject<ForgeProfile>(await File.ReadAllTextAsync(installProfileJson));
                if (installProfile == null)
                    throw new FileNotFoundException("Failed to get the forge install profile meta.");

                // VERSION
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
                if (forgeVersionMeta.MinecraftArguments != null)
                {
                    MinecraftVersionMeta.ArgumentsLegacy = forgeVersionMeta.MinecraftArguments;
                }

                // Copy vanilla jar
                if (!File.Exists(forgeVersion.VersionJarPath))
                {
                    ReportProgress(0, $"ui_copying_jar", "vanilla");
                    File.Copy(forgeVersion.VanillaJarPath, forgeVersion.VersionJarPath);
                }

                moddedData = new ModdedData(forgeVersionMeta.MainClass, forgeVersion, localLibraries);
            }

            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-DMcEmu=net.minecraft.client.main.Main", 2));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Dlog4j2.formatMsgNoLookups=true", 2));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Djava.rmi.server.useCodebaseOnly=true", 2));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Dcom.sun.jndi.rmi.object.trustURLCodebase=false", 2));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg($"-Dminecraft.client.jar={forgeVersion.VersionJarPath}", 2));


            //_classPath += $"{forgeVersion.VersionJarPath};"; - not needed
            return moddedData;
        }
    }
}
