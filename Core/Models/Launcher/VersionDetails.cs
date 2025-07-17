namespace Tavstal.KonkordLauncher.Core.Models.Launcher
{
    public class VersionDetails
    {
        public string VanillaVersion { get; set; } = string.Empty;
        public string InstanceVersion { get; set; } = string.Empty;
        public string VersionDirectory { get; set; } = string.Empty;
        public string VersionJsonPath { get; set; } = string.Empty;
        public string VersionJarPath { get; set; } = string.Empty;
        public string VanillaJarPath { get; set; } = string.Empty;
        public string GameDir { get; set; } = string.Empty;
        public string NativesDir { get; set; } = string.Empty;

        public VersionDetails() { }

        public VersionDetails(string vanillaVersion, string instanceVersion, string versionDirectory, string versionJsonPath, string versionJarPath, string vanillaJarPath, string gameDir, string nativesDir)
        {
            VanillaVersion = vanillaVersion;
            InstanceVersion = instanceVersion;
            VersionDirectory = versionDirectory;
            VersionJsonPath = versionJsonPath;
            VersionJarPath = versionJarPath;
            VanillaJarPath = vanillaJarPath;
            GameDir = gameDir;
            NativesDir = nativesDir;
        }
    }
}
