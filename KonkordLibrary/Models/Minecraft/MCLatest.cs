using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLibrary.Models.Minecraft
{
    public class MCLatest
    {
        [JsonPropertyName("release"), JsonProperty("release")]
        public string Release { get; set; }
        [JsonPropertyName("snapshot"), JsonProperty("snapshot")]
        public string Snapshot { get; set; }

        public MCLatest() { }

        public MCLatest(string release, string snapshot)
        {
            Release = release;
            Snapshot = snapshot;
        }
    }
}
