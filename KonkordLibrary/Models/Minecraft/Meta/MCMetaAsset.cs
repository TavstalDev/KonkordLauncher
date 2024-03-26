using System.Text.Json.Serialization;

namespace KonkordLibrary.Models.Minecraft.Meta
{
    public class MCMetaAsset
    {
        [JsonPropertyName("id")]
        public string Id {  get; set; }
        [JsonPropertyName("sha1")]
        public string Sha1 { get; set; }
        [JsonPropertyName("size")]
        public int Size { get; set; }
        [JsonPropertyName("totalSize")]
        public int TotalSize { get; set; }
        [JsonPropertyName("url")]
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
