using System.Text.Json.Serialization;

namespace Tavstal.KonkordLauncher.Desktop.Models.DTOs.Package.CurseForge;

public class CurseForgeModLoader
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;

    [JsonPropertyName("primary")] public bool IsPrimary { get; set; }
}
