
using System.Text.Json.Serialization;


namespace Tavstal.KonkordLauncher.Common.Models.Package.Modrinth;

/// <summary>
/// Represents the Modrinth package index file for a modpack or instance export.
/// </summary>
public class ModrinthPackageIndex
{
    /// <summary>
    /// Gets or sets the target game identifier.
    /// </summary>
    [JsonPropertyName("game")]
    public string Game { get; set; } = "minecraft";

    /// <summary>
    /// Gets or sets the package format version.
    /// </summary>
    [JsonPropertyName("formatVersion")]
    public int FormatVersion { get; set; } = 1;
    
    /// <summary>
    /// Gets or sets the unique version identifier for this package.
    /// </summary>
    [JsonPropertyName("versionId")]
    public required string VersionId { get; set; }
    
    /// <summary>
    /// Gets or sets the display name of the package version.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the package summary or description.
    /// </summary>
    [JsonPropertyName("summary")]
    public required string Summary { get; set; }

    /// <summary>
    /// Gets or sets the dependency map for the package.
    /// </summary>
    [JsonPropertyName("dependencies")]
    public Dictionary<string, string> Dependencies { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of files included in the package.
    /// </summary>
    [JsonPropertyName("files")]
    public List<PackageFile> Files { get; set; } = [];
}