using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;

namespace Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge.New
{
    public class ForgeVersionProfile
    {
        [JsonPropertyName("spec"), JsonProperty("spec")]
        public int Spec { get; set; }
        [JsonPropertyName("profile"), JsonProperty("profile")]
        public string Profile { get; set; }
        [JsonPropertyName("version"), JsonProperty("version")]
        public string Version { get; set; }
        [JsonPropertyName("path"), JsonProperty("path")]
        public string? Path { get; set; }
        [JsonPropertyName("minecraft"), JsonProperty("minecraft")]
        public string Minecraft { get; set; }
        [JsonPropertyName("serverJarPath"), JsonProperty("serverJarPath")]
        public string ServerJarPath { get; set; }
        [JsonPropertyName("data"), JsonProperty("data")]
        public JObject Data { get; set; }
        [JsonPropertyName("processors"), JsonProperty("processors")]
        public JArray Processors { get; set; }
        [JsonPropertyName("libraries"), JsonProperty("libraries")]
        public List<LibraryMeta> Libraries { get; set; }
    }
}
