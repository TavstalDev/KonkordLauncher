using KonkordLibrary.Helpers;
using KonkordLibrary.Models.Forge.Installer;
using KonkordLibrary.Models.Forge.New;
using KonkordLibrary.Models.Installer;
using KonkordLibrary.Models.Launcher;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;

namespace KonkordLibrary.Models.Forge
{
    public abstract class ForgeInstallerBase : MinecraftInstaller
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

        private static readonly Dictionary<string, Type> _installers = new Dictionary<string, Type>
        {
            #region New
            { "1.20.4", typeof(ForgeInstNew) },
            { "1.20.3", typeof(ForgeInstNew) },
            { "1.20.2", typeof(ForgeInstNew) },
            { "1.20.1", typeof(ForgeInstNew) },
            { "1.20", typeof(ForgeInstNew) },
            { "1.19.4", typeof(ForgeInstNew) },
            { "1.19.3", typeof(ForgeInstNew) },
            { "1.19.2", typeof(ForgeInstNew) },
            { "1.19.1", typeof(ForgeInstNew) },
            { "1.19", typeof(ForgeInstNew) },
            { "1.18.2", typeof(ForgeInstNew) },
            { "1.18.1", typeof(ForgeInstNew) },
            { "1.18", typeof(ForgeInstNew) },
            { "1.17.1", typeof(ForgeInstNew) },
            { "1.16.5", typeof(ForgeInstNew) },
            { "1.16.4", typeof(ForgeInstNew) },
            { "1.16.3", typeof(ForgeInstNew) },
            { "1.16.2", typeof(ForgeInstNew) },
            { "1.16.1", typeof(ForgeInstNew) },
            { "1.15.2", typeof(ForgeInstNew) },
            { "1.15.1", typeof(ForgeInstNew) },
            { "1.15", typeof(ForgeInstNew) },
            { "1.14.4", typeof(ForgeInstNew) },
            { "1.14.3", typeof(ForgeInstNew) },
            { "1.14.2", typeof(ForgeInstNew) },
            { "1.13.2", typeof(ForgeInstNew) },
            { "1.12.2", typeof(ForgeInstNew) },
            #endregion
            #region Legacy
            { "1.12.1", typeof(ForgeInstLegacy) },
            { "1.12", typeof(ForgeInstLegacy) },
            { "1.11.2", typeof(ForgeInstLegacy) },
            { "1.11", typeof(ForgeInstLegacy) },
            { "1.10.2", typeof(ForgeInstLegacy) },
            { "1.10", typeof(ForgeInstLegacy) },
            { "1.9.4", typeof(ForgeInstLegacy) },
            { "1.9", typeof(ForgeInstLegacy) },
            { "1.8.9", typeof(ForgeInstLegacy) },
            { "1.8.8", typeof(ForgeInstLegacy) },
            { "1.8", typeof(ForgeInstLegacy) },
            { "1.7.10", typeof(ForgeInstLegacy) },
            { "1.7.10_pre4", typeof(ForgeInstLegacy) },
            #endregion
            #region Old
            { "1.7.2", typeof(ForgeInstOld) },
            { "1.6.4", typeof(ForgeInstOld) },
            { "1.6.3", typeof(ForgeInstOld) },
            { "1.6.2", typeof(ForgeInstOld) },
            { "1.6.1", typeof(ForgeInstOld) },
            { "1.5.2", typeof(ForgeInstOld) },
            { "1.5.1", typeof(ForgeInstOld) },
            { "1.5", typeof(ForgeInstOld) },
            { "1.4.7", typeof(ForgeInstOld) },
            { "1.4.6", typeof(ForgeInstOld) },
            { "1.4.5", typeof(ForgeInstOld) },
            { "1.4.4", typeof(ForgeInstOld) },
            { "1.4.3", typeof(ForgeInstOld) },
            { "1.4.2", typeof(ForgeInstOld) },
            { "1.4.1", typeof(ForgeInstOld) },
            { "1.4.0", typeof(ForgeInstOld) },
            { "1.3.2", typeof(ForgeInstOld) },
            { "1.2.5", typeof(ForgeInstOld) },
            { "1.2.4", typeof(ForgeInstOld) },
            { "1.2.3", typeof(ForgeInstOld) },
            { "1.1", typeof(ForgeInstOld) }
            #endregion
        };

