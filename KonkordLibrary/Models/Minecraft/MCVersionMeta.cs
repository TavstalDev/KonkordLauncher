using KonkordLibrary.Models.Minecraft.Library;
using KonkordLibrary.Models.Minecraft.Meta;
using System.Text.Json.Serialization;

namespace KonkordLibrary.Models.Minecraft
{
    public class MCVersionMeta
    {
        [JsonPropertyName("arguments")]
        public MCMetaArgument Arguments { get; set; }

        [JsonPropertyName("assetIndex")]
        public MCMetaAsset AssetIndex { get; set; }

        [JsonPropertyName("assets")]
        public string Assets { get; set; }

        [JsonPropertyName("complianceLevel")]
        public int ComplianceLevel { get; set; }

        [JsonPropertyName("downloads")]
        public MCMetaDownloads Downloads { get; set; }

        [JsonPropertyName("id")]
        public string Id {  get; set; }

        [JsonPropertyName("javaVersion")]
        public JavaVersion JavaVersion { get; set; }
        [JsonPropertyName("libraries")]
        public List<MCLibrary> Libraries { get; set; }
        [JsonPropertyName("logging")]
        public MCLogging Logging {  get; set; }
        [JsonPropertyName("mainClass")]
        public string MainClass { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
