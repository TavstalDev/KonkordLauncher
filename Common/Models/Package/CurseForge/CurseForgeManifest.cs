using System.Text.Json.Serialization;

namespace Tavstal.KonkordLauncher.Common.Models.Package.CurseForge;

public class CurseForgeManifest
{
    [JsonPropertyName("author")]
    public string Author { get; set; }
    
    [JsonPropertyName("files")]
    public List<CurseForgeFile> Files { get; set; }

    [JsonPropertyName("manifestType")]
    public string ManifestType { get; set; } = "minecraftModpack";

    [JsonPropertyName("manifestVersion")]
    public int ManifestVersion { get; set; } = 1;   
    
    [JsonPropertyName("minecraft")]
    public CurseForgeMinecraft Minecraft { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("overrides")]
    public string Overrides { get; set; } = "overrides";
    
    [JsonPropertyName("version")]
    public string Version { get; set; }
}