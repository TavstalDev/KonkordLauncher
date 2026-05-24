
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.User;

public class Skin
{
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("state")]
    public string State { get; set; }
    [JsonProperty("url")]
    public string Url { get; set; }
    [JsonProperty("variant")]
    public string Variant { get; set; }
    [JsonProperty("alias")]
    public string? Alias { get; set; }

    public Skin() { }
    public Skin(string id, string state, string url, string variant, string? alias)
    {
        Id = id;
        State = state;
        Url = url;
        Variant = variant;
        Alias = alias;
    }
}