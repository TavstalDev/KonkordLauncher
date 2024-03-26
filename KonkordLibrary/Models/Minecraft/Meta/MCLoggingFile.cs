using System.Text.Json.Serialization;

namespace KonkordLibrary.Models.Minecraft.Meta
{
    public class MCLoggingFile
    {
        [JsonPropertyName("sha1")]
        public string Sha1 { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("size")]
        public int Size { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }

        public MCLoggingFile() { }

        public MCLoggingFile(string id, string sha1, string url, int size)
        {
            Id = id;
            Sha1 = sha1;
            Url = url;
            Size = size;
        }
    }
}
