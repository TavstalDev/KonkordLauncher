using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Managers;
using Tavstal.KonkordLauncher.Core.Models.Forge.Installer;
using Tavstal.KonkordLauncher.Core.Models.Launcher;
using Tavstal.KonkordLauncher.Core.Models.Minecraft;
using Tavstal.KonkordLauncher.Core.Models.Minecraft.Library;

namespace Tavstal.KonkordLauncher.Core.Models.Installer
{
    public class MinecraftInstaller
    {
        // File jsons can be get through the manifest
        private static readonly string _mcVersionManifestUrl = "https://launchermeta.mojang.com/mc/game/version_manifest.json";
        public static string MCVerisonManifestUrl { get { return _mcVersionManifestUrl; } }

        public Profile Profile { get; }
        public VersionDetails VersionData { get; }
        public VersionManifest VersionManifest { get; }
        public MCVersion MinecraftVersion { get; }
        public MCVersionMeta MinecraftVersionMeta { get; private set; }
        public string JavaPath { get; internal set; }
        public bool IsDebug { get; }
        protected IProgressReporter _progressReporter { get; }
        protected string _classPath { get; set; }
        protected List<LaunchArg> _jvmArguments { get; set; }
        protected List<LaunchArg> _gameArguments { get; set; }
        protected List<LaunchArg> _jvmArgumentsBeforeClassPath { get; set; }

        public MinecraftInstaller() 
        {

        }

        public MinecraftInstaller(Profile profile, IProgressReporter progressReporter, bool isDebug)
        {
            _jvmArguments = [];
            _gameArguments = [];
            _jvmArgumentsBeforeClassPath = [];
            _progressReporter = progressReporter;
            IsDebug = isDebug;
            _classPath = string.Empty; // Null fix

            VersionManifest? localManifest = JsonHelper.ReadJsonFile<VersionManifest>(Path.Combine(IOHelper.ManifestDir, "vanillaManifest.json"));
            if (localManifest == null)
                throw new FileNotFoundException("Failed to get the vanillaManifest");

            VersionManifest = localManifest;
            Profile = profile;
            switch (profile.Type)
            {
                case EProfileType.CUSTOM:
                    {
                        VersionData = GameHelper.GetProfileVersionDetails(EProfileKind.VANILLA, profile.VersionVanillaId, profile.VersionVanillaId, null);
                        break;
                    }
                default:
                    {
                        VersionData = GameHelper.GetProfileVersionDetails(profile.Type, VersionManifest, profile);
                        break;
                    }
            }

            MCVersion? mcVersion = VersionManifest.Versions.Find(x => x.Id == VersionData.VanillaVersion);
            if (mcVersion == null)
                throw new Exception($"Failed to get the minecraft version for '{VersionData.VanillaVersion}'.");
            MinecraftVersion = mcVersion;

            

            JavaPath = string.IsNullOrEmpty(profile.JavaPath) ? (isDebug ? "java" : "javaw") : profile.JavaPath;
            if (!(JavaPath.EndsWith("java") || JavaPath.EndsWith("javaw") || JavaPath.EndsWith("java.exe") || JavaPath.EndsWith("javaw.exe")))
            {
                JavaPath = Path.Combine(JavaPath, isDebug ? "java.exe" : "javaw.exe");
            }

        }

        /// <summary>
        /// Updates the progress bar with the specified percentage and text.
        /// </summary>
        /// <param name="percent">The percentage value for the progress bar.</param>
        /// <param name="text">The text to display along with the progress bar.</param>
        internal void UpdateProgressbarTranslated(double percent, string text, params object[]? args)
        {
            _progressReporter.SetStatus(TranslationManager.Translate(text, args));
            _progressReporter.SetProgress(percent);
        }

        /// <summary>
        /// Initiates the Minecraft installation process asynchronously but does not wait for its completion.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="Process"/> representing the installation process.
        /// </returns>
        public async Task<Process> Install()
        {
            string tempDir = Path.Combine(IOHelper.TempDir, Path.GetRandomFileName());
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);

            if (!Directory.Exists(VersionData.VersionDirectory))
                Directory.CreateDirectory(VersionData.VersionDirectory);

            await DownloadVersion();
            await DownloadMappings();
            await DownloadAssets();

