using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi
{
    public class VersionManifest
    {
        [JsonPropertyName("latest"), JsonProperty("latest")]
        public VersionManifestLatest Latest {  get; set; }
        [JsonPropertyName("versions"), JsonProperty("versions")]
        public List<MinecraftVersion> Versions { get; set; }

        public VersionManifest() { }

        public VersionManifest(VersionManifestLatest latest, List<MinecraftVersion> versions)
        {
            Latest = latest;
            Versions = versions;
        }
    }
}
