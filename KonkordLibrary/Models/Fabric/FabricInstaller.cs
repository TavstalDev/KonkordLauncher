using KonkordLibrary.Enums;
using KonkordLibrary.Helpers;
using KonkordLibrary.Models.Installer;
using KonkordLibrary.Models.Launcher;
using KonkordLibrary.Models.Minecraft.Library;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Windows.Controls;

namespace KonkordLibrary.Models.Fabric
{
    public class FabricInstaller : MinecraftInstaller
    {
        #region Variables
        private static readonly string _fabricVersionManifestUrl = "https://meta.fabricmc.net/v2/versions";
        public static string FabricVersionManifestUrl { get { return _fabricVersionManifestUrl; } }

        private static readonly string _fabricLoaderJsonUrl = "https://meta.fabricmc.net/v2/versions/loader/{0}/{1}/profile/json";
        // https://meta.fabricmc.net/v2/versions/loader/1.16.5/0.15.6/profile/json
        public static string FabricLoaderJsonUrl { get { return _fabricLoaderJsonUrl; } }
        private static readonly string _fabricLoaderJarUrl = "https://maven.fabricmc.net/net/fabricmc/fabric-loader/{0}/fabric-loader-{0}.jar";
        // Version Example: 0.15.6
        public static string FabricLoaderJarUrl { get { return _fabricLoaderJarUrl; } }
        #endregion

        public FabricInstaller(Profile profile, Label label, ProgressBar progressBar, bool isDebug) : base(profile, label, progressBar, isDebug)
        {

        }

        internal override async Task<ModedData?> InstallModed(string tempDir)
        {
            UpdateProgressbarTranslated(0, "ui_reading_manifest", new object[] { "fabricManifest" });
            if (!File.Exists(IOHelper.FabricManifestJsonFile))
            {
                NotificationHelper.SendErrorTranslated("manifest_file_not_found", "messagebox_error", new object[] { "fabric" });
                return null;
            }

            VersionDetails fabricVersion = GameHelper.GetProfileVersionDetails(EProfileKind.FABRIC, Profile.VersionId, Profile.VersionVanillaId, Profile.GameDirectory);

            // Create versionDir in the versions folder
            if (!Directory.Exists(fabricVersion.VersionDirectory))
                Directory.CreateDirectory(fabricVersion.VersionDirectory);

            // Check libsizes dir
            string librarySizeCacheDir = Path.Combine(IOHelper.CacheDir, "libsizes");
            if (!Directory.Exists(librarySizeCacheDir))
                Directory.CreateDirectory(librarySizeCacheDir);
            string librarySizeCachePath = Path.Combine(librarySizeCacheDir, $"{fabricVersion.VanillaVersion}-fabric-{fabricVersion.InstanceVersion}.json");

            // Download version json
            FabricVersionMeta? fabricVersionMeta = null;
            List<MCLibrary> localLibraries = new List<MCLibrary>();
            if (!File.Exists(fabricVersion.VersionJsonPath))
            {
                string resultJson = string.Empty;
                UpdateProgressbarTranslated(0, $"ui_downloading_version_json", new object[] { "fabric" });
                using (HttpClient client = new HttpClient())
                {
                    resultJson = await client.GetStringAsync(string.Format(FabricLoaderJsonUrl, fabricVersion.VanillaVersion, fabricVersion.InstanceVersion));
                    await File.WriteAllTextAsync(fabricVersion.VersionJsonPath, resultJson);
                }

                // Add the libraries
                fabricVersionMeta = JsonConvert.DeserializeObject<FabricVersionMeta>(resultJson);
                int localLibrarySize = 0;
                if (fabricVersionMeta == null)
                {
                    File.Delete(fabricVersion.VersionJsonPath); // Delete it because this if part won't be executed again if it exists
                    NotificationHelper.SendErrorTranslated("version_meta_invalid", "messagebox_error", new object[] { "fabric" });
                    return null;
                }

                UpdateProgressbarTranslated(0, $"ui_reading_version_json", new object[] { "fabric" });
                foreach (var lib in fabricVersionMeta.Libraries)
                {
                    localLibrarySize += lib.Size;
                    localLibraries.Add(new MCLibrary(lib.Name, new MCLibraryDownloads(new MCLibraryArtifact(lib.GetPath(), lib.Sha1, lib.Size, lib.GetURL()), null), new List<MCLibraryRule>()));
                }
                // Save the version cache
                await JsonHelper.WriteJsonFileAsync(librarySizeCachePath, localLibrarySize);
            }
            else
            {
                UpdateProgressbarTranslated(0, $"ui_reading_version_json", new object[] { "fabric" });
                fabricVersionMeta = JsonConvert.DeserializeObject<FabricVersionMeta>(await File.ReadAllTextAsync(fabricVersion.VersionJsonPath));
                if (fabricVersionMeta == null)
                {
                    NotificationHelper.SendErrorTranslated("version_meta_invalid", "messagebox_error", new object[] { "fabric" });
                    return null;
                }

                foreach (var lib in fabricVersionMeta.Libraries)
                {
                    localLibraries.Add(new MCLibrary(lib.Name, new MCLibraryDownloads(new MCLibraryArtifact(lib.GetPath(), lib.Sha1, lib.Size, lib.GetURL()), null), new List<MCLibraryRule>()));
                }
            }


            // Download Loader
            string loaderDirPath = Path.Combine(IOHelper.LibrariesDir, $"net\\fabricmc\\fabric-loader\\{fabricVersion.InstanceVersion}");
            string loaderJarPath = Path.Combine(loaderDirPath, $"fabric-loader-{fabricVersion.InstanceVersion}.jar");
            if (!Directory.Exists(loaderDirPath))
                Directory.CreateDirectory(loaderDirPath);

            if (!File.Exists(loaderJarPath))
            {
                UpdateProgressbarTranslated(0, $"ui_downloading_loader", new object[] { "fabric" });
                using (HttpClient client = new HttpClient())
                {
                    byte[] bytes = await client.GetByteArrayAsync(string.Format(FabricLoaderJarUrl, fabricVersion.InstanceVersion));
                    await File.WriteAllBytesAsync(loaderJarPath, bytes);
                }
            }

            UpdateProgressbarTranslated(0, $"ui_getting_launch_arguments");
            if (!File.Exists(fabricVersion.VersionJarPath))
            {
                UpdateProgressbarTranslated(0, $"ui_copying_jar", new object[] { "vanilla" });
                File.Copy(fabricVersion.VanillaJarPath, fabricVersion.VersionJarPath);
            }

            ModedData modedData = new ModedData(fabricVersionMeta.MainClass, fabricVersion, localLibraries);

            foreach (var arg in fabricVersionMeta.Arguments.GetGameArgs())
                _gameArguments.Add(new LaunchArg(arg, 1));

            foreach (var arg in fabricVersionMeta.Arguments.GetJVMArgs())
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
            return modedData;
        }
    }
}
