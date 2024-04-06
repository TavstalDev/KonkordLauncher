using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace KonkordLibrary.Models.Minecraft
{
    public class VersionManifest
    {
        [JsonPropertyName("latest"), JsonProperty("latest")]
        public MCLatest Latest {  get; set; }
        [JsonPropertyName("versions"), JsonProperty("versions")]
        public List<MCVersion> Versions { get; set; }

        public VersionManifest() { }

        public VersionManifest(MCLatest latest, List<MCVersion> versions)
        {
            Latest = latest;
            Versions = versions;
        }
    }
}
