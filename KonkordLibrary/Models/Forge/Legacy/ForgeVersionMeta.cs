using KonkordLibrary.Models.Minecraft.Library;
using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using KonkordLibrary.Models.Minecraft.Meta;

namespace KonkordLibrary.Models.Forge.Legacy
{
    public class ForgeVersionMeta
    {
        [JsonPropertyName("id"), JsonProperty("id")]
        public string Id { get; set; }
        [JsonPropertyName("type"), JsonProperty("type")]
        public string Type { get; set; }
        [JsonPropertyName("minecraftArguments"), JsonProperty("minecraftArguments")]
        public string MinecraftArguments { get; set; }
        [JsonPropertyName("mainClass"), JsonProperty("mainClass")]
        public string MainClass { get; set; }
        [JsonPropertyName("inheritsFrom"), JsonProperty("inheritsFrom")]
        public string InheritsFrom { get; set; }
        [JsonPropertyName("jar"), JsonProperty("jar")]
        public string Jar { get; set; }
        [JsonPropertyName("logging"), JsonProperty("logging")]
        public MCLogging? Logging { get; set; }
        [JsonPropertyName("libraries"), JsonProperty("libraries")]
        public List<ForgeLibrary> Libraries { get; set; }
    }
}
