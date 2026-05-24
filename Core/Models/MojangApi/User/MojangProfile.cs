
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.User;

public class MojangProfile
{
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("skins")]
    public List<Skin> Skins {  get; set; }
    [JsonProperty("capes")]
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