using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.User
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

        public Cape() { }

        public Cape(string id, string state, string url, string alias)
        {
            Id = id;
            State = state;
            Url = url;
            Alias = alias;
        }
    }
}
