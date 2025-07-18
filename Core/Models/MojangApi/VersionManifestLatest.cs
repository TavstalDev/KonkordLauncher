using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi
{
    public class VersionManifestLatest
    {
        [JsonPropertyName("release"), JsonProperty("release")]
        public string Release { get; set; }
        [JsonPropertyName("snapshot"), JsonProperty("snapshot")]
        public string Snapshot { get; set; }

        public VersionManifestLatest() { }

        public VersionManifestLatest(string release, string snapshot)
        {
            Release = release;
            Snapshot = snapshot;
        }
    }
}
