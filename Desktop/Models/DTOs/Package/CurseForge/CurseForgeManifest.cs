using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tavstal.KonkordLauncher.Desktop.Models.DTOs.Package.CurseForge;

public class CurseForgeManifest
{
    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    [JsonPropertyName("files")]
    public List<CurseForgeFile> Files { get; set; } = [];

    [JsonPropertyName("manifestType")] public string ManifestType { get; set; } = "minecraftModpack";

    [JsonPropertyName("manifestVersion")] public int ManifestVersion { get; set; } = 1;

    [JsonPropertyName("minecraft")] public CurseForgeMinecraft? Minecraft { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    [JsonPropertyName("overrides")] public string Overrides { get; set; } = "overrides";

    [JsonPropertyName("version")] public string Version { get; set; } = string.Empty;
}
