
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Common.Models.Package.Modrinth;

public class PackageFile
{
    [JsonProperty("path")]
    public string Path { get; set; }
    
    [JsonProperty("hashes")]
    public Dictionary<string, string> Hashes { get; set; } = new();

    [JsonProperty("env")]
    public Dictionary<string, string> Env { get; set; } = new();

    [JsonProperty("downloads")]
    public List<string> Downloads { get; set; } = [];
    
    [JsonProperty("fileSize")]
    public long FileSize { get; set; }
}