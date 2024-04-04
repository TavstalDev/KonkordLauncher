using KonkordLibrary.Enums;
using KonkordLibrary.Helpers;
using KonkordLibrary.Models.GameManager;
using KonkordLibrary.Models.Installer;
using KonkordLibrary.Models.Minecraft.Library;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Windows.Controls;

namespace KonkordLibrary.Models.Forge
{
    public class ForgeInstaller : MinecraftInstaller
    {
        #region Variables
        private static readonly string _forgeVersionManifestUrl = "https://maven.minecraftforge.net/net/minecraftforge/forge/maven-metadata.xml";
        public static string ForgeVersionManifest { get { return _forgeVersionManifestUrl; } }

        // neoforge is the same as forge (but why, I can't tell)
        private static readonly string _forgeLoaderUniversalJarUrl = "https://files.minecraftforge.net/maven/net/minecraftforge/forge/{0}/forge-{0}-universal.jar";
        // Version Example: 1.20.4-49.0.38
        public static string ForgeLoaderUniversalJarUrl { get { return _forgeLoaderUniversalJarUrl; } }
        private static readonly string _forgeInstallerJarUrl = "https://files.minecraftforge.net/maven/net/minecraftforge/forge/{0}/forge-{0}-installer.jar";
        // Version Example: 1.20.4-49.0.38
        public static string ForgeInstallerJarUrl { get { return _forgeInstallerJarUrl; } }
        #endregion

        public ForgeInstaller(Profile profile, Label label, ProgressBar progressBar, bool isDebug) : base(profile, label, progressBar, isDebug)
        {
        }

        internal override async Task<ModedData?> InstallModed(string tempDir)
        {
            UpdateProgressbar(0, $"Reading the forgeManifest file...");
            if (!File.Exists(IOHelper.ForgeManifestJsonFile))
            {
                NotificationHelper.SendError("Failed to get the forge manifest.", "Error");
                return null;
            }

            VersionResponse forgeVersion = Managers.GameManager.GetProfileVersionDetails(EProfileKind.FORGE, Profile.VersionId, Profile.VersionVanillaId, Profile.GameDirectory);

            UpdateProgressbar(0, $"Creating directories...");
            // Create versionDir in the versions folder
            if (!Directory.Exists(forgeVersion.VersionDirectory))
                Directory.CreateDirectory(forgeVersion.VersionDirectory);

            // Check libsizes dir
            string librarySizeCacheDir = Path.Combine(IOHelper.CacheDir, "libsizes");
            if (!Directory.Exists(librarySizeCacheDir))
                Directory.CreateDirectory(librarySizeCacheDir);

            // Download Installer
            UpdateProgressbar(0, $"Downloading the forge installer...");
            string installerJarPath = Path.Combine(tempDir, "installer.jar");
            string installerDir = Path.Combine(tempDir, "installer");
            using (HttpClient client = new HttpClient())
            {
                byte[] bytes = await client.GetByteArrayAsync(string.Format(ForgeInstallerJarUrl, $"{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}"));
                await File.WriteAllBytesAsync(installerJarPath, bytes);
            }

            // Extract Installer
            UpdateProgressbar(0, $"Extracting the forge installer...");
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
                    _classPath += $"{newPath};";
                }
                IOHelper.MoveDirectory(mavenTempDir, IOHelper.LibrariesDir, false);
            }

            ForgeVersionMeta? forgeVersionMeta = JsonConvert.DeserializeObject<ForgeVersionMeta>(await File.ReadAllTextAsync(forgeVersion.VersionJsonPath));
            if (forgeVersionMeta == null)
                throw new FileNotFoundException("Failed to get the forge version meta.");

            ForgeVersionProfile? installProfile = JsonConvert.DeserializeObject<ForgeVersionProfile>(await File.ReadAllTextAsync(installProfileJson));
            if (installProfile == null)
                throw new FileNotFoundException("Failed to get the forge install profile meta.");

            UpdateProgressbar(0, $"Checking forge installer libraries...");
            List<MCLibrary> localLibraries = new List<MCLibrary>();
            localLibraries.AddRange(forgeVersionMeta.Libraries);

