using KonkordLibrary.Enums;
using KonkordLibrary.Helpers;
using KonkordLibrary.Models.Forge.Legacy;
using KonkordLibrary.Models.Installer;
using KonkordLibrary.Models.Minecraft.Library;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Windows.Controls;

namespace KonkordLibrary.Models.Forge.Installer
{
    /*
     * 1.13+ are new
     * 1.12.x - ok
     * 1.11.x - ok
     * 1.9.x - ok
     * 1.8.x - ok
     * 1.7.x - ok
     * 1.6 and below are old
    */
    public class ForgeInstLegacy : ForgeInstallerBase
    {
        private string _extraVersion {  get; set; }

        public ForgeInstLegacy() : base() { }

        public ForgeInstLegacy(Profile profile, Label label, ProgressBar progressBar, bool isDebug) : base(profile, label, progressBar, isDebug)
        {
        }

        internal override async Task<ModedData?> InstallModed(string tempDir)
        {
            UpdateProgressbar(0, $"Trying to get recommended java path...");
            MinecraftVersionMeta.JavaVersion.FindOnSystem(out string? RecommendedJavaPath);
            if (!string.IsNullOrEmpty(RecommendedJavaPath) && string.IsNullOrEmpty(Profile.JavaPath))
            {
                if (Directory.Exists(RecommendedJavaPath))
                    JavaPath = Path.Combine(RecommendedJavaPath, "bin", IsDebug ? "java.exe" : "javaw.exe");
            }

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
                byte[] bytes;
                try
                {
                    bytes = await client.GetByteArrayAsync(string.Format(ForgeInstallerJarUrl, $"{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}"));
                }
                catch
                {
                    int length = forgeVersion.VanillaVersion.Split('.').Length;
                    if (length == 3)
                    {
                        bytes = await client.GetByteArrayAsync(string.Format(ForgeInstallerJarUrl, $"{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}-{forgeVersion.VanillaVersion}"));
                        _extraVersion = $"-{forgeVersion.VanillaVersion}";
                    }
                    else
                    {
                        bytes = await client.GetByteArrayAsync(string.Format(ForgeInstallerJarUrl, $"{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}-{forgeVersion.VanillaVersion}.0"));
                        _extraVersion = $"-{forgeVersion.VanillaVersion}.0";
                    }
                }
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

            // EXTRACT UNIVERSAL
            string universalJarPath = Path.Combine(installerDir, $"forge-{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}{_extraVersion}-universal.jar");
            string universalDir = Path.Combine(installerDir, $"forge-{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}{_extraVersion}-universal");
            if (!Directory.Exists(universalDir) && File.Exists(universalJarPath))
            {
                ZipFile.ExtractToDirectory(universalJarPath, universalDir);
            }

            // COPY UNIVERSAL
            string forgeUniversalDir = Path.Combine(IOHelper.LibrariesDir, $"net\\minecraftforge\\forge\\{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}");
            string forgeUniversalPath = Path.Combine(forgeUniversalDir, $"forge-{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}{_extraVersion}-universal.jar");
            if (!Directory.Exists(forgeUniversalDir))
                Directory.CreateDirectory(forgeUniversalDir);

            if (!File.Exists(forgeUniversalPath))
                File.Copy(universalJarPath, forgeUniversalPath);
            _classPath += $"{forgeUniversalPath};";

            // VERSION
            if (!File.Exists(forgeVersion.VersionJsonPath))
                File.Move(Path.Combine(universalDir, "version.json"), forgeVersion.VersionJsonPath);

            ForgeProfile? installProfile = JsonConvert.DeserializeObject<ForgeProfile>(await File.ReadAllTextAsync(installProfileJson));
            if (installProfile == null)
                throw new FileNotFoundException("Failed to get the forge install profile meta.");

            ForgeVersionMeta? forgeVersionMeta = installProfile.VersionInfo;
            if (forgeVersionMeta == null)
                throw new FileNotFoundException("Failed to get the forge version meta.");

            UpdateProgressbar(0, $"Checking forge installer libraries...");
            List<MCLibrary> localLibraries = new List<MCLibrary>();
            foreach (var lib in forgeVersionMeta.Libraries)
            {
                string? url = lib.GetUrl(true);
                if (url == null)
                    continue;

                localLibraries.Add(new MCLibrary()
                {
                    Name = lib.Name,
                    Downloads = new MCLibraryDownloads()
                    {
                        Artifact = new MCLibraryArtifact()
                        {
                            Path = lib.GetPath(),
                            Sha1 = string.Empty,
                            Size = 0,
                            Url = url,
                        },
                        Classifiers = null
                    },
                    Natives = null,
                    Rules = new List<MCLibraryRule>()
                });
            }

            // Add launch arguments
            UpdateProgressbar(0, $"Adding forge arguments...");
            if (forgeVersionMeta.MinecraftArguments != null)
            {
                MinecraftVersionMeta.ArgumentsLegacy = forgeVersionMeta.MinecraftArguments;
            }

            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-DMcEmu=net.minecraft.client.main.Main", 2));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Dlog4j2.formatMsgNoLookups=true", 2));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Djava.rmi.server.useCodebaseOnly=true", 2));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Dcom.sun.jndi.rmi.object.trustURLCodebase=false", 2));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg($"-Dminecraft.client.jar={forgeVersion.VersionJarPath}", 2));

            // Copy vanilla jar
            if (!File.Exists(forgeVersion.VersionJarPath))
            {
                UpdateProgressbar(0, $"Copying the vanilla jar file...");
                File.Copy(forgeVersion.VanillaJarPath, forgeVersion.VersionJarPath);
            }
            //_classPath += $"{forgeVersion.VersionJarPath};"; - not needed


            ModedData modedData = new ModedData(forgeVersionMeta.MainClass, forgeVersion, localLibraries);
            return modedData;
        }
    }
}
