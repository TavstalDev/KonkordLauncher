using System.Diagnostics;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Installers.Forge;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;
using Tavstal.KonkordLauncher.Core.Services;

namespace Tavstal.KonkordLauncher.Core.Installers
{
    public class MinecraftInstaller
    {
        private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(MinecraftInstaller));
        private readonly MinecraftFileService _minecraftFileService;
        private readonly LauncherDetails _launcherDetails;
        private readonly ClientDetails _client;
        public string JavaPath { get; protected set; }
        public int Memory { get; protected set; }
        public Resolution? Resolution { get; protected set; }
        
        public EMinecraftKind Kind { get; protected set; }
        public string? UserJvmArgs { get; protected set; }
        public VersionDetails VersionData { get; }
        public VersionManifest VersionManifest { get; }
        public MinecraftVersion MinecraftVersion { get; }


        protected bool IsDebug { get; }
        protected IProgressReporter? _progressReporter { get; }


        protected VersionMeta MinecraftVersionMeta { get; private set; }
        protected string _classPath { get; set; }
        protected List<LaunchArg> _jvmArguments { get; set; }
        protected List<LaunchArg> _gameArguments { get; set; }
        protected List<LaunchArg> _jvmArgumentsBeforeClassPath { get; set; }

        public MinecraftInstaller(string javaPath, string minecraftVersion, int memory, LauncherDetails launcherDetails, ClientDetails clientDetails, 
            EMinecraftKind kind = EMinecraftKind.VANILLA, string? gameDirectory = null, Resolution? resolution = null, 
            string? jvmArgs = "-XX:+UnlockExperimentalVMOptions -XX:+UseG1GC -XX:G1NewSizePercent=20 -XX:G1ReservePercent=20 -XX:MaxGCPauseMillis=50 -XX:G1HeapRegionSize=16M -Djava.net.preferIPv4Stack=true", 
            string? customVersion = null, IProgressReporter? progressReporter = null, bool isDebug = false)
        {
            _jvmArguments = [];
            _gameArguments = [];
            _jvmArgumentsBeforeClassPath = [];
            _progressReporter = progressReporter;
            IsDebug = isDebug;
            _classPath = string.Empty; // Null fix

            _launcherDetails = launcherDetails;
            _client = clientDetails;
            
            JavaPath = javaPath;
            Memory = memory;
            Resolution = resolution;
            Kind = kind;
            UserJvmArgs = jvmArgs;

            VersionManifest? localManifest = JsonHelper.ReadJsonFile<VersionManifest>(PathHelper.VanillaManifestPath);
            if (localManifest == null)
            {
                _logger.Error("Failed to read the local vanilla manifest. Please ensure that the file exists and is valid.");
                return;
            }
            VersionManifest = localManifest;
            MinecraftVersion? mcVersion = VersionManifest.Versions.Find(x => x.Id == minecraftVersion);
            if (mcVersion == null)
            {
                _logger.Error("The specified Minecraft version does not exist in the manifest: " + minecraftVersion);
                return;
            }
            MinecraftVersion = mcVersion;
            
            // Get Version Data
            VersionData = GameHelper.GetVersionDetails(PathHelper.VersionsDir, minecraftVersion, kind, customVersion, gameDirectory);
            _minecraftFileService = new MinecraftFileService();
        }
        
        protected void ReportProgress(double percent, string translationKey, params object[]? args)
        {
            _progressReporter?.SetStatusTranslated(translationKey, args);
            _progressReporter?.SetProgress(percent);
        }
        
