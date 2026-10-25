using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tavstal.KonkordLauncher.Desktop.Models.DTOs.Package.CurseForge;

public class CurseForgeMinecraft
{
    [JsonPropertyName("modLoaders")] public List<CurseForgeModLoader>? ModLoaders { get; set; }

    [JsonPropertyName("version")] public string Version { get; set; }
}
