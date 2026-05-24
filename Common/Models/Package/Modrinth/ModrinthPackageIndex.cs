
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Common.Models.Package.Modrinth;

public class ModrinthPackageIndex
{
    [JsonProperty("game")]
    public string Game { get; set; } = "minecraft";

    [JsonProperty("formatVersion")]
    public int FormatVersion { get; set; } = 1;
    
    [JsonProperty("versionId")]
    public string VersionId { get; set; }
    
    [JsonProperty("name")]
    public string Name { get; set; }
    
    [JsonProperty("summary")]
    public string Summary { get; set; }

    [JsonProperty("dependencies")]
    public Dictionary<string, string> Dependencies { get; set; } = new();

    [JsonProperty("files")]
    public List<PackageFile> Files { get; set; } = [];

}