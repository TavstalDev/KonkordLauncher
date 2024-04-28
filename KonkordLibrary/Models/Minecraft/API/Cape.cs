using Newtonsoft.Json;
using System.Text.Json.Serialization;
using Tavstal.KonkordLibrary.Helpers;

namespace Tavstal.KonkordLibrary.Models.Minecraft.API
{
    public class Cape
    {
        [JsonPropertyName("id"), JsonProperty("id")]
        public string Id {  get; set; }
        [JsonPropertyName("state"), JsonProperty("state")]
        public string State {  get; set; }
        [JsonPropertyName("url"), JsonProperty("url")]
        public string Url {  get; set; }
        [JsonPropertyName("alias"), JsonProperty("alias")]
        public string Alias {  get; set; }
        [System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]
        public string Path { get; set; }

        public Cape() { }

        public Cape(string id, string state, string url, string alias)
        {
            Id = id;
            State = state;
            Url = url;
            Alias = alias;
            Path = GetPath();
        }

        public string GetPath()
        {
            if (Alias.ToLower() == "none")
                return "/assets/images/steve_full_nocape.png";
            else
                return System.IO.Path.Combine(IOHelper.CacheDir, "capes", $"{Alias}.png");
        }
    }
}
