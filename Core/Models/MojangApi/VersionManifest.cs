
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi;

public class VersionManifest
{
    [JsonProperty("latest")]
    public VersionManifestLatest Latest {  get; set; }
    [JsonProperty("versions")]
    public List<MinecraftVersion> Versions { get; set; }

    public VersionManifest()
    {
        Latest = new VersionManifestLatest();
        Versions = [];
    }

    public VersionManifest(VersionManifestLatest latest, List<MinecraftVersion> versions)
    {
        Latest = latest;
        Versions = versions;
    }
}