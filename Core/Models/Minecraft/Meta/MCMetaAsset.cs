using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.Minecraft.Meta
{
    public class MCMetaAsset
    {
        [JsonPropertyName("id"), JsonProperty("id")]
        public string Id {  get; set; }
        [JsonPropertyName("sha1"), JsonProperty("sha1")]
        public string Sha1 { get; set; }
        [JsonPropertyName("size"), JsonProperty("size")]
        public int Size { get; set; }
        [JsonPropertyName("totalSize"), JsonProperty("totalSize")]
        public int TotalSize { get; set; }
        [JsonPropertyName("url"), JsonProperty("url")]
        public string Url { get; set; }

        public MCMetaAsset() { }

        public MCMetaAsset(string id, string sha1, int size, int totalSize, string url)
        {
            Id = id;
            Sha1 = sha1;
            Size = size;
            TotalSize = totalSize;
            Url = url;
        }
    }
}
