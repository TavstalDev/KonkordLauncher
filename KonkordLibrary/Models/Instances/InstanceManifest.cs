using KonkordLibrary.Enums;
using KonkordLibrary.Models.Launcher;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace KonkordLibrary.Models.Instances
{
    public class InstanceManifest : Profile
    {
        [JsonProperty("useFileServer"), JsonPropertyName("useFileServer")]
        public bool UseFileServer { get; set; }
        [JsonProperty("fileServer"), JsonPropertyName("fileServer")]
        public string? FileServer { get; set; }
        [JsonProperty("modList"), JsonPropertyName("modList")]
        public List<string> ModList { get; set; }
        [JsonProperty("manifestVersion"), JsonPropertyName("manifestVersion")]
        public int ManifestVersion { get; set; }

        public InstanceManifest() { }

        public InstanceManifest(bool useFileServer, string? fileServer, List<string> modList, int manifestVersion, string name, string icon, string versionId, string versionVanillaId, EProfileType type, EProfileKind kind, Resolution resolution, string gameDirectory, string javaPath, string jVMArgs, int memory, ELaucnherVisibility launcherVisibility)
        {
            UseFileServer = useFileServer;
            FileServer = fileServer;
            ModList = modList;
            ManifestVersion = manifestVersion;
            Name = name;
            Icon = icon;
            VersionId = versionId;
            VersionVanillaId = versionVanillaId;
            Type = type;
            Kind = kind;
            Resolution = resolution;
            GameDirectory = gameDirectory;
            JavaPath = javaPath;
            JVMArgs = jVMArgs;
            Memory = memory;
            LauncherVisibility = launcherVisibility;
        }
    }
}
