using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace KonkordLibrary.Models.Minecraft.API
{
    public class Skin
    {
        [JsonProperty("id"), JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonProperty("state"), JsonPropertyName("state")]
        public string State { get; set; }
        [JsonProperty("url"), JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonProperty("variant"), JsonPropertyName("variant")]
        public string Variant { get; set; }
        [JsonProperty("alias"), JsonPropertyName("alias")]
        public string Alias { get; set; }

        public Skin() { }
        public Skin(string id, string state, string url, string variant, string alias)
        {
            Id = id;
            State = state;
            Url = url;
            Variant = variant;
            Alias = alias;
        }
    }
}
