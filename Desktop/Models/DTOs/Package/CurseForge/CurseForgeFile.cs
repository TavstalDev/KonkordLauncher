using System.Text.Json.Serialization;

namespace Tavstal.KonkordLauncher.Desktop.Models.DTOs.Package.CurseForge;

public class CurseForgeFile
{
    [JsonPropertyName("fileID")] public ulong FileId { get; set; }

    [JsonPropertyName("projectID")] public ulong ProjectId { get; set; }

    [JsonPropertyName("required")] public bool Required { get; set; }
}
