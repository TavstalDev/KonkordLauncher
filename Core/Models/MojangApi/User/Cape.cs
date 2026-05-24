
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.User;

public class Cape
{
    [JsonProperty("id")]
    public string Id {  get; set; }
    [JsonProperty("state")]
    public string State {  get; set; }
    [JsonProperty("url")]
    public string Url {  get; set; }
    [JsonProperty("alias")]
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