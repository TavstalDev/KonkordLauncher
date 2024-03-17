using KonkordLauncher.API.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KonkordLauncher.API.Models.Minecraft
{
    public class MCVersion
    {
        [JsonPropertyName("id")]
        public string Id {  get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("url")]
        public string Url {  get; set; }
        [JsonPropertyName("time")]
        public DateTime Time { get; set; }
        [JsonPropertyName("releaseTime")]
        public DateTime ReleaseTime {  get; set; }

        public EVersionType GetVersionType()
        {
            switch (Type)
            {
                case "snapshot":
                    {
                        return EVersionType.SNAPSHOT;
                    }
                case "old_alpha":
                    {
                        return EVersionType.OLD_ALPHA;
                    }
                case "old_beta":
                    {
                        return EVersionType.OLD_BETA;
                    }
                default:
                case "release":
                    {
                        return EVersionType.RELEASE;
                    }
            }
        }

        public MCVersion() { }

        public MCVersion(string raw)
        {
            MCVersion? local = JsonSerializer.Deserialize<MCVersion>(raw);
            if (local == null)
                return;

            Id = local.Id;
            Type = local.Type;
            Url = local.Url;
            Time = local.Time;
            ReleaseTime = local.ReleaseTime;
        }

        public MCVersion(string id, string type, string url, DateTime time, DateTime releaseTime) : this(id)
        {
            Type = type;
            Url = url;
            Time = time;
            ReleaseTime = releaseTime;
        }

        public static MCVersion? FromJson(string json)
        {
            return JsonSerializer.Deserialize<MCVersion>(json);
        }
    }
}
