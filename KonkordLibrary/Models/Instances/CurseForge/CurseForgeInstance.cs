using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace KonkordLibrary.Models.Instances.CurseForge
{
    public class CurseForgeInstance
    {
        [JsonProperty("minecraft"), JsonPropertyName("minecraft")]
        public CurseMinecraft Minecraft { get; set; }
        [JsonProperty("manifestType"), JsonPropertyName("manifestType")]
        public string ManifestType { get; set; }
        [JsonProperty("manifestVersion"), JsonPropertyName("manifestVersion")]
        public int ManifestVersion { get; set; }
        [JsonProperty("name"), JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonProperty("version"), JsonPropertyName("version")]
        public string Version { get; set; }
        [JsonProperty("author"), JsonPropertyName("author")]
        public string Author { get; set; }
        [JsonProperty("files"), JsonPropertyName("files")]
        public List<CurseFile> Files { get; set; }
        [JsonProperty("overrides"), JsonPropertyName("overrides")]
        public string Overrides { get; set; }

        public CurseForgeInstance(CurseMinecraft minecraft, string manifestType, int manifestVersion, string name, string version, string author, List<CurseFile> files, string overrides)
        {
            Minecraft = minecraft;
            ManifestType = manifestType;
            ManifestVersion = manifestVersion;
            Name = name;
            Version = version;
            Author = author;
            Files = files;
            Overrides = overrides;
        }
    }
}
