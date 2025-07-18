using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.User
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
        public List<Cape> Capes { get; set; }

        public MojangProfile() { }

        public MojangProfile(string id, string name, List<Skin> skins, List<Cape> capes)
        {
            Id = id;
            Name = name;
            Skins = skins;
            Capes = capes;
        }
    }
}
