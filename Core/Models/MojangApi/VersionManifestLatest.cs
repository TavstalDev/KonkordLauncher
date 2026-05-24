
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi;

public class VersionManifestLatest
{
    [JsonProperty("release")]
    public string Release { get; set; }
    [JsonProperty("snapshot")]
    public string Snapshot { get; set; }

    public VersionManifestLatest()
    {
        Release = string.Empty;
        Snapshot = string.Empty;
    }

    public VersionManifestLatest(string release, string snapshot)
    {
        Release = release;
        Snapshot = snapshot;
    }
}