            var modedData = await InstallModed(tempDir);
            string arguments = string.Empty;
            List<MCLibrary> libraries = MinecraftVersionMeta.Libraries;
            List<MCLibrary> natives = new List<MCLibrary>();
            if (modedData != null)
            {
                if (!Directory.Exists(modedData.VersionData.GameDir))
                    Directory.CreateDirectory(modedData.VersionData.GameDir);

                if (modedData.Libraries.Count > 0)
                    libraries.InsertRange(0, modedData.Libraries);

                await DownloadLogging(modedData.VersionData.VersionDirectory, modedData.VersionData.GameDir); // 0
                natives = await DownloadLibraries(libraries); // 1
                if (Profile.Kind != EProfileKind.FORGE || (Profile.Kind == EProfileKind.FORGE && this.GetType() != typeof(ForgeInstNew)))
                    _classPath += modedData.VersionData.VersionJarPath; // 2
                arguments = await BuildArguments(modedData.VersionData.GameDir, modedData.MainClass, modedData.VersionData.NativesDir, modedData.VersionData.InstanceVersion); // 3
                
            }
            else
            {
                if (!Directory.Exists(VersionData.GameDir))
                    Directory.CreateDirectory(VersionData.GameDir);

                await DownloadLogging(VersionData.VersionDirectory, VersionData.GameDir); // 0
                natives = await DownloadLibraries(libraries); // 1
                _classPath += VersionData.VersionJarPath; // 2
                arguments = await BuildArguments(VersionData.GameDir, MinecraftVersionMeta.MainClass, VersionData.NativesDir); // 3
            }

            
            await DownloadNatives(natives, modedData != null ? modedData.VersionData.NativesDir : VersionData.NativesDir);
            IOHelper.DeleteDirectory(tempDir);
            return StartJava(arguments);
        }

        /// <summary>
        /// Asynchronously installs modded data using mod loaders such as Forge, Fabric, or Quilt.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="ModedData"/> representing the modded data, or null if installation fails.
        /// </returns>
        internal virtual async Task<ModedData?> InstallModed(string tempDir)
        {
            // Vanilla installer, do nothing
            await Task.Delay(1);
            return default;
        }

        /// <summary>
        /// Asynchronously downloads the Minecraft version JSON and JAR files.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        private async Task DownloadVersion()
        {
            // JSON
            if (!File.Exists(VersionData.VersionJsonPath))
            {
                UpdateProgressbarTranslated(0, $"ui_downloading_version_json", new object[] { MinecraftVersion.Id, 0 });

                Progress<double> progress = new Progress<double>();
                progress.ProgressChanged += (sender, e) =>
                {
                    UpdateProgressbarTranslated(e, "ui_downloading_version_json", new object[] { MinecraftVersion.Id, e.ToString("0.00") });
                };
                string? jsonResult = await HttpHelper.GetStringAsync(MinecraftVersion.Url, progress);
                if (jsonResult == null)
                    return;

                MinecraftVersionMeta = JsonConvert.DeserializeObject<MCVersionMeta>(jsonResult);
                await File.WriteAllTextAsync(VersionData.VersionJsonPath, jsonResult);
            }
            else
            {
                UpdateProgressbarTranslated(0, $"ui_reading_version_json", new object[] { MinecraftVersion.Id });
                string jsonResult = await File.ReadAllTextAsync(VersionData.VersionJsonPath);
                MinecraftVersionMeta = JsonConvert.DeserializeObject<MCVersionMeta>(jsonResult);
            }

            if (MinecraftVersionMeta == null)
                throw new NullReferenceException($"Failed to get the minecraft version meta.");

            // JAR
            if (!File.Exists(VersionData.VersionJarPath))
            {
                UpdateProgressbarTranslated(0, $"ui_downloading_version_jar", new object[] { MinecraftVersion.Id, 0 });

                Progress<double> progress = new Progress<double>();
                progress.ProgressChanged += (sender, e) =>
                {
                    UpdateProgressbarTranslated(e, "ui_downloading_version_jar", new object[] { MinecraftVersion.Id, e.ToString("0.00") });
                };

                byte[]? bytes = await HttpHelper.GetByteArrayAsync(MinecraftVersionMeta.Downloads.Client.Url, progress);
                if (bytes == null)
                    return;
                await File.WriteAllBytesAsync(VersionData.VersionJarPath, bytes);
            }
        }

