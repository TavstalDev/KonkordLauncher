using KonkordLibrary.Enums;
using KonkordLibrary.Helpers;
using KonkordLibrary.Models.Fabric;
using KonkordLibrary.Models.Installer;
using KonkordLibrary.Models.Minecraft.Library;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Windows.Controls;

namespace KonkordLibrary.Models.Quilt
{
    public class QuiltInstaller : MinecraftInstaller
    {
        private static readonly string _quiltVersionManifestUrl = "https://meta.quiltmc.org/v3/versions";
        public static string QuiltVersionManifestUrl { get { return _quiltVersionManifestUrl; } }
        private static readonly string _quiltLoaderJsonUrl = "https://meta.quiltmc.org/v3/versions/loader/{0}/{1}/profile/json";
        // https://meta.quiltmc.org/v3/versions/loader/1.20.4/0.24.0/profile/json
        public static string QuiltLoaderJsonUrl { get { return _quiltLoaderJsonUrl; } }
        private static readonly string _quiltLoaderJarUrl = "https://maven.quiltmc.org/repository/release/org/quiltmc/quilt-loader/{0}/quilt-loader-{0}.jar";
        // Version Example: 0.24.0
        public static string QuiltLoaderJarUrl { get { return _quiltLoaderJarUrl; } }

        public QuiltInstaller(Profile profile, Label label, ProgressBar progressBar, bool isDebug) : base(profile, label, progressBar, isDebug)
        {

        }

        internal override async Task<ModedData?> InstallModed(string tempDir)
        {
            UpdateProgressbar(0, $"Reading the quiltManifest file...");
            if (!File.Exists(IOHelper.QuiltManifestJsonFile))
            {
                NotificationHelper.SendError("Failed to get the quilt manifest.", "Error");
                return null;
            }

            VersionDetails quiltVersion = Managers.GameManager.GetProfileVersionDetails(EProfileKind.QUILT, Profile.VersionId, Profile.VersionVanillaId, Profile.GameDirectory);

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
            List<MCLibrary> localLibraries = new List<MCLibrary>();
            if (!File.Exists(quiltVersion.VersionJsonPath))
            {
                string resultJson = string.Empty;
                UpdateProgressbar(0, $"Downloading the quilt version json file...");
                using (HttpClient client = new HttpClient())
                {
                    resultJson = await client.GetStringAsync(string.Format(QuiltLoaderJsonUrl, quiltVersion.VanillaVersion, quiltVersion.InstanceVersion));
                    await File.WriteAllTextAsync(quiltVersion.VersionJsonPath, resultJson);
                }

                // Add the libraries
                quiltVersionMeta = JsonConvert.DeserializeObject<FabricVersionMeta>(resultJson);
                int localLibrarySize = 0;
                if (quiltVersionMeta == null)
                {
                    File.Delete(quiltVersion.VersionJsonPath); // Delete it because this if part won't be executed again if it exists
                    NotificationHelper.SendError("Failed to get the quilt version meta", "Error");
                    return null;
                }

                UpdateProgressbar(0, $"Reading the quilt version json file...");
                foreach (var lib in quiltVersionMeta.Libraries)
                {
                    localLibrarySize += lib.Size;
                    localLibraries.Add(new MCLibrary(lib.Name, new MCLibraryDownloads(new MCLibraryArtifact(lib.GetPath(), lib.Sha1, lib.Size, lib.GetURL()), null), new List<MCLibraryRule>()));
                }
                // Save the version cache
                await JsonHelper.WriteJsonFileAsync(librarySizeCachePath, localLibrarySize);
            }
            else
            {
                UpdateProgressbar(0, $"Reading the quilt version json file...");
                quiltVersionMeta = JsonConvert.DeserializeObject<FabricVersionMeta>(await File.ReadAllTextAsync(quiltVersion.VersionJsonPath));
                if (quiltVersionMeta == null)
                {
                    NotificationHelper.SendError("Failed to get the quilt version meta", "Error");
                    return null;
                }

                foreach (var lib in quiltVersionMeta.Libraries)
                {
                    localLibraries.Add(new MCLibrary(lib.Name, new MCLibraryDownloads(new MCLibraryArtifact(lib.GetPath(), lib.Sha1, lib.Size, lib.GetURL()), null), new List<MCLibraryRule>()));
                }
            }


            // Download Loader
            string loaderDirPath = Path.Combine(IOHelper.LibrariesDir, $"net\\fabricmc\\fabric-loader\\{quiltVersion.InstanceVersion}");
            string loaderJarPath = Path.Combine(loaderDirPath, $"fabric-loader-{quiltVersion.InstanceVersion}.jar");
            if (!Directory.Exists(loaderDirPath))
                Directory.CreateDirectory(loaderDirPath);

            if (!File.Exists(loaderJarPath))
            {
                UpdateProgressbar(0, $"Downloading the quilt loader...");
                using (HttpClient client = new HttpClient())
                {
                    byte[] bytes = await client.GetByteArrayAsync(string.Format(QuiltLoaderJarUrl, quiltVersion.InstanceVersion));
                    await File.WriteAllBytesAsync(loaderJarPath, bytes);
                }
            }

            UpdateProgressbar(0, $"Getting launch arguments...");
            if (!File.Exists(quiltVersion.VersionJarPath))
            {
                UpdateProgressbar(0, $"Copying the vanilla jar file...");
                File.Copy(quiltVersion.VanillaJarPath, quiltVersion.VersionJarPath);
            }

            ModedData modedData = new ModedData(quiltVersionMeta.MainClass, quiltVersion, localLibraries);

            foreach (var arg in quiltVersionMeta.Arguments.GetGameArgs())
                _gameArguments.Add(new LaunchArg(arg, 1));

            foreach (var arg in quiltVersionMeta.Arguments.GetJVMArgs())
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
