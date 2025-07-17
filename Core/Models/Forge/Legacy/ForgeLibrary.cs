using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.Forge.Legacy
{
    public class ForgeLibrary
    {
        [JsonPropertyName("name"), JsonProperty("name")]
        public string Name { get; set; }
        [JsonPropertyName("url"), JsonProperty("url")]
        public string? Url { get; set; }
        [JsonPropertyName("clientreq"), JsonProperty("clientreq")]
        public bool? ClientRequires {  get; set; }
        [JsonPropertyName("serverreq"), JsonProperty("serverreq")]
        public bool? ServerRequires {  get; set; }
        [JsonPropertyName("checksums"), JsonProperty("checksums")]
        public List<string>? Checksums {  get; set; }

        public string? GetUrl(bool isLegacy = false)
        {
            if (Url == null)
            {
                if (isLegacy)
                    Url = "https://libraries.minecraft.net/";
                else
                    return null;
            }

            string[] rawUrl = Name.Split(':');

            return Path.Combine(Url, rawUrl[0].Replace('.', '/'), rawUrl[1], rawUrl[2], $"{rawUrl[1]}-{rawUrl[2]}.jar").Replace("\\", "/");
        }

        public string GetPath()
        {
            string[] rawUrl = Name.Split(':');

            return Path.Combine(rawUrl[0].Replace('.', '\\'), rawUrl[1], rawUrl[2], $"{rawUrl[1]}-{rawUrl[2]}.jar");
        }
    }
}
