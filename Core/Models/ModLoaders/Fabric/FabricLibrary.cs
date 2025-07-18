using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.ModLoaders.Fabric
{
    public class FabricLibrary
    {
        [JsonPropertyName("name"), JsonProperty("name")]
        public string Name { get; set; }
        [JsonPropertyName("url"), JsonProperty("url")]
        public string Url { get; set; }
        [JsonPropertyName("md5"), JsonProperty("md5")]
        public string Md5 { get; set; }
        [JsonPropertyName("sha1"), JsonProperty("sha1")]
        public string Sha1 { get; set; }
        [JsonPropertyName("sha256"), JsonProperty("sha256")]
        public string Sha256 { get; set; }
        [JsonPropertyName("sha512"), JsonProperty("sha512")]
        public string Sha512 { get; set; }
        [JsonPropertyName("size"), JsonProperty("size")]
        public int Size { get; set; }

        public FabricLibrary() { }


        public string GetURL()
        {
            string path;
            string[] parts = this.Name.Split(":", 3);
            path = parts[0].Replace(".", "/") + "/" + parts[1] + "/" + parts[2] + "/" + parts[1] + "-" + parts[2] + ".jar";

            return Url + path;
        }

        public string GetPath()
        {
            string[] parts = this.Name.Split(":", 3);
            char separator = '/';
            string path = parts[0].Replace('.', separator) + separator + parts[1] + separator + parts[2] + separator + parts[1] + "-" + parts[2] + ".jar";
            return path.Replace(" ", "_");
        }
    }
}
