using KonkordLibrary.Models.Minecraft.Library;
using KonkordLibrary.Models.Minecraft.Meta;
using System.Text.Json.Serialization;

namespace KonkordLibrary.Models.Forge
{
    public class ForgeVersionMeta
    {
        [JsonPropertyName("arguments")]
        public MCMetaArgument Arguments { get; set; }
        [JsonPropertyName("id")]
        public string Id {  get; set; }
        [JsonPropertyName("inheritsFrom")]
        public string InheritsFrom { get; set; }
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
