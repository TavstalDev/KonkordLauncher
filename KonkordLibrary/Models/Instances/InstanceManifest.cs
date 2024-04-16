using Tavstal.KonkordLibrary.Enums;
using Tavstal.KonkordLibrary.Models.Launcher;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Tavstal.KonkordLibrary.Models.Instances
{
    public class InstanceManifest : Profile
    {
        [JsonProperty("fileServer"), JsonPropertyName("fileServer")]
        public string? FileServer { get; set; }
        [JsonProperty("modList"), JsonPropertyName("modList")]
        public List<string> ModList { get; set; }
        [JsonProperty("manifestVersion"), JsonPropertyName("manifestVersion")]
        public int ManifestVersion { get; set; }

        public InstanceManifest() { }

        public InstanceManifest(string? fileServer, List<string> modList, int manifestVersion, string name, string icon, string versionId, string versionVanillaId, EProfileType type, EProfileKind kind, Resolution resolution, string gameDirectory, string javaPath, string jVMArgs, int memory, ELaucnherVisibility launcherVisibility)
        {
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

        public Profile GetProfile()
        {
            return new Profile()
            {
                Name = Name,
                Icon = Icon,
                VersionId = VersionId,
                VersionVanillaId = VersionVanillaId,
                Type = Type,
                Kind = Kind,
                Resolution = Resolution,
                GameDirectory = GameDirectory,
                JavaPath = JavaPath,
                JVMArgs = JVMArgs,
                Memory = Memory,
                LauncherVisibility = LauncherVisibility
                
            };
        }
    }
}
