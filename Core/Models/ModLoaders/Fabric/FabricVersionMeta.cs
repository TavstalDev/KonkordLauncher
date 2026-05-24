
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;

namespace Tavstal.KonkordLauncher.Core.Models.ModLoaders.Fabric;

public class FabricVersionMeta
{
    [JsonProperty("arguments")]
    public ArgumentMeta Arguments { get; set; }
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("inheritsFrom")]
    public string InheritsFrom { get; set; }
    [JsonProperty("libraries")]
    public List<FabricLibrary> Libraries { get; set; }
    [JsonProperty("mainClass")]
    public string MainClass { get; set; }
    [JsonProperty("type")]
    public string Type { get; set; }
}