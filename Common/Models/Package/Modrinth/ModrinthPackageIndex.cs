using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Common.Models.Package.Modrinth;

public class ModrinthPackageIndex
{
    [JsonProperty("game"), JsonPropertyName("game")]
    public string Game { get; set; } = "minecraft";

    [JsonProperty("formatVersion"), JsonPropertyName("formatVersion")]
    public int FormatVersion { get; set; } = 1;
    
    [JsonProperty("versionId"), JsonPropertyName("versionId")]
    public string VersionId { get; set; }
    
    [JsonProperty("name"), JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonProperty("summary"), JsonPropertyName("summary")]
    public string Summary { get; set; }

    [JsonProperty("dependencies"), JsonPropertyName("dependencies")]
    public Dictionary<string, string> Dependencies { get; set; } = new();

    [JsonProperty("files"), JsonPropertyName("files")]
    public List<PackageFile> Files { get; set; } = [];

}