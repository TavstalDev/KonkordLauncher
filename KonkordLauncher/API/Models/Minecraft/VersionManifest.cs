using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace KonkordLauncher.API.Models.Minecraft
{
    public class VersionManifest
    {
        [JsonPropertyName("latest")]
        public MCLatest Latest {  get; set; }
        [JsonPropertyName("versions")]
        public List<MCVersion> Versions { get; set; }

        public VersionManifest() { }

        public VersionManifest(MCLatest latest, List<MCVersion> versions)
        {
            Latest = latest;
            Versions = versions;
        }

        public static VersionManifest? FromJson(string json)
        {
            return JsonSerializer.Deserialize<VersionManifest>(json);
        }
    }
}
