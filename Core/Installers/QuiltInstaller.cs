using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.Launcher;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Fabric;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta.Library;

namespace Tavstal.KonkordLauncher.Core.Installers
{
    public class QuiltInstaller : MinecraftInstaller
    {
        #region Variables
        private static readonly string _quiltVersionManifestUrl = "https://meta.quiltmc.org/v3/versions";
        public static string QuiltVersionManifestUrl { get { return _quiltVersionManifestUrl; } }
        private static readonly string _quiltLoaderJsonUrl = "https://meta.quiltmc.org/v3/versions/loader/{0}/{1}/profile/json";
        // https://meta.quiltmc.org/v3/versions/loader/1.20.4/0.24.0/profile/json
        public static string QuiltLoaderJsonUrl { get { return _quiltLoaderJsonUrl; } }
        private static readonly string _quiltLoaderJarUrl = "https://maven.quiltmc.org/repository/release/org/quiltmc/quilt-loader/{0}/quilt-loader-{0}.jar";
        // Version Example: 0.24.0
        public static string QuiltLoaderJarUrl { get { return _quiltLoaderJarUrl; } }
        #endregion

        public QuiltInstaller(Profile profile, IProgressReporter progressReporter, bool isDebug) : base(profile, progressReporter, isDebug)
        {

        }