        /// <summary>
        /// Asynchronously downloads the Minecraft assets.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        private async Task DownloadAssets()
        {
            // AssetIndex
            UpdateProgressbarTranslated(0, $"ui_checking_asset_index_json", new object[] { MinecraftVersionMeta.AssetIndex.Id });
            string assetIndex = MinecraftVersionMeta.AssetIndex.Id;
            string assetPath = Path.Combine(IOHelper.AssetsDir, $"indexes/{assetIndex}.json");
            JToken? assetJToken = null;
            if (!File.Exists(assetPath))
            {
                UpdateProgressbarTranslated(0, $"ui_downloading_asset_index_json", new object[] { assetIndex, 0 });

                Progress<double> progress = new Progress<double>();
                progress.ProgressChanged += (sender, e) =>
                {
                    UpdateProgressbarTranslated(e, "ui_downloading_asset_index_json", new object[] { assetIndex, e.ToString("0.00") });
                };

                string? resultJson = await HttpHelper.GetStringAsync(MinecraftVersionMeta.AssetIndex.Url, progress);
                if (resultJson == null)
                    return;

                assetJToken = JObject.Parse(resultJson)["objects"];
                await File.WriteAllTextAsync(assetPath, resultJson);
            }
            else
            {
                UpdateProgressbarTranslated(0, $"ui_reading_asset_index_json", new object[] { assetIndex });
                string resultJson = await File.ReadAllTextAsync(assetPath);
                assetJToken = JObject.Parse(resultJson)["objects"];
            }


            if (assetJToken == null)
            {
                NotificationHelper.SendErrorTranslated("json_token_not_found", "messagebox_error", new object[] { "assetJToken (mc asset objects)" });
                return;
            }

            // Asset Dir
            string assetObjectDir = Path.Combine(IOHelper.AssetsDir, "objects");
            if (!Directory.Exists(assetObjectDir))
                Directory.CreateDirectory(assetObjectDir);

            // Assets

            string hash = string.Empty;
            string objectDir = string.Empty;
            string objectPath = string.Empty;
            int downloadedAssetSize = 0;
            UpdateProgressbarTranslated(0, $"ui_checking_assets");
            foreach (JToken token in assetJToken.ToList())
            {
                hash = token.First["hash"].ToString();
                objectDir = Path.Combine(assetObjectDir, hash.Substring(0, 2));
                objectPath = Path.Combine(objectDir, $"{hash}");

                if (!Directory.Exists(objectDir))
                    Directory.CreateDirectory(objectDir);

                if (!File.Exists(objectPath))
                {
                    byte[]? array = await HttpHelper.GetByteArrayAsync($"https://resources.download.minecraft.net/{hash.Substring(0, 2)}/{hash}");
                    if (array == null)
                        return;

                    await File.WriteAllBytesAsync(objectPath, array);
                }
                downloadedAssetSize += int.Parse(token.First["size"].ToString());
                double percent = (double)downloadedAssetSize / (double)MinecraftVersionMeta.AssetIndex.TotalSize * 100d;
                UpdateProgressbarTranslated(percent, $"ui_downloading_assets", new object[] { percent.ToString("0.00") });
            }
        }

