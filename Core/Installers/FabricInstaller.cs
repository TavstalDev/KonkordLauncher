using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Fabric;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta.Library;

namespace Tavstal.KonkordLauncher.Core.Installers
{
    public class FabricInstaller : MinecraftInstaller
    {
        private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(FabricInstaller));
        
        public FabricInstaller(string javaPath, string minecraftVersion, int memory, LauncherDetails launcherDetails, ClientDetails clientDetails, 
            EMinecraftKind kind = EMinecraftKind.VANILLA, string? gameDirectory = null, Resolution? resolution = null, 
            string? jvmArgs = "-XX:+UnlockExperimentalVMOptions -XX:+UseG1GC -XX:G1NewSizePercent=20 -XX:G1ReservePercent=20 -XX:MaxGCPauseMillis=50 -XX:G1HeapRegionSize=16M -Djava.net.preferIPv4Stack=true", 
            string? customVersion = null, IProgressReporter? progressReporter = null, bool isDebug = false) 
            : base(javaPath, minecraftVersion, memory, launcherDetails, clientDetails, kind, gameDirectory, resolution, jvmArgs, customVersion, progressReporter, isDebug)
        {

        }

        protected override async Task<ModdedData?> InstallModedAsync(string tempDir)
        {
            ReportProgress(0, "ui_reading_manifest", "fabricManifest");
            if (!File.Exists(PathHelper.FabricManifestPath))
            {
                _logger.Error("Fabric manifest file not found at path: " + PathHelper.FabricManifestPath);
                return null;
            }

            VersionDetails fabricVersion = GameHelper.GetVersionDetails(PathHelper.VersionsDir, this.MinecraftVersion.Id, EMinecraftKind.QUILT, this.VersionData.CustomVersion, this.VersionData.GameDir);

            // Create versionDir in the versions folder
            if (!Directory.Exists(fabricVersion.VersionDirectory))
                Directory.CreateDirectory(fabricVersion.VersionDirectory);

            // Check libsizes dir
            string librarySizeCacheDir = Path.Combine(PathHelper.CacheDir, "libsizes");
            if (!Directory.Exists(librarySizeCacheDir))
                Directory.CreateDirectory(librarySizeCacheDir);
            string librarySizeCachePath = Path.Combine(librarySizeCacheDir, $"{fabricVersion.MinecraftVersion}-fabric-{fabricVersion.CustomVersion}.json");

            // Download version json
            FabricVersionMeta? fabricVersionMeta;
            List<LibraryMeta> localLibraries = new List<LibraryMeta>();
            if (!File.Exists(fabricVersion.VersionJsonPath))
            {
                ReportProgress(0, $"ui_downloading_version_json", "fabric", 0);

                Progress<double> progress = new Progress<double>();
                progress.ProgressChanged += (sender, e) =>
                {
                    ReportProgress(e, "ui_downloading_version_json", "fabric", e.ToString("0.00"));
                };

                var resultJson = await HttpHelper.GetStringAsync(string.Format(FabricEndpoints.LoaderJsonUrl, fabricVersion.MinecraftVersion, fabricVersion.CustomVersion), progress);
                if (resultJson == null)
                    return null;
                
                await File.WriteAllTextAsync(fabricVersion.VersionJsonPath, resultJson);

                // Add the libraries
                fabricVersionMeta = JsonConvert.DeserializeObject<FabricVersionMeta>(resultJson);
                int localLibrarySize = 0;
                if (fabricVersionMeta == null)
                {
                    File.Delete(fabricVersion.VersionJsonPath); // Delete it because this if part won't be executed again if it exists
                    _logger.Error("Fabric version meta is null after deserialization. Invalid JSON format.");
                    return null;
                }

                ReportProgress(0, $"ui_reading_version_json", "fabric");
                foreach (var lib in fabricVersionMeta.Libraries)
                {
                    localLibrarySize += lib.Size;
                    localLibraries.Add(new LibraryMeta(lib.Name, new LibraryDownloads(new Artifact(lib.GetPath(), lib.Sha1, lib.Size, lib.GetURL()), null), new List<Rule>()));
                }
                // Save the version cache
                await JsonHelper.WriteJsonFileAsync(librarySizeCachePath, localLibrarySize);
            }
            else
            {
                ReportProgress(0, $"ui_reading_version_json", "fabric");
                fabricVersionMeta = JsonConvert.DeserializeObject<FabricVersionMeta>(await File.ReadAllTextAsync(fabricVersion.VersionJsonPath));
                if (fabricVersionMeta == null)
                {
                    _logger.Error("Fabric version meta is null after deserialization. Invalid JSON format.");
                    return null;
                }

                foreach (var lib in fabricVersionMeta.Libraries)
                {
                    localLibraries.Add(new LibraryMeta(lib.Name, new LibraryDownloads(new Artifact(lib.GetPath(), lib.Sha1, lib.Size, lib.GetURL()), null), new List<Rule>()));
                }
            }


            // Download Loader
            string loaderDirPath = Path.Combine(PathHelper.LibrariesDir, $"net\\fabricmc\\fabric-loader\\{fabricVersion.CustomVersion}");
            string loaderJarPath = Path.Combine(loaderDirPath, $"fabric-loader-{fabricVersion.CustomVersion}.jar");
            if (!Directory.Exists(loaderDirPath))
                Directory.CreateDirectory(loaderDirPath);

            if (!File.Exists(loaderJarPath))
            {
                ReportProgress(0, $"ui_downloading_loader", "fabric", 0);

                Progress<double> progress = new Progress<double>();
                progress.ProgressChanged += (_, e) =>
                {
                    ReportProgress(e, "ui_downloading_loader", "fabric", e.ToString("0.00"));
                };

                byte[]? bytes = await HttpHelper.GetByteArrayAsync(string.Format(FabricEndpoints.LoaderJarUrl, fabricVersion.CustomVersion), progress);
                if (bytes == null)
                    return null;
                
                await File.WriteAllBytesAsync(loaderJarPath, bytes);
            }

            ReportProgress(0, $"ui_getting_launch_arguments");
            if (!File.Exists(fabricVersion.VersionJarPath))
            {
                ReportProgress(0, $"ui_copying_jar", "vanilla");
                File.Copy(fabricVersion.VanillaJarPath, fabricVersion.VersionJarPath);
            }

            ModdedData moddedData = new ModdedData(fabricVersionMeta.MainClass, fabricVersion, localLibraries);

            foreach (var arg in fabricVersionMeta.Arguments.GetGameArgs())
                _gameArguments.Add(new LaunchArg(arg, 1));

            foreach (var arg in fabricVersionMeta.Arguments.GetJvmArgs())
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
