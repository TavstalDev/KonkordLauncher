using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Models.Minecraft.Library;
using Tavstal.KonkordLauncher.Core.Models.Minecraft.Meta;

namespace Tavstal.KonkordLauncher.Core.Models.Forge.New
{
    public class ForgeVersionMeta
    {
        [JsonPropertyName("arguments"), JsonProperty("arguments")]
        public MCMetaArgument Arguments { get; set; }
        [JsonPropertyName("id"), JsonProperty("id")]
        public string Id { get; set; }
        [JsonPropertyName("inheritsFrom"), JsonProperty("inheritsFrom")]
        public string InheritsFrom { get; set; }
        [JsonPropertyName("libraries"), JsonProperty("libraries")]
        public List<MCLibrary> Libraries { get; set; }
        [JsonPropertyName("logging"), JsonProperty("logging")]
        public MCLogging Logging { get; set; }
        [JsonPropertyName("mainClass"), JsonProperty("mainClass")]
        public string MainClass { get; set; }
        [JsonPropertyName("type"), JsonProperty("type")]
        public string Type { get; set; }
    }
}