        /// <summary>
        /// Asynchronously downloads the logging configuration file if present in the MinecraftVersionMeta and adds its path to the arguments.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        private async Task DownloadLogging(string versionDirectory, string gameDir)
        {
            if (MinecraftVersionMeta.Logging != null && MinecraftVersionMeta.Logging.Client != null)
            {
                UpdateProgressbarTranslated(0, $"ui_checking_logging");
                string logDirPath = Path.Combine(IOHelper.AssetsDir, "log_configs");
                if (!Directory.Exists(logDirPath))
                    Directory.CreateDirectory(logDirPath);
                string logReadmePath = Path.Combine(logDirPath, "readme.txt");
                if (!File.Exists(logReadmePath))
                    await File.WriteAllTextAsync(logReadmePath, $"The log config files has been moved to {IOHelper.VersionsDir}\\<your_version> so the logs can be made per instance.");
                string logFilePath = Path.Combine(versionDirectory, MinecraftVersionMeta.Logging.Client.File.Id);
                if (!File.Exists(logFilePath))
                {
                    UpdateProgressbarTranslated(0, $"ui_downloading_logging", new object[] { 0 });

                    Progress<double> progress = new Progress<double>();
                    progress.ProgressChanged += (sender, e) =>
                    {
                        UpdateProgressbarTranslated(e, "ui_downloading_logging", new object[] { e.ToString("0.00") });
                    };

                    string? r = await HttpHelper.GetStringAsync(MinecraftVersionMeta.Logging.Client.File.Url, progress);
                    if (r == null)
                        return;

                    // FIX LOG LOCATION
                    r = r.Replace("fileName=\"logs", $"fileName=\"{gameDir}\\logs").Replace("filePattern=\"logs", $"filePattern=\"{gameDir}\\logs");

                    await File.WriteAllTextAsync(logFilePath, r);
                }
                _jvmArguments.Add(new LaunchArg(MinecraftVersionMeta.Logging.Client.Argument.Replace("${path}", logFilePath), 0));
            }
        }

        /// <summary>
        /// Asynchronously downloads the mappings for the Minecraft version.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        private async Task DownloadMappings()
        {
            UpdateProgressbarTranslated(0, $"ui_checking_client_mappings");
            if (MinecraftVersionMeta.Downloads.ClientMappings == null)
                return;

            string clientMappinsPath = Path.Combine(VersionData.VersionDirectory, "client.txt");
            if (!File.Exists(clientMappinsPath))
            {
                UpdateProgressbarTranslated(0, $"ui_downloading_client_mappings", new object[] { 0 });

                Progress<double> progress = new Progress<double>();
                progress.ProgressChanged += (sender, e) =>
                {
                    UpdateProgressbarTranslated(e, "ui_downloading_client_mappings", new object[] { e.ToString("0.00") });
                };

                string? r = await HttpHelper.GetStringAsync(MinecraftVersionMeta.Downloads.ClientMappings.Url, progress);
                if (r == null)
                    return;

                await File.WriteAllTextAsync(clientMappinsPath, r);
            }
        }

        /// <summary>
        /// Asynchronously downloads Minecraft and mod loader libraries.
        /// </summary>
        /// <param name="mcLibs">The list of Minecraft libraries to download.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a list of downloaded <see cref="MCLibrary"/> objects.
        /// </returns>
        private async Task<List<MCLibrary>> DownloadLibraries(List<MCLibrary> mcLibs)
        {
            UpdateProgressbarTranslated(0, $"ui_checking_libraries");
            List<MCLibrary> natives = new List<MCLibrary>();
            double libraryOverallSize = 0;
            double libraryDownloadedSize = 0;
            string libraryCacheDir = Path.Combine(IOHelper.CacheDir, "libsizes");
            if (!Directory.Exists(libraryCacheDir))
                Directory.CreateDirectory(libraryCacheDir);
            string librarySizeCacheFilePath = Path.Combine(libraryCacheDir, $"{VersionData.VanillaVersion}-{Profile.Kind}-{VersionData.InstanceVersion}.json");


            // Calculate the overallSize or read it from cache
            UpdateProgressbarTranslated(0, $"ui_calculating_lib_size");
            if (!File.Exists(librarySizeCacheFilePath))
            {
                foreach (var lib in mcLibs)
                {
                    // Check the library rule
                    if (!lib.GetRulesResult())
                        continue;

                    if (lib.Downloads.Artifact == null)
                        continue;

                    libraryOverallSize += lib.Downloads.Artifact.Size;
                }

                await File.WriteAllTextAsync(librarySizeCacheFilePath, libraryOverallSize.ToString());
            }
            else
                libraryOverallSize = int.Parse(await File.ReadAllTextAsync(librarySizeCacheFilePath));

            // Download the actual libs
            foreach (var lib in mcLibs)
            {
                // Check the library rule
                if (!lib.GetRulesResult())
                    continue;

                string libFilePath = string.Empty;
                if (lib.Downloads.Artifact != null)
                {
                    string localPath = lib.Downloads.Artifact.Path.Replace('/', '\\');
                    int libDirIndex = localPath.LastIndexOf('\\');
                    string libDirPath = Path.Combine(IOHelper.LibrariesDir, localPath.Remove(libDirIndex, localPath.Length - libDirIndex));

                    if (!Directory.Exists(libDirPath))
                        Directory.CreateDirectory(libDirPath);

                    libFilePath = Path.Combine(IOHelper.LibrariesDir, localPath);
                    if (!File.Exists(libFilePath))
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(lib.Downloads.Artifact.Url))
                            {
                                Progress<double> progress = new Progress<double>();
                                progress.ProgressChanged += (sender, e) =>
                                {
                                    UpdateProgressbarTranslated(e, "ui_library_download", new object[] { lib.Name, e.ToString("0.00") });
                                };

                                byte[]? bytes = await HttpHelper.GetByteArrayAsync(lib.Downloads.Artifact.Url, progress);
                                if (bytes == null)
                                    continue;

                                await File.WriteAllBytesAsync(libFilePath, bytes);
                            }
                            else
                                libraryDownloadedSize -= lib.Downloads.Artifact.Size; // Fix 100%+ bug
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine($"Failed to get file: {lib.Downloads.Artifact.Url}");
                            Debug.WriteLine($"Error: {e}");
                        }
                    }


