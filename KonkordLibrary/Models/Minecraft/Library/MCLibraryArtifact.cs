using System.Text.Json.Serialization;

namespace KonkordLibrary.Models.Minecraft.Library
{
    public class MCLibraryArtifact
    {
        [JsonPropertyName("path")]
        public string Path { get; set; }
        [JsonPropertyName("sha1")]
        public string Sha1 { get; set; }
        [JsonPropertyName("size")]
        public int Size { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }

        public MCLibraryArtifact() { }

        public MCLibraryArtifact(string path, string sha1, int size, string url)
        {
            Path = path;
            Sha1 = sha1;
            Size = size;
            Url = url;
        }
    }
}
