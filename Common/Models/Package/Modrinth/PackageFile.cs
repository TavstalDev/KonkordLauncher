using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Common.Models.Package.Modrinth;

public class PackageFile
{
    [JsonProperty("path"), JsonPropertyName("path")]
    public string Path { get; set; }
    
    [JsonProperty("hashes"), JsonPropertyName("hashes")]
    public Dictionary<string, string> Hashes { get; set; }
    
    [JsonProperty("env"), JsonPropertyName("env")]
    public Dictionary<string, string> Env { get; set; }
    
    [JsonProperty("downloads"), JsonPropertyName("downloads")]
    public List<string> Downloads { get; set; }
    
    [JsonProperty("fileSize"), JsonPropertyName("fileSize")]
    public long FileSize { get; set; }
}