        public async Task<Process?> Install()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);

            if (!Directory.Exists(VersionData.VersionDirectory))
                Directory.CreateDirectory(VersionData.VersionDirectory);

            var localVersionMeta = await MinecraftFileService.DownloadVersionAsync(VersionData, MinecraftVersion, _progressReporter);
            if (localVersionMeta == null)
            {
                _logger.Error("Failed to download the version meta data. Please check your internet connection and try again.");
                return null;
            }
            MinecraftVersionMeta = localVersionMeta;
            await MinecraftFileService.DownloadMappingsAsync(MinecraftVersionMeta, VersionData, _progressReporter);
            await MinecraftFileService.DownloadAssetsAsync(MinecraftVersionMeta, _progressReporter);

            var modedData = await InstallModedAsync(tempDir);
            string arguments;
            List<LibraryMeta> libraries = MinecraftVersionMeta.Libraries;
            List<LibraryMeta> natives;
            if (modedData != null)
            {
                if (!Directory.Exists(modedData.VersionData.GameDir))
                    Directory.CreateDirectory(modedData.VersionData.GameDir);

                if (modedData.Libraries.Count > 0)
                    libraries.InsertRange(0, modedData.Libraries);

                var arg = await MinecraftFileService.DownloadLoggingAsync(MinecraftVersionMeta, modedData.VersionData.VersionDirectory, modedData.VersionData.GameDir, _progressReporter);
                if (arg != null)
                    _jvmArguments.Add(arg);
                
                var libResult = await MinecraftFileService.DownloadLibrariesAsync(Kind, VersionData, libraries, _classPath, _progressReporter);
                _classPath = libResult.Item1;
                natives = libResult.Item2;
                if (this.GetType() != typeof(ForgeInstNew))
                    _classPath += modedData.VersionData.VersionJarPath;
                arguments = await BuildArguments(modedData.VersionData.GameDir, modedData.MainClass, modedData.VersionData.NativesDir, modedData.VersionData.CustomVersion);
                
            }
            else
            {
                if (!Directory.Exists(VersionData.GameDir))
                    Directory.CreateDirectory(VersionData.GameDir);

                var arg = await MinecraftFileService.DownloadLoggingAsync(MinecraftVersionMeta, VersionData.VersionDirectory, VersionData.GameDir, _progressReporter);
                if (arg != null)
                    _jvmArguments.Add(arg);
                
                var libResult = await MinecraftFileService.DownloadLibrariesAsync(Kind, VersionData, libraries, _classPath, _progressReporter);
                _classPath = libResult.Item1;
                natives = libResult.Item2;
                _classPath += VersionData.VersionJarPath;
                arguments = await BuildArguments(VersionData.GameDir, MinecraftVersionMeta.MainClass, VersionData.NativesDir);
            }

            
            await MinecraftFileService.DownloadNatives(natives, modedData != null ? modedData.VersionData.NativesDir : VersionData.NativesDir, _progressReporter);
            FileSystemHelper.DeleteDirectory(tempDir);
            return JavaProcessLauncher.StartJava(JavaPath, arguments);
        }
        
        protected virtual Task<ModdedData?> InstallModedAsync(string tempDir)
        {
            // Vanilla installer, do nothing
            return Task.FromResult<ModdedData?>(null);
        }

        protected async Task<string> BuildArguments(string gameDir, string mainClass, string nativesDir, string? modVersion = null)
        {
            List<string> arguments = new List<string>();

            //UpdateProgressbarTranslated(0, $"ui_building_args");

            #region JVM

            arguments.Add("-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump");
            if (_jvmArgumentsBeforeClassPath.Count > 0)
                _jvmArgumentsBeforeClassPath.OrderByDescending(x => x.Priority).ToList()
                    .ForEach(a => { arguments.Add(a.Arg); });

            // Vanilla Args
            arguments.Add(MinecraftVersionMeta.GetJvmArgumentString());

            // Moded Args
            if (_jvmArguments.Count > 0)
                _jvmArguments.OrderByDescending(x => x.Priority).ToList()
                    .ForEach(a => { arguments.Add(a.Arg); });

            // Maximum Memory
            if (Memory is > 0 and <= 256)
                arguments.Add($"-Xmx{Memory * 1024}M");
            else if (Memory > 256)
                arguments.Add($"-Xmx{Memory}M");
            else
                arguments.Add($"-Xmx4G");

            // Minimum Memory
            arguments.Add($"-Xms256M");

            // The JVM args set by the user
            if (!string.IsNullOrEmpty(UserJvmArgs))
                arguments.Add($"{UserJvmArgs}");

            arguments.Add($"-Dminecraft.applet.TargetDirectory=\"{gameDir}\"");

            #endregion

            #region Minecraft Args

            // The main class
            arguments.Add(mainClass);
            // Vanilla Args
            arguments.Add(MinecraftVersionMeta.GetGameArgumentString());

            // Moded Args
            if (_gameArguments.Count > 0)
                _gameArguments.OrderByDescending(x => x.Priority).ToList()
                    .ForEach(a => { arguments.Add(a.Arg); });

            #endregion

            // Screen resolution
            if (Resolution != null)
            {
                if (Resolution.X > 0)
                    arguments.Add($"--width {Resolution.X}");

                if (Resolution.Y > 0)
                    arguments.Add($"--height {Resolution.Y}");
            }

            string versionName = string.Empty;
            switch (Kind)
            {
                case EMinecraftKind.VANILLA:
                {
                    versionName = VersionData.MinecraftVersion;
                    break;
                }
                case EMinecraftKind.FABRIC:
                {
                    versionName = $"fabric-loader-{modVersion}-{VersionData.MinecraftVersion}";
                    break;
                }
                case EMinecraftKind.QUILT:
                {
                    versionName = $"quilt-loader-{modVersion}-{VersionData.MinecraftVersion}";
                    break;
                }
                case EMinecraftKind.FORGE:
                {
                    versionName = $"forge-{modVersion}";
                    break;
                }
            }

            string argumentString = string.Join(' ', arguments);

            #region Replace Variable Placeholders

            argumentString = argumentString
                //.Replace("-Djava.library.path=${natives_directory}", "")
                .Replace("${natives_directory}", nativesDir)
                .Replace("${launcher_name}", _launcherDetails.LauncherName)
                .Replace("${launcher_version}", _launcherDetails.LauncherVersion)
                .Replace("${auth_player_name}", _client.DisplayName)
                .Replace("${version_name}", versionName)
                .Replace("${game_directory}", gameDir)
                .Replace("${assets_root}", PathHelper.AssetsDir)
                .Replace("${assets_index_name}", MinecraftVersionMeta.Index.Id)
                .Replace("${auth_uuid}", _client.UUID)
                .Replace("${auth_access_token}",
                    string.IsNullOrEmpty(_client.AccessToken) ? "none" : _client.AccessToken)
                .Replace("${clientid}", _client.ClientId)
                .Replace("${auth_xuid}", _client.Xuid)
                .Replace("${user_type}", "msa")
                .Replace("${version_type}", "release")
                .Replace("${classpath}", $"\"{_classPath}\"")
                .Replace("${library_directory}", PathHelper.LibrariesDir)
                .Replace("${classpath_separator}", ";")
                .Replace("${user_properties}", "{}");

            #endregion

            return argumentString;
        }
    }
}
