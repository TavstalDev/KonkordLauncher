using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge.Legacy
{
    public class ForgeProfileInfo
    {
        [JsonPropertyName("profileName"), JsonProperty("profileName")]
        public string ProfileName { get; set; }
        [JsonPropertyName("target"), JsonProperty("target")]
        public string Target { get; set; }
        [JsonPropertyName("path"), JsonProperty("path")]
        public string Path { get; set; }
        [JsonPropertyName("version"), JsonProperty("version")]
        public string Version { get; set; }
        [JsonPropertyName("filePath"), JsonProperty("filePath")]
        public string FilePath { get; set; }
        [JsonPropertyName("minecraft"), JsonProperty("minecraft")]
        public string Minecraft { get; set; }
        [JsonPropertyName("mirrorList"), JsonProperty("mirrorList")]
        public string MirrorList { get; set; }
    }
}
