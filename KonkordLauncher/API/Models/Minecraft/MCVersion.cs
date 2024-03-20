using KonkordLauncher.API.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;

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

        public EVersionType GetVersionBaseType() {
            switch (GetMCVersionType())
            {
                case EMCVersionType.RELEASE:
                    return EVersionType.RELEASE;
                case EMCVersionType.SNAPSHOT:
                    return EVersionType.SNAPSHOT;
                default:
                    return EVersionType.BETA;
            }
        }

        public EMCVersionType GetMCVersionType()
        {
            switch (Type)
            {
                case "snapshot":
                    {
                        return EMCVersionType.SNAPSHOT;
                    }
                case "old_alpha":
                    {
                        return EMCVersionType.OLD_ALPHA;
                    }
                case "old_beta":
                    {
                        return EMCVersionType.OLD_BETA;
                    }
                default:
                case "release":
                    {
                        return EMCVersionType.RELEASE;
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
