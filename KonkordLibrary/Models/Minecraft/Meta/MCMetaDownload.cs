using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace KonkordLibrary.Models.Minecraft.Meta
{
    public class MCMetaDownload
    {
        [JsonPropertyName("sha1"), JsonProperty("sha1")]
        public string Sha1 { get; set; }
        [JsonPropertyName("url"), JsonProperty("url")]
        public string Url { get; set; }
        [JsonPropertyName("size"), JsonProperty("size")]
        public int Size { get; set; }

        public MCMetaDownload() { }

        public MCMetaDownload(string sha1, string url, int size)
        {
            Sha1 = sha1;
            Url = url;
            Size = size;
        }
    }
}
