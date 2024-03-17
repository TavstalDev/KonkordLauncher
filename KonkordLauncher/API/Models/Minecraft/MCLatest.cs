using System.Text.Json;
using System.Text.Json.Serialization;

namespace KonkordLauncher.API.Models.Minecraft
{
    public class MCLatest
    {
        [JsonPropertyName("release")]
        public string Release { get; set; }
        [JsonPropertyName("snapshot")]
        public string Snapshot { get; set; }

        public MCLatest() { }

        public MCLatest(string raw)
        {
            MCLatest? local = JsonSerializer.Deserialize<MCLatest>(raw);
            if (local == null)
                return;

            Release = local.Release;
            Snapshot = local.Snapshot;
        }

        public MCLatest(string release, string snapshot)
        {
            Release = release;
            Snapshot = snapshot;
        }

        public static MCLatest? FromJson(string json)
        {
            return JsonSerializer.Deserialize<MCLatest>(json);
        }
    }
}
