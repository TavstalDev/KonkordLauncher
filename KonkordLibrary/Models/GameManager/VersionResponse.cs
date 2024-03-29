namespace KonkordLibrary.Models.GameManager
{
    public class VersionResponse
    {
        public string VanillaVersion { get; set; } = string.Empty;
        public string InstanceVersion { get; set; } = string.Empty;
        public string VersionDirectory { get; set; } = string.Empty;
        public string VersionJsonPath { get; set; } = string.Empty;
        public string VersionJarPath { get; set; } = string.Empty;
        public string VanillaJarPath { get; set; } = string.Empty;
        public string GameDir { get; set; } = string.Empty;

        public VersionResponse() { }

        public VersionResponse(string vanillaVersion, string instanceVersion, string versionDirectory, string versionJsonPath, string versionJarPath, string vanillaJarPath, string gameDir)
        {
            VanillaVersion = vanillaVersion;
            InstanceVersion = instanceVersion;
            VersionDirectory = versionDirectory;
            VersionJsonPath = versionJsonPath;
            VersionJarPath = versionJarPath;
            VanillaJarPath = vanillaJarPath;
            GameDir = gameDir;
        }
    }
}
