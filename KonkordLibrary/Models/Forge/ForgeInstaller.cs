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
        private static readonly string _forgeVersionManifestUrl = "https://maven.minecraftforge.net/net/minecraftforge/forge/maven-metadata.xml";
        public static string ForgeVersionManifest { get { return _forgeVersionManifestUrl; } }

        // neoforge is the same as forge (but why, I can't tell)
        private static readonly string _forgeLoaderUniversalJarUrl = "https://files.minecraftforge.net/maven/net/minecraftforge/forge/{0}/forge-{0}-universal.jar";
        // Version Example: 1.20.4-49.0.38
        public static string ForgeLoaderUniversalJarUrl { get { return _forgeLoaderUniversalJarUrl; } }
        private static readonly string _forgeInstallerJarUrl = "https://files.minecraftforge.net/maven/net/minecraftforge/forge/{0}/forge-{0}-installer.jar";
        // Version Example: 1.20.4-49.0.38
        public static string ForgeInstallerJarUrl { get { return _forgeInstallerJarUrl; } }

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


            // Create versionDir in the versions folder
            if (!Directory.Exists(forgeVersion.VersionDirectory))
                Directory.CreateDirectory(forgeVersion.VersionDirectory);

            // Check libsizes dir
            string librarySizeCacheDir = Path.Combine(IOHelper.CacheDir, "libsizes");
            if (!Directory.Exists(librarySizeCacheDir))
                Directory.CreateDirectory(librarySizeCacheDir);
            string librarySizeCachePath = Path.Combine(librarySizeCacheDir, $"{forgeVersion.VanillaVersion}-forge-{forgeVersion.InstanceVersion}.json");

            // Download Installer
            string installerJarPath = Path.Combine(tempDir, "installer.jar");
            string installerDir = Path.Combine(tempDir, "installer");
            using (HttpClient client = new HttpClient())
            {
                byte[] bytes = await client.GetByteArrayAsync(string.Format(ForgeInstallerJarUrl, $"{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}"));
                await File.WriteAllBytesAsync(installerJarPath, bytes);
            }

            // Extract Installer
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

            List<MCLibrary> localLibraries = new List<MCLibrary>();
            localLibraries.AddRange(forgeVersionMeta.Libraries);

            using (HttpClient client = new HttpClient())
            {
                foreach (MCLibrary lib in installProfile.Libraries)
                {
                    string localPath = lib.Downloads.Artifact.Path;
                    string libDirPath = Path.Combine(IOHelper.LibrariesDir, localPath.Remove(localPath.LastIndexOf('/'), localPath.Length - localPath.LastIndexOf('/')));
                    if (!Directory.Exists(libDirPath))
                        Directory.CreateDirectory(libDirPath);
                    string libFilePath = Path.Combine(IOHelper.LibrariesDir, localPath);
                    if (!File.Exists(libFilePath))
                    {
                        // TODO, fix percent
                        UpdateProgressbar(0, $"Downloading the '{lib.Name}' library... {0:0.00}%");
                        if (!string.IsNullOrEmpty(lib.Downloads.Artifact.Url))
                        {
                            byte[] bytes = await client.GetByteArrayAsync(lib.Downloads.Artifact.Url);
                            await File.WriteAllBytesAsync(libFilePath, bytes);
                        }

                    }
                }
            }
            //localLibraries.AddRange(installProfile.Libraries);

            // Copy vanilla jar - NOT NEEDED, BREAKS IT
            /*if (!File.Exists(forgeVersion.VersionJarPath))
            {
                UpdateProgressbar(0, $"Copying the vanilla jar file...");
                File.Copy(forgeVersion.VanillaJarPath, forgeVersion.VersionJarPath);
            }*/

            // these mother fuckers are missing, then forge will work, for modern versions... :)
            #region GET CLIENTS libraries
            UpdateProgressbar(0, $"Checking forge client library files...");

            #endregion

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
            #endregion

            foreach (var arg in forgeVersionMeta.Arguments.GetGameArgs())
                _gameArguments.Add(new LaunchArg(arg, 1));

            foreach (var arg in forgeVersionMeta.Arguments.GetJVMArgs())
            {
                _jvmArguments.Add(new LaunchArg(arg, 1));
            }

            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-DMcEmu=net.minecraft.client.main.Main", 2));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Dlog4j2.formatMsgNoLookups=true", 2));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Djava.rmi.server.useCodebaseOnly=true", 2));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Dcom.sun.jndi.rmi.object.trustURLCodebase=false", 2));

            ModedData modedData = new ModedData(forgeVersionMeta.MainClass, forgeVersion, localLibraries);
            return modedData;
        }

        #region Temporal stuff
        // These functions for testing, if they work I will try to rework them to work with this project
        // Source: CmlLib
        protected async Task MapAndStartProcessors(JObject installProfile, string installerDir)
        {
            var installerData = installProfile["data"] as JObject;
            Dictionary<string, string?>? mapData = null;
            if (installerData != null)
                mapData = MapProcessorData(installerData, "client", VersionData.VanillaJarPath, installerDir);
            await StartProcessors(installProfile["processors"] as JArray, mapData ?? new());
        }

        protected Dictionary<string, string?> MapProcessorData(
            JObject data, string kind, string minecraftJar, string installDir)
        {
            var dataMapping = new Dictionary<string, string?>();
            foreach (var item in data)
            {
                var key = item.Key;
                var value = item.Value?[kind]?.ToString();

                if (string.IsNullOrEmpty(value))
                    continue;

                var fullPath = Mapper.ToFullPath(value, IOHelper.LibrariesDir);
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
                var item = processors[i];

                var outputs = item["outputs"] as JObject;
                if (outputs == null || !checkProcessorOutputs(outputs, mapData))
                {
                    var sides = item["sides"] as JArray;
                    if (sides == null || sides.FirstOrDefault()?.ToString() == "client") //skip server side
                        await startProcessor(item, mapData);
                }
            }
        }

        private bool checkProcessorOutputs(JObject outputs, Dictionary<string, string?> mapData)
        {
            foreach (var item in outputs)
            {
                if (item.Value == null)
                    continue;

                var key = Mapper.Interpolation(item.Key, mapData, true);
                var value = Mapper.Interpolation(item.Value.ToString(), mapData, true);

                if (!File.Exists(key) || !IOHelper.CheckSHA1(key, value))
                    return false;
            }

            return true;
        }

        private async Task startProcessor(JToken processor, Dictionary<string, string?> mapData)
        {
            var name = processor["jar"]?.ToString();
            if (name == null)
                return;

            // jar
            var jar = PackageName.Parse(name);
            var jarPath = Path.Combine(IOHelper.LibrariesDir, jar.GetPath());

            var jarFile = new JarFile(jarPath);
            var jarManifest = jarFile.GetManifest();

            // mainclass
            string? mainClass = null;
            var hasMainclass = jarManifest?.TryGetValue("Main-Class", out mainClass) ?? false;
            if (!hasMainclass || string.IsNullOrEmpty(mainClass))
                return;

            // classpath
            var classpathObj = processor["classpath"];
            var classpath = new List<string>();
            if (classpathObj != null)
            {
                foreach (var libName in classpathObj)
                {
                    var libNameString = libName?.ToString();
                    if (string.IsNullOrEmpty(libNameString))
                        continue;

                    var lib = Path.Combine(IOHelper.LibrariesDir,
                        PackageName.Parse(libNameString).GetPath());
                    classpath.Add(lib);
                }
            }
            classpath.Add(jarPath);

            // arg
            var argsArr = processor["args"] as JArray;
            string[]? args = null;
            if (argsArr != null)
            {
                var arrStrs = argsArr.Select(x => x.ToString()).ToArray();
                args = Mapper.Map(arrStrs, mapData, IOHelper.LibrariesDir);
            }

            await startJava(classpath.ToArray(), mainClass, args);
        }

        private async Task startJava(string[] classpath, string mainClass, string[]? args)
        {
            if (string.IsNullOrEmpty(JavaPath))
                throw new InvalidOperationException("JavaPath was empty");

            var arg =
                $"-cp {IOUtil.CombinePath(classpath)} " +
                $"{mainClass}";

            if (args != null && args.Length > 0)
                arg += " " + string.Join(" ", args);

            var process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {
                FileName = JavaPath,
                Arguments = arg,
            };

            process.Start();
            await process.WaitForExitAsync();
        }
        #endregion
    }
}