            // Download installer libraries
            string librarySizeCachePath = Path.Combine(librarySizeCacheDir, $"{forgeVersion.VanillaVersion}-forge-installer-{forgeVersion.InstanceVersion}.json");
            using (HttpClient client = new HttpClient())
            {
                int downloadedSize = 0;
                int toDownloadSize = 0;
                if (!File.Exists(librarySizeCachePath))
                {
                    foreach (MCLibrary lib in installProfile.Libraries)
                        toDownloadSize += lib.Downloads.Artifact.Size;
                    UpdateProgressbar(0, $"Saving library size cache file...");
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
                        double percent = (double)downloadedSize / (double)toDownloadSize * 100d;
                        UpdateProgressbar(percent, $"Downloading the '{lib.Name}' library... {percent:0.00}%");
                        if (!string.IsNullOrEmpty(lib.Downloads.Artifact.Url))
                        {
                            byte[] bytes = await client.GetByteArrayAsync(lib.Downloads.Artifact.Url);
                            await File.WriteAllBytesAsync(libFilePath, bytes);
                        }

                    }

                    
                }
            }

            // Copy vanilla jar - NOT NEEDED, BREAKS IT
            /*if (!File.Exists(forgeVersion.VersionJarPath))
            {
                UpdateProgressbar(0, $"Copying the vanilla jar file...");
                File.Copy(forgeVersion.VanillaJarPath, forgeVersion.VersionJarPath);
            }*/

            // Add launch arguments
            UpdateProgressbar(0, $"Adding forge arguments...");
            foreach (var arg in forgeVersionMeta.Arguments.GetGameArgs())
                _gameArguments.Add(new LaunchArg(arg, 1));

            foreach (var arg in forgeVersionMeta.Arguments.GetJVMArgs())
                _jvmArguments.Add(new LaunchArg(arg, 1));

            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-DMcEmu=net.minecraft.client.main.Main", 2));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Dlog4j2.formatMsgNoLookups=true", 2));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Djava.rmi.server.useCodebaseOnly=true", 2));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Dcom.sun.jndi.rmi.object.trustURLCodebase=false", 2));

            UpdateProgressbar(0, $"Building forge...");
            // Generate client libs
            await MapAndStartProcessors(installProfile, installerDir);

            #region GET minecraftforge libraries
            string forgeUniversal = string.Format(ForgeLoaderUniversalJarUrl, $"{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}");

            string forgeUniversalDir = Path.Combine(IOHelper.LibrariesDir, $"net\\minecraftforge\\forge\\{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}");
            string forgeUniversalPath = Path.Combine(forgeUniversalDir, $"forge-{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}-universal.jar");
            if (!Directory.Exists(forgeUniversalDir))
                Directory.CreateDirectory(forgeUniversalDir);

            UpdateProgressbar(0, $"Checking forge universal library file...");
            if (!File.Exists(forgeUniversalPath))
            {
                UpdateProgressbar(0, $"Downloadig forge universal library file...");
                using (HttpClient client = new HttpClient())
                {
                    byte[] bytes = await client.GetByteArrayAsync(forgeUniversal);
                    await File.WriteAllBytesAsync(forgeUniversalPath, bytes);
                }
            }

            UpdateProgressbar(0, $"Extracting forge universal...");
            await extractUniversal(installerDir, forgeUniversalPath, $"{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}");

            var jar = Path.Combine(installerDir, $"maven/net/minecraftforge/forge/{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}/forge-{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}.jar");
            if (File.Exists(jar)) //fix 1.17+ 
            {
                if (!File.Exists(forgeVersion.VersionJarPath))
                    File.Copy(jar, forgeVersion.VersionJarPath);
            }
            #endregion

            ModedData modedData = new ModedData(forgeVersionMeta.MainClass, forgeVersion, localLibraries);
            return modedData;
        }

        private async Task extractUniversal(string installerPath, string universalPath, string forgeVersion)
        {
            // copy universal library to minecraft
            var universal = Path.Combine(installerPath, universalPath);
            var des = Path.Combine(IOHelper.LibrariesDir, $"net\\minecraftforge\\forge\\{forgeVersion}\\forge-{forgeVersion}-client.jar");

            if (File.Exists(universal) && !File.Exists(des))
            {
                string desDir = des.Remove(des.LastIndexOf("\\"), des.Length - des.LastIndexOf("\\"));
                if (!Directory.Exists(desDir))
                    Directory.CreateDirectory(desDir);

                File.Copy(universal, des);
                Debug.WriteLine($"{universal} \n {des}");
                
            }
        }

        #region Temporal stuff
        // These functions for testing, if they work I will try to rework them to work with this project
        // Source: CmlLib
        protected async Task MapAndStartProcessors(ForgeVersionProfile installProfile, string installerDir)
        {
            JObject installerData = installProfile.Data;
            Dictionary<string, string?>? mapData = null;
            if (installerData != null)
                mapData = MapProcessorData(installerData, "client", VersionData.VanillaJarPath, installerDir);
            await StartProcessors(installProfile.Processors, mapData ?? new());
        }

        protected Dictionary<string, string?> MapProcessorData(JObject data, string kind, string minecraftJar, string installDir)
        {
            Dictionary<string, string?> dataMapping = new Dictionary<string, string?>();
            foreach (KeyValuePair<string, JToken?> item in data)
            {
                string key = item.Key;
                string? value = item.Value?[kind]?.ToString();

                if (string.IsNullOrEmpty(value))
                    continue;

                string? fullPath = Mapper.ToFullPath(value, IOHelper.LibrariesDir);
                if (fullPath == value)
                {
                    value = value.Trim('/');
                    dataMapping.Add(key, Path.Combine(installDir, value));
                }
                else
                    dataMapping.Add(key, fullPath);
            }

            dataMapping.Add("SIDE", "client");
            dataMapping.Add("MINECRAFT_JAR", minecraftJar);
            dataMapping.Add("INSTALLER", Path.Combine(installDir, "installer.jar"));

            return dataMapping;
        }

        protected async Task StartProcessors(JArray? processors, Dictionary<string, string?> mapData)
        {
            if (processors == null || processors.Count == 0)
                return;

            for (int i = 0; i < processors.Count; i++)
            {
                JToken item = processors[i];

                JObject? outputs = item["outputs"] as JObject;
                if (outputs == null || !checkProcessorOutputs(outputs, mapData))
                {
                    JArray? sides = item["sides"] as JArray;
                    if (sides == null || sides.FirstOrDefault()?.ToString() == "client") //skip server side
                        await startProcessor(item, mapData);
                }
                double percent = (double)i / (double)processors.Count * 100d;
                UpdateProgressbar(percent, $"Building forge {percent:00}%...");
            }
        }

        private bool checkProcessorOutputs(JObject outputs, Dictionary<string, string?> mapData)
        {
            foreach (var item in outputs)
            {
                if (item.Value == null)
                    continue;

                string key = Mapper.Interpolation(item.Key, mapData, true);
                string value = Mapper.Interpolation(item.Value.ToString(), mapData, true);

                if (!File.Exists(key) || !IOHelper.CheckSHA1(key, value))
                    return false;
            }

            return true;
        }

        private async Task startProcessor(JToken processor, Dictionary<string, string?> mapData)
        {
            string? name = processor["jar"]?.ToString();
            if (name == null)
                return;

            // jar
            PackageName jar = PackageName.Parse(name);
            string jarPath = Path.Combine(IOHelper.LibrariesDir, jar.GetPath());

            ProcessorJarFile jarFile = new ProcessorJarFile(jarPath);
            Dictionary<string, string?>? jarManifest = jarFile.GetManifest();

            // mainclass
            string? mainClass = null;
            bool hasMainclass = jarManifest?.TryGetValue("Main-Class", out mainClass) ?? false;
            if (!hasMainclass || string.IsNullOrEmpty(mainClass))
                return;

            // classpath
            JToken? classpathObj = processor["classpath"];
            List<string> classpath = new List<string>();
            if (classpathObj != null)
            {
                foreach (var libName in classpathObj)
                {
                    string? libNameString = libName?.ToString();
                    if (string.IsNullOrEmpty(libNameString))
                        continue;

                    string? lib = Path.Combine(IOHelper.LibrariesDir,
                        PackageName.Parse(libNameString).GetPath());
                    classpath.Add(lib);
                }
            }
            classpath.Add(jarPath);

            // arg
            JArray? argsArr = processor["args"] as JArray;
            string[]? args = null;
            if (argsArr != null)
            {
                string[]? arrStrs = argsArr.Select(x => x.ToString()).ToArray();
                args = Mapper.Map(arrStrs, mapData, IOHelper.LibrariesDir);
            }

            await startJava(classpath.ToArray(), mainClass, args);
        }

        private async Task startJava(string[] classpath, string mainClass, string[]? args)
        {
            if (string.IsNullOrEmpty(JavaPath))
                throw new InvalidOperationException("JavaPath was empty");

            string combinedPath = string.Join(Path.PathSeparator.ToString(),
                classpath.Select(x =>
                {
                    string path = Path.GetFullPath(x);
                    if (path.Contains(' '))
                        return "\"" + path + "\"";
                    else
                        return path;
                }));

            string? arg =
                $"-cp {combinedPath} " +
                $"{mainClass}";

            if (args != null && args.Length > 0)
                arg += " " + string.Join(" ", args);

            Process process = new Process();

            string localJavaPath = JavaPath;
            if (localJavaPath.EndsWith("java.exe"))
                localJavaPath.Replace("java.exe", "javaw.exe");
            if (localJavaPath.EndsWith("java"))
                localJavaPath += "w";
            // java.exe - shows console
            // javaw.exe - does not show console

            process.StartInfo = new ProcessStartInfo()
            {
                FileName = localJavaPath,
                Arguments = arg,
                UseShellExecute = false,
                RedirectStandardError = true
            };

            process.Start();
            await process.WaitForExitAsync();

#if DEBUG
            string o = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(o))
                NotificationHelper.SendError(o, "Error");
#endif
        }
        #endregion
    }
}
