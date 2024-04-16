using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;

namespace Tavstal.KonkordLibrary.Models.Minecraft.API
{
    public class MojangProfile
    {
        [JsonProperty("id"), JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonProperty("name"), JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonProperty("skins"), JsonPropertyName("skins")]
        public List<Skin> Skins {  get; set; }
        [JsonProperty("capes"), JsonPropertyName("capes")]
        public JArray Capes { get; set; } // TODO, find its model

        public MojangProfile() { }

        public MojangProfile(string id, string name, List<Skin> skins, JArray capes)
        {
            Id = id;
            Name = name;
            Skins = skins;
            Capes = capes;
        }
    }
}
