using KonkordLibrary.Enums;
using KonkordLibrary.Helpers;
using KonkordLibrary.Models.GameManager;
using KonkordLibrary.Models.Minecraft;
using KonkordLibrary.Models.Minecraft.Library;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace KonkordLibrary.Models.Installer
{
    public class MinecraftInstaller
    {
        // File jsons can be get throught the manifest
        private static readonly string _mcVersionManifestUrl = "https://launchermeta.mojang.com/mc/game/version_manifest.json";
        public static string MCVerisonManifestUrl { get { return _mcVersionManifestUrl; } }

        public Profile Profile { get; }
        public VersionResponse VersionData { get; }
        public VersionManifest VersionManifest { get; }
        public MCVersion MinecraftVersion { get; }
        public MCVersionMeta MinecraftVersionMeta { get; private set; }
        public string JavaPath { get; }
        public Label ProgressbarLabel { get; }
        public ProgressBar ProgressBar { get; }
        public bool IsDebug { get; }
        internal string _classPath { get; set; }
        internal List<LaunchArg> _jvmArguments { get; set; }
        internal List<LaunchArg> _gameArguments { get; set; }
        internal List<LaunchArg> _jvmArgumentsBeforeClassPath { get; set; }

        public MinecraftInstaller(Profile profile, Label label, ProgressBar progressBar, bool isDebug)
        {
            _jvmArguments = new List<LaunchArg>();
            _gameArguments = new List<LaunchArg>();
            _jvmArgumentsBeforeClassPath = new List<LaunchArg>();
            ProgressbarLabel = label;
            ProgressBar = progressBar;
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
                        VersionData = Managers.GameManager.GetProfileVersionDetails(EProfileKind.VANILLA, profile.VersionVanillaId, profile.VersionVanillaId, null);
                        break;
                    }
                default:
                    {
                        VersionData = Managers.GameManager.GetProfileVersionDetails(profile.Type, VersionManifest, profile);
                        break;
                    }
            }

            MCVersion? mcVersion = VersionManifest.Versions.Find(x => x.Id == VersionData.VanillaVersion);
            if (mcVersion == null)
                throw new Exception($"Failed to get the minecraft version for '{VersionData.VanillaVersion}'.");
            MinecraftVersion = mcVersion;

            JavaPath = string.IsNullOrEmpty(profile.JavaPath) ? (isDebug ? "java" : "javaw") : profile.JavaPath;

        }

        /// <summary>
        /// Updates the progress bar with the specified percentage and text.
        /// </summary>
        /// <param name="percent">The percentage value for the progress bar.</param>
        /// <param name="text">The text to display along with the progress bar.</param>
        internal void UpdateProgressbar(double percent, string text)
        {
            ProgressbarLabel.Content = text;
            ProgressBar.Value = percent > ProgressBar.Maximum ? ProgressBar.Maximum : percent;
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
                if (Profile.Kind != EProfileKind.FORGE)
                    _classPath += modedData.VersionData.VersionJarPath; // 2
                arguments = await BuildArguments(modedData.VersionData.GameDir, modedData.MainClass, modedData.VersionData.InstanceVersion); // 3
                
            }
            else
            {
                if (!Directory.Exists(VersionData.GameDir))
                    Directory.CreateDirectory(VersionData.GameDir);

                await DownloadLogging(VersionData.VersionDirectory, VersionData.GameDir); // 0
                natives = await DownloadLibraries(libraries); // 1
                _classPath += VersionData.VersionJarPath; // 2
                arguments = await BuildArguments(VersionData.GameDir, MinecraftVersionMeta.MainClass); // 3
                
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
                UpdateProgressbar(0, $"Downloading the '{MinecraftVersion.Id}' version json file...");
                using (HttpClient client = new HttpClient())
                {
                    string jsonResult = await client.GetStringAsync(MinecraftVersion.Url);

                    MinecraftVersionMeta = JsonConvert.DeserializeObject<MCVersionMeta>(jsonResult);
                    await File.WriteAllTextAsync(VersionData.VersionJsonPath, jsonResult);
                }
            }
            else
            {
                UpdateProgressbar(0, $"Reading the '{MinecraftVersion.Id}' version json file...");
                string jsonResult = await File.ReadAllTextAsync(VersionData.VersionJsonPath);
                MinecraftVersionMeta = JsonConvert.DeserializeObject<MCVersionMeta>(jsonResult);
            }

            if (MinecraftVersionMeta == null)
                throw new NullReferenceException($"Failed to get the minecraft version meta.");

            // JAR
            if (!File.Exists(VersionData.VersionJarPath))
            {
                UpdateProgressbar(0, $"Downloading the '{MinecraftVersion.Id}' version jar file...");
                using (HttpClient client = new HttpClient())
                {
                    byte[] bytes = await client.GetByteArrayAsync(MinecraftVersionMeta.Downloads.Client.Url);
                    await File.WriteAllBytesAsync(VersionData.VersionJarPath, bytes);
                }
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
            UpdateProgressbar(0, $"Checking the '{MinecraftVersionMeta.AssetIndex.Id}' assetIndex json file...");
            string assetIndex = MinecraftVersionMeta.AssetIndex.Id;
            string assetPath = Path.Combine(IOHelper.AssetsDir, $"indexes/{assetIndex}.json");
            JToken? assetJToken = null;
            if (!File.Exists(assetPath))
            {
                UpdateProgressbar(0, $"Downloading the '{assetIndex}' assetIndex json file...");
                using (HttpClient client = new HttpClient())
                {
                    string resultJson = await client.GetStringAsync(MinecraftVersionMeta.AssetIndex.Url);
                    assetJToken = JObject.Parse(resultJson)["objects"];
                    await File.WriteAllTextAsync(assetPath, resultJson);
                }
            }
            else
            {
                UpdateProgressbar(0, $"Reading the '{assetIndex}' assetIndex json file...");
                string resultJson = await File.ReadAllTextAsync(assetPath);
                assetJToken = JObject.Parse(resultJson)["objects"];
            }


            if (assetJToken == null)
            {
                NotificationHelper.SendError("Failed to get the assetJToken", "Error");
                return;
            }

            // Asset Dir
            string assetObjectDir = Path.Combine(IOHelper.AssetsDir, "objects");
            if (!Directory.Exists(assetObjectDir))
                Directory.CreateDirectory(assetObjectDir);

            // Assets
            using (HttpClient client = new HttpClient())
            {
                string hash = string.Empty;
                string objectDir = string.Empty;
                string objectPath = string.Empty;
                int downloadedAssetSize = 0;
                UpdateProgressbar(0, $"Checking assets...");
                foreach (JToken token in assetJToken.ToList())
                {
                    hash = token.First["hash"].ToString();
                    objectDir = Path.Combine(assetObjectDir, hash.Substring(0, 2));
                    objectPath = Path.Combine(objectDir, $"{hash}");

                    if (!Directory.Exists(objectDir))
                        Directory.CreateDirectory(objectDir);

                    if (!File.Exists(objectPath))
                    {
                        byte[] array = await client.GetByteArrayAsync($"https://resources.download.minecraft.net/{hash.Substring(0, 2)}/{hash}");
                        await File.WriteAllBytesAsync(objectPath, array);
                    }
                    downloadedAssetSize += int.Parse(token.First["size"].ToString());
                    double percent = (double)downloadedAssetSize / (double)MinecraftVersionMeta.AssetIndex.TotalSize * 100d;
                    UpdateProgressbar(percent, $"Downloading assets... {percent:0.00}%");
                }
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
                UpdateProgressbar(0, $"Checking logging file...");
                string logDirPath = Path.Combine(IOHelper.AssetsDir, "log_configs");
                if (!Directory.Exists(logDirPath))
                    Directory.CreateDirectory(logDirPath);
                string logReadmePath = Path.Combine(logDirPath, "readme.txt");
                if (!File.Exists(logReadmePath))
                    await File.WriteAllTextAsync(logReadmePath, $"The log config files has been moved to {IOHelper.VersionsDir}\\<your_version> so the logs can be made per instance.");
                string logFilePath = Path.Combine(versionDirectory, MinecraftVersionMeta.Logging.Client.File.Id);
                if (!File.Exists(logFilePath))
                {
                    UpdateProgressbar(0, $"Downloading logging file...");
                    using (HttpClient client = new HttpClient())
                    {
                        string r = await client.GetStringAsync(MinecraftVersionMeta.Logging.Client.File.Url);

                        // FIX LOG LOCATION
                        r = r.Replace("fileName=\"logs", $"fileName=\"{gameDir}\\logs").Replace("filePattern=\"logs", $"filePattern=\"{gameDir}\\logs");

                        await File.WriteAllTextAsync(logFilePath, r);
                    }
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
            UpdateProgressbar(0, $"Checking client mappings...");
            string clientMappinsPath = Path.Combine(VersionData.VersionDirectory, "client.txt");
            if (!File.Exists(clientMappinsPath))
            {
                UpdateProgressbar(0, $"Downloading client mappings...");
                using (HttpClient client = new HttpClient())
                {
                    string r = await client.GetStringAsync(MinecraftVersionMeta.Downloads.ClientMappings.Url);
                    await File.WriteAllTextAsync(clientMappinsPath, r);
                }
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
            UpdateProgressbar(0, $"Checking the libraries...");
            List<MCLibrary> natives = new List<MCLibrary>();
            double libraryOverallSize = 0;
            double libraryDownloadedSize = 0;
            string libraryCacheDir = Path.Combine(IOHelper.CacheDir, "libsizes");
            if (!Directory.Exists(libraryCacheDir))
                Directory.CreateDirectory(libraryCacheDir);
            string librarySizeCacheFilePath = Path.Combine(libraryCacheDir, $"{VersionData.VanillaVersion}-{Profile.Kind}-{VersionData.InstanceVersion}.json");

            using (HttpClient client = new HttpClient())
            {
                // Calculate the overallSize or read it from cache
                UpdateProgressbar(0, $"Calculating library sizes...");
                if (!File.Exists(librarySizeCacheFilePath))
                {
                    foreach (var lib in mcLibs)
                    {
                        // Check the library rule
                        if (lib.Rules != null)
                            if (lib.Rules.Count > 0)
                            {
                                bool action = lib.Rules[0].Action == "allow";
                                if (lib.Rules[0].OS != null)
                                {
                                    switch (lib.Rules[0].OS.Name)
                                    {
                                        case "osx": // lib requies machintosh
                                            {
                                                if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && action)
                                                {
                                                    continue;
                                                }
                                                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && !action)
                                                {
                                                    continue;
                                                }
                                                break;
                                            }
                                        case "linux":
                                            {
                                                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && action)
                                                {
                                                    continue;
                                                }
                                                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !action)
                                                {
                                                    continue;
                                                }
                                                break;
                                            }
                                        case "windows":
                                            {
                                                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && action)
                                                {
                                                    continue;
                                                }
                                                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !action)
                                                {
                                                    continue;
                                                }
                                                break;
                                            }
                                    }
                                }
                            }

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
                    if (lib.Rules != null)
                        if (lib.Rules.Count > 0)
                        {
                            bool action = lib.Rules[0].Action == "allow";
                            if (lib.Rules[0].OS != null)
                            {
                                switch (lib.Rules[0].OS.Name)
                                {
                                    case "osx": // lib requies machintosh
                                        {
                                            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && action)
                                            {
                                                continue;
                                            }
                                            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && !action)
                                            {
                                                continue;
                                            }
                                            break;
                                        }
                                    case "linux":
                                        {
                                            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && action)
                                            {
                                                continue;
                                            }
                                            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !action)
                                            {
                                                continue;
                                            }
                                            break;
                                        }
                                    case "windows":
                                        {
                                            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && action)
                                            {
                                                continue;
                                            }
                                            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !action)
                                            {
                                                continue;
                                            }
                                            break;
                                        }
                                }
                            }
                        }

                    string localPath = lib.Downloads.Artifact.Path;
                    string libDirPath = Path.Combine(IOHelper.LibrariesDir, localPath.Remove(localPath.LastIndexOf('/'), localPath.Length - localPath.LastIndexOf('/')));
                    if (!Directory.Exists(libDirPath))
                        Directory.CreateDirectory(libDirPath);

                    string libFilePath = Path.Combine(IOHelper.LibrariesDir, localPath);
                    if (!File.Exists(libFilePath))
                    {
                        if (!string.IsNullOrEmpty(lib.Downloads.Artifact.Url))
                        {
                            byte[] bytes = await client.GetByteArrayAsync(lib.Downloads.Artifact.Url);
                            await File.WriteAllBytesAsync(libFilePath, bytes);
                        }
                        else
                            libraryDownloadedSize -= lib.Downloads.Artifact.Size; // Fix 100%+ bug
                    }

                    if (!_classPath.Contains(libFilePath))
                    {
                        _classPath += $"{libFilePath};";
                        if (lib.Name.StartsWith("org.lwjgl"))
                        {
                            natives.Add(lib);
                        }
                    }
                    libraryDownloadedSize += lib.Downloads.Artifact.Size;
                    if (libraryOverallSize == 0)
                        libraryOverallSize = 1; // NaN fix
                    double percent = libraryDownloadedSize / libraryOverallSize * 100;
                    UpdateProgressbar(percent, $"Downloading the '{lib.Name}' library... {percent:0.00}%");
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
            UpdateProgressbar(0, $"Checking natives...");
            if (!Directory.Exists(nativeDir))
                Directory.CreateDirectory(nativeDir);


            foreach (MCLibrary lib in nativeLibs)
            {
                string localPath = lib.Downloads.Artifact.Path;
                string libFilePath = Path.Combine(IOHelper.LibrariesDir, localPath);

                string tempZipDir = Path.Combine(IOHelper.TempDir, localPath.Split('.')[0]);

                ZipFile.ExtractToDirectory(libFilePath, tempZipDir, true);

                string[] files = Directory.GetFiles(tempZipDir, "*.dll", searchOption: SearchOption.AllDirectories);
                if (files != null)
                {
                    foreach (string file in files)
                    {
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
        private async Task<string> BuildArguments(string gameDir, string mainClass, string? modVersion = null)
        {
            List<string> arguments = new List<string>();

            UpdateProgressbar(0, $"Building arguments...");
            #region JVM
            arguments.Add("-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump");
            if (_jvmArgumentsBeforeClassPath.Count > 0)
                _jvmArgumentsBeforeClassPath.OrderByDescending(x => x.Priority).ToList().ForEach((LaunchArg a) => {
                    arguments.Add(a.Arg);
                });

            // Vanilla Args
            arguments.Add(MinecraftVersionMeta.Arguments.GetJVMArgString());

            // Moded Args
            if (_jvmArguments.Count > 0)
                _jvmArguments.OrderByDescending(x => x.Priority).ToList().ForEach((LaunchArg a) => {
                    arguments.Add(a.Arg);
                });

            // Maximum Memory
            if (Profile.Memory > 0 && Profile.Memory <= 256)
                arguments.Add($"-Xmx{Profile.Memory}G");
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
            #endregion
            #region Minecraft Args
            // The main class
            arguments.Add(mainClass);
            // Vanilla Args
            arguments.Add(MinecraftVersionMeta.Arguments.GetGameArgString());
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
                        arguments.Add($"--width {(int)SystemParameters.PrimaryScreenWidth * 0.44}");

                    if (Profile.Resolution.Y > 0)
                        arguments.Add($"--height {Profile.Resolution.Y}");
                    else
                        arguments.Add($"--height {(int)SystemParameters.PrimaryScreenHeight * 0.44}");
                }
            }
            else
            {
                arguments.Add($"--width {(int)SystemParameters.PrimaryScreenWidth * 0.44}");
                arguments.Add($"--height {(int)SystemParameters.PrimaryScreenHeight * 0.44}");
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
                .Replace("${natives_directory}", VersionData.NativesDir)
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
                .Replace("${classpath_separator}", ";");
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

            //Debug.WriteLine(arguments.Replace(' ', '\n'));
            Process? p = Process.Start(psi);
            return p;
        }
    }
}
