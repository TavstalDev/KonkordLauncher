using System.Text.Json.Serialization;

namespace KonkordLibrary.Models.Minecraft.Meta
{
    public class MCMetaDownload
    {
        [JsonPropertyName("sha1")]
        public string Sha1 { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("size")]
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
