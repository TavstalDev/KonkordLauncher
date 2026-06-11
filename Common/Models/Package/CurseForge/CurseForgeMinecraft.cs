using System.Text.Json.Serialization;

namespace Tavstal.KonkordLauncher.Common.Models.Package.CurseForge;

public class CurseForgeMinecraft
{
    [JsonPropertyName("modLoaders")]
    public List<CurseForgeModLoader>? ModLoaders { get; set; }
    
    [JsonPropertyName("version")]
    public string Version { get; set; }
}