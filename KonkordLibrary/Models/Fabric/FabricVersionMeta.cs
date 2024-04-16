using Newtonsoft.Json;
using Tavstal.KonkordLibrary.Models.Minecraft.Meta;
using System.Text.Json.Serialization;

namespace Tavstal.KonkordLibrary.Models.Fabric
{
    public class FabricVersionMeta
    {
        [JsonPropertyName("arguments"), JsonProperty("arguments")]
        public MCMetaArgument Arguments { get; set; }
        [JsonPropertyName("id"), JsonProperty("id")]
        public string Id { get; set; }
        [JsonPropertyName("inheritsFrom"), JsonProperty("inheritsFrom")]
        public string InheritsFrom { get; set; }
        [JsonPropertyName("libraries"), JsonProperty("libraries")]
        public List<FabricLibrary> Libraries { get; set; }
        [JsonPropertyName("mainClass"), JsonProperty("mainClass")]
        public string MainClass { get; set; }
        [JsonPropertyName("type"), JsonProperty("type")]
        public string Type { get; set; }
    }
}