        public static Dictionary<string, Type> Installers { get { return _installers; } }

        private static readonly List<string> _unsupportedVersions = new List<string>()
        {
           "1.1", "1.2.3", "1.2.4", "1.2.5", "1.3.2", "1.4.0", "1.4.1", "1.4.2", "1.4.3", "1.4.4", "1.4.5", "1.4.6", "1.4.7", "1.5", "1.5.1", "1.5.2"
        };

        public static List<string> UnsupportedVersions {  get { return _unsupportedVersions; } }
        #endregion

        public ForgeInstallerBase() : base() { }

        public ForgeInstallerBase(Profile profile, Label label, ProgressBar progressBar, bool isDebug) : base(profile, label, progressBar, isDebug)
        {
        }

        /// <summary>
        /// Creates a Forge installer instance based on the specified parameters.
        /// </summary>
        /// <param name="profile">The profile.</param>
        /// <param name="label">The label for displaying status information.</param>
        /// <param name="progressBar">The progress bar for displaying installation progress.</param>
        /// <param name="isDebug">A flag indicating whether debug mode is enabled.</param>
        /// <returns>
        /// The created Forge installer instance, or null if creation fails.
        /// </returns>
        public static ForgeInstallerBase? Create(Profile profile, Label label, ProgressBar progressBar, bool isDebug)
        {
            ForgeInstallerBase? localInstaller = null;

            if (_installers.TryGetValue(profile.VersionVanillaId, out Type? value))
            {
                localInstaller = (ForgeInstallerBase?)Activator.CreateInstance(value, profile, label, progressBar, isDebug);
            }

            return localInstaller;
        }

        #region Functions from CmlLib
        /// <summary>
        /// Asynchronously maps and starts processors for the specified Forge version profile and installer directory.
        /// </summary>
        /// <param name="installProfile">The Forge version profile.</param>
        /// <param name="installerDir">The directory of the installer.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        protected async Task MapAndStartProcessors(ForgeVersionProfile installProfile, string installerDir)
        {
            JObject installerData = installProfile.Data;
            Dictionary<string, string?>? mapData = null;
            if (installerData != null)
                mapData = MapProcessorData(installerData, "client", VersionData.VanillaJarPath, installerDir);
            await StartProcessors(installProfile.Processors, mapData ?? new());
        }

        /// <summary>
        /// Maps processor data using the specified parameters.
        /// </summary>
        /// <param name="data">The processor data.</param>
        /// <param name="kind">The kind of processor.</param>
        /// <param name="minecraftJar">The path of the Minecraft JAR file.</param>
        /// <param name="installDir">The installation directory.</param>
        /// <returns>
        /// A dictionary containing mapped processor data.
        /// </returns>
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

        /// <summary>
        /// Asynchronously starts processors using the specified processor array and mapped data.
        /// </summary>
        /// <param name="processors">The array of processors to start.</param>
        /// <param name="mapData">The mapped data for processors.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
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
                UpdateProgressbarTranslated(percent, $"ui_building", new object[] { "forge", percent.ToString("0.00") });
            }
        }

        /// <summary>
        /// Checks processor outputs using the specified outputs object and mapped data.
        /// </summary>
        /// <param name="outputs">The outputs object to check.</param>
        /// <param name="mapData">The mapped data for processors.</param>
        /// <returns>
        /// True if processor outputs are valid; otherwise, false.
        /// </returns>
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

        /// <summary>
        /// Asynchronously starts a processor using the specified processor token and mapped data.
        /// </summary>
        /// <param name="processor">The processor token to start.</param>
        /// <param name="mapData">The mapped data for the processor.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
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

        /// <summary>
        /// Asynchronously starts a Java process with the specified classpath, main class, and arguments.
        /// </summary>
        /// <param name="classpath">The classpath for the Java process.</param>
        /// <param name="mainClass">The main class for the Java process.</param>
        /// <param name="args">Optional: The arguments for the Java process.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
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
                localJavaPath = localJavaPath.Replace("java.exe", "javaw.exe");
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
                NotificationHelper.SendErrorMsg(o, "Error - ForgeInstaller");
#endif
        }
        #endregion
    }
}
