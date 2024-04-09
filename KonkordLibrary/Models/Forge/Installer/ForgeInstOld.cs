using KonkordLibrary.Enums;
using KonkordLibrary.Helpers;
using KonkordLibrary.Models.Forge.Legacy;
using KonkordLibrary.Models.Installer;
using KonkordLibrary.Models.Launcher;
using KonkordLibrary.Models.Minecraft.Library;
using KonkordLibrary.Models.Minecraft.Meta;
using Newtonsoft.Json;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Windows.Controls;

namespace KonkordLibrary.Models.Forge.Installer
{
    /*
     * 1.13+ are new
     * 1.7.10+ are legacy
     * 1.6 - ok
     * 1.5.2 -
     * these below are zips
     * 1.5.1 -
     * 1.4 -
     * 1.3 -
     * 1.2 -
     * 1.1 - 
     * 1.0 - 
    */
    public class ForgeInstOld : ForgeInstallerBase
    {
        private string _extraVersion { get; set; }

        public ForgeInstOld() : base() { }

        public ForgeInstOld(Profile profile, Label label, ProgressBar progressBar, bool isDebug) : base(profile, label, progressBar, isDebug)
        {
        }

        internal override async Task<ModedData?> InstallModed(string tempDir)
        {
            UpdateProgressbarTranslated(0, "ui_finding_recommended_java");
            string? RecommendedJavaPath;
            if (MinecraftVersionMeta.JavaVersion != null)
                MinecraftVersionMeta.JavaVersion.FindOnSystem(out RecommendedJavaPath);
            else
                JavaVersion.FindOnSystem(8, out RecommendedJavaPath);

            if (!string.IsNullOrEmpty(RecommendedJavaPath) && string.IsNullOrEmpty(Profile.JavaPath))
            {
                if (Directory.Exists(RecommendedJavaPath))
                    JavaPath = Path.Combine(RecommendedJavaPath, "bin", IsDebug ? "java.exe" : "javaw.exe");
            }

            UpdateProgressbarTranslated(0, "ui_reading_manifest", new object[] { "forge" });
            if (!File.Exists(IOHelper.ForgeManifestJsonFile))
            {
                NotificationHelper.SendErrorTranslated("manifest_file_not_found", "messagebox_error", new object[] { "forge" });
                return null;
            }

            VersionDetails forgeVersion = GameHelper.GetProfileVersionDetails(EProfileKind.FORGE, Profile.VersionId, Profile.VersionVanillaId, Profile.GameDirectory);

            UpdateProgressbarTranslated(0, $"ui_creating_directories");
            // Create versionDir in the versions folder
            if (!Directory.Exists(forgeVersion.VersionDirectory))
                Directory.CreateDirectory(forgeVersion.VersionDirectory);

            // Check libsizes dir
            string librarySizeCacheDir = Path.Combine(IOHelper.CacheDir, "libsizes");
            if (!Directory.Exists(librarySizeCacheDir))
                Directory.CreateDirectory(librarySizeCacheDir);

            // Download Installer
            UpdateProgressbarTranslated(0, $"ui_downloading_installer", new object[] { "forge" });

            // Check Minecraft Version
            string[] rawVersion = VersionData.VanillaVersion.Split('.');
            int MajorVersion = int.Parse(rawVersion[1]);
            int MinorVersion = 0;
            if (rawVersion.Length == 3)
                MinorVersion = int.Parse(rawVersion[2]);

            string installerFormat = ".jar";
            string forgeInstallerUrl = ForgeInstallerJarUrl;
            bool isZipInstaller = false;
            if (MajorVersion == 5 && MinorVersion < 2 || MajorVersion < 5)
            {
                installerFormat = ".zip";
                forgeInstallerUrl = ForgeLoaderUniversalJarUrl.Replace(".jar", ".zip");
                isZipInstaller = true;
            }

            string installerJarPath = Path.Combine(tempDir, $"installer{installerFormat}");
            string installerDir = Path.Combine(tempDir, "installer");
            using (HttpClient client = new HttpClient())
            {
                byte[] bytes;
                try
                {
                    bytes = await client.GetByteArrayAsync(string.Format(forgeInstallerUrl, $"{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}"));
                }
                catch
                {
                    int length = forgeVersion.VanillaVersion.Split('.').Length;
                    if (length == 3)
                    {
                        bytes = await client.GetByteArrayAsync(string.Format(forgeInstallerUrl, $"{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}-{forgeVersion.VanillaVersion}"));
                        _extraVersion = $"-{forgeVersion.VanillaVersion}";
                    }
                    else
                    {
                        bytes = await client.GetByteArrayAsync(string.Format(forgeInstallerUrl, $"{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}-{forgeVersion.VanillaVersion}.0"));
                        _extraVersion = $"-{forgeVersion.VanillaVersion}.0";
                    }
                }
                await File.WriteAllBytesAsync(installerJarPath, bytes);
            }

            // Extract Installer
            UpdateProgressbarTranslated(0, $"ui_extracting_installer", new object[] { "forge" });
            ZipFile.ExtractToDirectory(installerJarPath, installerDir);

            ModedData modedData;
            if (isZipInstaller) // Old Zip Installer
            {
                // Extract Vanilla

                // Remove META-INF

                // Copy Installer to Vanilla.jar


                // Package vanilla.jar

                modedData = new ModedData("net.minecraft.client.main.Main", forgeVersion, new List<MCLibrary>());
                throw new NotImplementedException($"The '{VersionData.VanillaVersion}-{VersionData.InstanceVersion}' forge version is not supported for now.");
            }
            else // Jar Installer
            {
                // Move version.json and profile.json 
                string installProfileJson = Path.Combine(forgeVersion.VersionDirectory, "install_profile.json");
                // INSTALL PROFILE
                if (!File.Exists(installProfileJson))
                    File.Move(Path.Combine(installerDir, "install_profile.json"), installProfileJson);

                // EXTRACT UNIVERSAL
                string universalJarPath = Path.Combine(installerDir, $"minecraftforge-universal-{forgeVersion.VanillaVersion}-{forgeVersion.InstanceVersion}{_extraVersion}.jar");
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

                ForgeProfile? installProfile = JsonConvert.DeserializeObject<ForgeProfile>(await File.ReadAllTextAsync(installProfileJson));
                if (installProfile == null)
                    throw new FileNotFoundException("Failed to get the forge install profile meta.");

                // VERSION
                ForgeVersionMeta? forgeVersionMeta = installProfile.VersionInfo;
                if (forgeVersionMeta == null)
                    throw new FileNotFoundException("Failed to get the forge version meta.");

                UpdateProgressbarTranslated(0, $"ui_checking_installer_libraries", new object[] { "forge" });
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
                UpdateProgressbarTranslated(0, $"ui_adding_arguments", new object[] { "forge" });
                if (forgeVersionMeta.MinecraftArguments != null)
                {
                    MinecraftVersionMeta.ArgumentsLegacy = forgeVersionMeta.MinecraftArguments;
                }

                // Copy vanilla jar
                if (!File.Exists(forgeVersion.VersionJarPath))
                {
                    UpdateProgressbarTranslated(0, $"ui_copying_jar", new object[] { "vanilla" });
                    File.Copy(forgeVersion.VanillaJarPath, forgeVersion.VersionJarPath);
                }

                modedData = new ModedData(forgeVersionMeta.MainClass, forgeVersion, localLibraries);
            }

            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-DMcEmu=net.minecraft.client.main.Main", 2));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Dlog4j2.formatMsgNoLookups=true", 2));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Djava.rmi.server.useCodebaseOnly=true", 2));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg("-Dcom.sun.jndi.rmi.object.trustURLCodebase=false", 2));
            _jvmArgumentsBeforeClassPath.Add(new LaunchArg($"-Dminecraft.client.jar={forgeVersion.VersionJarPath}", 2));

            
            //_classPath += $"{forgeVersion.VersionJarPath};"; - not needed
            return modedData;
        }
    }
}
