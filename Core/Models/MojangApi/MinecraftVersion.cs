using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Enums;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi
{
    public class MinecraftVersion
    {
        [JsonPropertyName("id"), JsonProperty("id")]
        public string Id {  get; set; }
        [JsonPropertyName("type"), JsonProperty("type")]
        public string Type { get; set; }
        [JsonPropertyName("url"), JsonProperty("url")]
        public string Url {  get; set; }
        [JsonPropertyName("time"), JsonProperty("time")]
        public DateTime Time { get; set; }
        [JsonPropertyName("releaseTime"), JsonProperty("releaseTime")]
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

        public MinecraftVersion() { }

        public MinecraftVersion(string id, string type, string url, DateTime time, DateTime releaseTime)
        {
            Id = id;
            Type = type;
            Url = url;
            Time = time;
            ReleaseTime = releaseTime;
        }
    }
}
