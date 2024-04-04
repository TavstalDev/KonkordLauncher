using KonkordLibrary.Enums;
using KonkordLibrary.Helpers;
using KonkordLibrary.Models.Installer;
using KonkordLibrary.Models.Minecraft.Library;
using Newtonsoft.Json;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Windows.Controls;

namespace KonkordLibrary.Models.Forge.Installer
{
    // Everything before 1.7
    public class ForgeInstOld : ForgeInstallerBase
    {
        public ForgeInstOld() : base() { }

        public ForgeInstOld(Profile profile, Label label, ProgressBar progressBar, bool isDebug) : base(profile, label, progressBar, isDebug)
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

            VersionDetails forgeVersion = Managers.GameManager.GetProfileVersionDetails(EProfileKind.FORGE, Profile.VersionId, Profile.VersionVanillaId, Profile.GameDirectory);

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

            /*UpdateProgressbar(0, $"Extracting forge universal...");
            await extractUniversal(installerDir, forgeUniversalPath, $"{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}");

            var jar = Path.Combine(installerDir, $"maven/net/minecraftforge/forge/{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}/forge-{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}.jar");
            if (File.Exists(jar)) //fix 1.17+ 
            {
                if (!File.Exists(forgeVersion.VersionJarPath))
                    File.Copy(jar, forgeVersion.VersionJarPath);
            }*/
            #endregion

            ModedData modedData = new ModedData(forgeVersionMeta.MainClass, forgeVersion, localLibraries);
            return modedData;
        }
    }
}