        internal override async Task<ModdedData?> InstallModed(string tempDir)
        {
            UpdateProgressbarTranslated(0, "ui_reading_manifest", new object[] { "quiltManifest" });
            if (!File.Exists(IOHelper.QuiltManifestJsonFile))
            {
                NotificationHelper.SendErrorTranslated("manifest_file_not_found", "messagebox_error", new object[] { "quilt" });
                return null;
            }

            VersionDetails quiltVersion = GameHelper.GetProfileVersionDetails(EProfileKind.QUILT, Profile.VersionId, Profile.VersionVanillaId, Profile.GameDirectory);

            // Create versionDir in the versions folder
            if (!Directory.Exists(quiltVersion.VersionDirectory))
                Directory.CreateDirectory(quiltVersion.VersionDirectory);

            // Check libsizes dir
            string librarySizeCacheDir = Path.Combine(IOHelper.CacheDir, "libsizes");
            if (!Directory.Exists(librarySizeCacheDir))
                Directory.CreateDirectory(librarySizeCacheDir);
            string librarySizeCachePath = Path.Combine(librarySizeCacheDir, $"{quiltVersion.VanillaVersion}-quilt-{quiltVersion.InstanceVersion}.json");

            // Download version json
            FabricVersionMeta? quiltVersionMeta = null;
            List<LibraryMeta> localLibraries = new List<LibraryMeta>();
            if (!File.Exists(quiltVersion.VersionJsonPath))
            {
                string? resultJson = string.Empty;
                UpdateProgressbarTranslated(0, $"ui_downloading_version_json", new object[] { "quilt", 0 });

                Progress<double> progress = new Progress<double>();
                progress.ProgressChanged += (sender, e) =>
                {
                    UpdateProgressbarTranslated(e, "ui_downloading_version_json", new object[] { "quilt", e.ToString("0.00") });
                };

                resultJson = await HttpHelper.GetStringAsync(string.Format(QuiltLoaderJsonUrl, quiltVersion.VanillaVersion, quiltVersion.InstanceVersion), progress);
                if (resultJson == null)
                    return null;

                await File.WriteAllTextAsync(quiltVersion.VersionJsonPath, resultJson);

                // Add the libraries
                quiltVersionMeta = JsonConvert.DeserializeObject<FabricVersionMeta>(resultJson);
                int localLibrarySize = 0;
                if (quiltVersionMeta == null)
                {
                    File.Delete(quiltVersion.VersionJsonPath); // Delete it because this if part won't be executed again if it exists
                    NotificationHelper.SendErrorTranslated("version_meta_invalid", "messagebox_error", new object[] { "quilt" });
                    return null;
                }

                UpdateProgressbarTranslated(0, $"ui_reading_version_json", new object[] { "quilt" });
                foreach (var lib in quiltVersionMeta.Libraries)
                {
                    localLibrarySize += lib.Size;
                    localLibraries.Add(new LibraryMeta(lib.Name, new LibraryDownloads(new Artifact(lib.GetPath(), lib.Sha1, lib.Size, lib.GetURL()), null), new List<Rule>()));
                }
                // Save the version cache
                await JsonHelper.WriteJsonFileAsync(librarySizeCachePath, localLibrarySize);
            }
            else
            {
                UpdateProgressbarTranslated(0, $"ui_reading_version_json", new object[] { "quilt" });
                quiltVersionMeta = JsonConvert.DeserializeObject<FabricVersionMeta>(await File.ReadAllTextAsync(quiltVersion.VersionJsonPath));
                if (quiltVersionMeta == null)
                {
                    NotificationHelper.SendErrorTranslated("version_meta_invalid", "messagebox_error", new object[] { "quilt" });
                    return null;
                }

                foreach (var lib in quiltVersionMeta.Libraries)
                {
                    localLibraries.Add(new LibraryMeta(lib.Name, new LibraryDownloads(new Artifact(lib.GetPath(), lib.Sha1, lib.Size, lib.GetURL()), null), new List<Rule>()));
                }
            }


            // Download Loader
            string loaderDirPath = Path.Combine(IOHelper.LibrariesDir, $"net\\fabricmc\\fabric-loader\\{quiltVersion.InstanceVersion}");
            string loaderJarPath = Path.Combine(loaderDirPath, $"fabric-loader-{quiltVersion.InstanceVersion}.jar");
            if (!Directory.Exists(loaderDirPath))
                Directory.CreateDirectory(loaderDirPath);

            if (!File.Exists(loaderJarPath))
            {
                UpdateProgressbarTranslated(0, $"ui_downloading_loader", new object[] { "quilt", 0 });

                Progress<double> progress = new Progress<double>();
                progress.ProgressChanged += (sender, e) =>
                {
                    UpdateProgressbarTranslated(e, "ui_downloading_loader", new object[] { "quilt", e.ToString("0.00") });
                };

                byte[]? bytes = await HttpHelper.GetByteArrayAsync(string.Format(QuiltLoaderJarUrl, quiltVersion.InstanceVersion), progress);
                if (bytes == null)
                    return null;
                await File.WriteAllBytesAsync(loaderJarPath, bytes);
            }

            UpdateProgressbarTranslated(0, $"ui_getting_launch_arguments");
            if (!File.Exists(quiltVersion.VersionJarPath))
            {
                UpdateProgressbarTranslated(0, $"ui_copying_jar", new object[] { "vanilla" });
                File.Copy(quiltVersion.VanillaJarPath, quiltVersion.VersionJarPath);
            }

            ModdedData moddedData = new ModdedData(quiltVersionMeta.MainClass, quiltVersion, localLibraries);

            foreach (var arg in quiltVersionMeta.Arguments.GetGameArgs())
                _gameArguments.Add(new LaunchArg(arg, 1));

            foreach (var arg in quiltVersionMeta.Arguments.GetJvmArgs())
            {
                if (arg == "-DFabricMcEmu= net.minecraft.client.main.Main ")
                {
                    _jvmArguments.Add(new LaunchArg("\"-DFabricMcEmu= net.minecraft.client.main.Main \"", 1));
                    continue;
                }

                _jvmArguments.Add(new LaunchArg(arg, 1));
            }

            _jvmArguments.Add(new LaunchArg("-DMcEmu=net.minecraft.client.main.Main", 1));
            _jvmArguments.Add(new LaunchArg("-Dlog4j2.formatMsgNoLookups=true", 1));
            _jvmArguments.Add(new LaunchArg("-Djava.rmi.server.useCodebaseOnly=true", 1));
            _jvmArguments.Add(new LaunchArg("-Dcom.sun.jndi.rmi.object.trustURLCodebase=false", 1));
            return moddedData;
        }
    }
}
