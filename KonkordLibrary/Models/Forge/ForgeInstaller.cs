using KonkordLibrary.Enums;
using KonkordLibrary.Helpers;
using KonkordLibrary.Models.Fabric;
using KonkordLibrary.Models.GameManager;
using KonkordLibrary.Models.Installer;
using KonkordLibrary.Models.Minecraft.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
                byte[] bytes = await client.GetByteArrayAsync(string.Format(ForgeInstallerJarUrl, forgeVersion.VanillaVersion, forgeVersion.InstanceVersion));
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
            localLibraries.AddRange(installProfile.Libraries);

            // Copy vanilla jar
            if (!File.Exists(forgeVersion.VersionJarPath))
            {
                UpdateProgressbar(0, $"Copying the vanilla jar file...");
                File.Copy(forgeVersion.VanillaJarPath, forgeVersion.VersionJarPath);
            }

            foreach (var arg in forgeVersionMeta.Arguments.GetGameArgs())
                _gameArguments.Add(new LaunchArg(arg, 1));

            foreach (var arg in forgeVersionMeta.Arguments.GetJVMArgs())
            {
                _jvmArguments.Add(new LaunchArg(arg, 1));
            }

            _jvmArguments.Add(new LaunchArg("-DMcEmu=net.minecraft.client.main.Main", 1));
            _jvmArguments.Add(new LaunchArg("-Dlog4j2.formatMsgNoLookups=true", 1));
            _jvmArguments.Add(new LaunchArg("-Djava.rmi.server.useCodebaseOnly=true", 1));
            _jvmArguments.Add(new LaunchArg("-Dcom.sun.jndi.rmi.object.trustURLCodebase=false", 1));

            ModedData modedData = new ModedData(forgeVersionMeta.MainClass, forgeVersion, localLibraries);

            return modedData;
        }
    }
}
