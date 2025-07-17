using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.Minecraft.Library
{
    public class MCLibraryArtifact
    {
        [JsonPropertyName("path"), JsonProperty("path")]
        public string Path { get; set; }
        [JsonPropertyName("sha1"), JsonProperty("sha1")]
        public string Sha1 { get; set; }
        [JsonPropertyName("size"), JsonProperty("size")]
        public int Size { get; set; }
        [JsonPropertyName("url"), JsonProperty("url")]
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