                    //libraryDownloadedSize += lib.Downloads.Artifact.Size;
                    //if (libraryOverallSize == 0)
                        //libraryOverallSize = 1; // NaN fix
                    //double percent = libraryDownloadedSize / libraryOverallSize * 100;
                    //UpdateProgressbarTranslated(percent, $"ui_library_download", new object[] { lib.Name, percent.ToString("0.00") });
                }
                else if (lib.Downloads.Classifiers != null)
                {
                    string[] rawUrl = lib.Name.Split(':');
                    string localPath = Path.Combine(rawUrl[0].Replace('.', '\\'), rawUrl[1], rawUrl[2], $"{rawUrl[1]}-{rawUrl[2]}.jar").Replace('/', '\\');
                    int libDirIndex = localPath.LastIndexOf('\\');
                    string libDirPath = Path.Combine(IOHelper.LibrariesDir, localPath.Remove(libDirIndex, localPath.Length - libDirIndex));

                    if (!Directory.Exists(libDirPath))
                        Directory.CreateDirectory(libDirPath);

                    libFilePath = Path.Combine(IOHelper.LibrariesDir, localPath);
                }

                if (!_classPath.Contains(libFilePath))
                {
                    _classPath += $"{libFilePath};";
                    if (lib.Name.StartsWith("org.lwjgl") || lib.Downloads.Classifiers != null)
                    {
                        natives.Add(lib);
                    }
                }
                else
                {
                    if (lib.Name.StartsWith("org.lwjgl") || lib.Downloads.Classifiers != null)
                    {
                        natives.Add(lib);
                    }
                }
            }

            return natives;
        }

        /// <summary>
        /// Asynchronously downloads native libraries.
        /// </summary>
        /// <param name="nativeLibs">The list of native libraries to download.</param>
        /// <param name="nativeDir">The directory to download the native libraries to.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        private async Task DownloadNatives(List<MCLibrary> nativeLibs, string nativeDir)
        {
            UpdateProgressbarTranslated(0, $"ui_checking_natives");
            if (!Directory.Exists(nativeDir))
                Directory.CreateDirectory(nativeDir);

            string libJarFilePath = string.Empty;
            string libJarFileDir = string.Empty;
            foreach (MCLibrary lib in nativeLibs)
            {
                Debug.WriteLine("nativeLib: " + lib.Name);
                if (lib.Downloads.Classifiers != null)
                {
                    string localUrl = string.Empty;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        if (lib.Downloads.Classifiers.WindowsNatives == null)
                            continue;

                        localUrl = lib.Downloads.Classifiers.WindowsNatives.Url;
                        libJarFilePath = lib.Downloads.Classifiers.WindowsNatives.Path;
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        if (lib.Downloads.Classifiers.LinuxNatives == null)
                            continue;

                        localUrl = lib.Downloads.Classifiers.LinuxNatives.Url;
                        libJarFilePath = lib.Downloads.Classifiers.LinuxNatives.Path;
                    }
                    else // OSX
                    {
                        if (lib.Downloads.Classifiers.OsxNatives == null)
                            continue;

                        localUrl = lib.Downloads.Classifiers.OsxNatives.Url;
                        libJarFilePath = lib.Downloads.Classifiers.OsxNatives.Path;
                    }

                    // Add dir
                    string localFilePath = Path.Combine(IOHelper.LibrariesDir, libJarFilePath);

                    if (!File.Exists(localFilePath))
                    {
                        string libJarDir = localFilePath.Remove(localFilePath.LastIndexOf('/'), localFilePath.Length - localFilePath.LastIndexOf('/'));
                        if (!Directory.Exists(libJarDir))
                            Directory.CreateDirectory(libJarDir);

                        Progress<double> progress = new Progress<double>();
                        progress.ProgressChanged += (sender, e) =>
                        {
                            UpdateProgressbarTranslated(e, "ui_library_download", new object[] { lib.Name, e.ToString("0.00") });
                        };

                        byte[]? bytes = await HttpHelper.GetByteArrayAsync(localUrl, progress);
                        if (bytes == null)
                            continue;

                        await File.WriteAllBytesAsync(localFilePath, bytes);
                    }
                }
                else
                    continue;

                string libFilePath = Path.Combine(IOHelper.LibrariesDir, libJarFilePath);
                List <string> dirBaseRaw = libJarFilePath.Split('.').ToList();
                dirBaseRaw.RemoveAt(dirBaseRaw.Count - 1);

                string dirRaw = string.Empty;
                foreach (var ba in dirBaseRaw)
                {
                    if (string.IsNullOrEmpty(dirRaw))
                        dirRaw += ba;
                    else
                        dirRaw += $".{ba}";
                }
                string tempZipDir = Path.Combine(IOHelper.TempDir, dirRaw);
                ZipFile.ExtractToDirectory(libFilePath, tempZipDir, true);

                string[] files = Directory.GetFiles(tempZipDir, "*.dll", searchOption: SearchOption.AllDirectories);
                if (files != null)
                {
                    foreach (string file in files)
                    {
                        if (Environment.Is64BitOperatingSystem && file.Contains("32"))
                            continue;
                        else if (!Environment.Is64BitOperatingSystem && !file.Contains("32"))
                            continue;

                        string fileName = file.Remove(0, file.LastIndexOf('\\') + 1);
                        string filePath = Path.Combine(nativeDir, fileName);
                        if (!File.Exists(filePath))
                            File.Move(file, filePath);
                    }
                }

                IOHelper.DeleteDirectory(tempZipDir);
            }
            await Task.Delay(1); // temporal
        }

        /// <summary>
        /// Asynchronously builds the launch arguments for Minecraft.
        /// </summary>
        /// <param name="gameDir">The game directory.</param>
        /// <param name="mainClass">The main class to launch.</param>
        /// <param name="modVersion">Optional: The mod version to include in the launch arguments.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the built launch arguments as a string.
        /// </returns>
        private async Task<string> BuildArguments(string gameDir, string mainClass, string nativesDir, string? modVersion = null)
        {
            List<string> arguments = new List<string>();

            UpdateProgressbarTranslated(0, $"ui_building_args");
            #region JVM
            arguments.Add("-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump");
            if (_jvmArgumentsBeforeClassPath.Count > 0)
                _jvmArgumentsBeforeClassPath.OrderByDescending(x => x.Priority).ToList().ForEach((LaunchArg a) => {
                    arguments.Add(a.Arg);
                });

            // Vanilla Args
            arguments.Add(MinecraftVersionMeta.GetJVMArgumentString());

            // Moded Args
            if (_jvmArguments.Count > 0)
                _jvmArguments.OrderByDescending(x => x.Priority).ToList().ForEach((LaunchArg a) => {
                    arguments.Add(a.Arg);
                });

            // Maximum Memory
            if (Profile.Memory > 0 && Profile.Memory <= 256)
                arguments.Add($"-Xmx{Profile.Memory * 1024}M");
            else if (Profile.Memory > 256)
                arguments.Add($"-Xmx{Profile.Memory}M");
            else
                arguments.Add($"-Xmx4G");
            // Minimum Memory
            // If someone breaks it by setting the maximum memory lower than 256MB... do not hit them
            arguments.Add($"-Xms256M");

            // The JVM args set by the user
            if (!string.IsNullOrEmpty(Profile.JVMArgs))
                arguments.Add($"{Profile.JVMArgs}");

            arguments.Add($"-Dminecraft.applet.TargetDirectory=\"{gameDir}\"");
            #endregion
            #region Minecraft Args
            // The main class
            arguments.Add(mainClass);
            // Vanilla Args
            arguments.Add(MinecraftVersionMeta.GetGameArgumentString());

            // Moded Args
            if (_gameArguments.Count > 0)
                _gameArguments.OrderByDescending(x => x.Priority).ToList().ForEach((LaunchArg a) => {
                    arguments.Add(a.Arg);
                });
            #endregion

            // Screen resolution
            if (Profile.Resolution != null)
            {
                if (Profile.Resolution.IsFullScreen)
                {
                    arguments.Add($"--width {(int)SystemParameters.PrimaryScreenWidth}");
                    arguments.Add($"--height {(int)SystemParameters.PrimaryScreenHeight}");
                }
                else
                {
                    if (Profile.Resolution.X > 0)
                        arguments.Add($"--width {Profile.Resolution.X}");
                    else
                        arguments.Add($"--width {(int)(SystemParameters.PrimaryScreenWidth * 0.44)}");

                    if (Profile.Resolution.Y > 0)
                        arguments.Add($"--height {Profile.Resolution.Y}");
                    else
                        arguments.Add($"--height {(int)(SystemParameters.PrimaryScreenHeight * 0.44)}");
                }
            }
            else
            {
                arguments.Add($"--width {(int)(SystemParameters.PrimaryScreenWidth * 0.44)}");
                arguments.Add($"--height {(int)(SystemParameters.PrimaryScreenHeight * 0.44)}");
            }

            AccountData? accountData = await JsonHelper.ReadJsonFileAsync<AccountData>(Path.Combine(IOHelper.MainDirectory, "accounts.json"));
            if (accountData == null)
                throw new Exception("Could not launch the game because failed to get the account details.");

            Account account = accountData.Accounts[accountData.SelectedAccountId];
            if (account == null)
                throw new Exception("Could not launch the game because failed to get the current account.");

            string clientId = "0";
            string xUID = "0";

            string versionName = string.Empty;
            switch (Profile.Kind)
            {
                case EProfileKind.VANILLA:
                    {
                        versionName = VersionData.VanillaVersion;
                        break;
                    }
                case EProfileKind.FABRIC:
                    {
                        versionName = $"fabric-loader-{modVersion}-{VersionData.VanillaVersion}";
                        break;
                    }
                case EProfileKind.QUILT:
                    {
                        versionName = $"quilt-loader-{modVersion}-{VersionData.VanillaVersion}";
                        break;
                    }
                case EProfileKind.FORGE:
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
                .Replace("${launcher_name}", "konkord-launcher")
                .Replace("${launcher_version}", "release")
                .Replace("${auth_player_name}", account.DisplayName)
                .Replace("${version_name}", versionName)
                .Replace("${game_directory}", gameDir)
                .Replace("${assets_root}", IOHelper.AssetsDir)
                .Replace("${assets_index_name}", MinecraftVersionMeta.AssetIndex.Id)
                .Replace("${auth_uuid}", account.UUID)
                .Replace("${auth_access_token}", string.IsNullOrEmpty(account.AccessToken) ? "none" : account.AccessToken)
                .Replace("${clientid}", clientId)
                .Replace("${auth_xuid}", xUID)
                .Replace("${user_type}", "msa")
                .Replace("${version_type}", "release")
                .Replace("${classpath}", $"\"{_classPath}\"")
                .Replace("${library_directory}", IOHelper.LibrariesDir)
                .Replace("${classpath_separator}", ";")
                .Replace("${user_properties}", "{}");
            #endregion

            return argumentString;
        }

        /// <summary>
        /// Starts the Java process with the specified arguments.
        /// </summary>
        /// <param name="arguments">The arguments to pass to the Java process.</param>
        /// <returns>
        /// A <see cref="Process"/> representing the started Java process.
        /// </returns>
        private Process StartJava(string arguments)
        {
            var psi = new ProcessStartInfo()
            {
                FileName = JavaPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardError = true,
            };

            Debug.WriteLine(arguments.Replace(' ', '\n'));
            Process? p = Process.Start(psi);
            return p;
        }
    }
}
