using System.Text.Json.Serialization;

namespace Tavstal.KonkordLauncher.Common.Models.Package.CurseForge;

public class CurseForgeModLoader
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("primary")]
    public bool IsPrimary { get; set; }
}