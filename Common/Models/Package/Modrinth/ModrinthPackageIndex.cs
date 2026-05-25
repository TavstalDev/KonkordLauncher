
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Common.Models.Package.Modrinth;

/// <summary>
/// Represents the Modrinth package index file for a modpack or instance export.
/// </summary>
public class ModrinthPackageIndex
{
    /// <summary>
    /// Gets or sets the target game identifier.
    /// </summary>
    [JsonProperty("game")]
    public string Game { get; set; } = "minecraft";

    /// <summary>
    /// Gets or sets the package format version.
    /// </summary>
    [JsonProperty("formatVersion")]
    public int FormatVersion { get; set; } = 1;
    
    /// <summary>
    /// Gets or sets the unique version identifier for this package.
    /// </summary>
    [JsonProperty("versionId")]
    public required string VersionId { get; set; }
    
    /// <summary>
    /// Gets or sets the display name of the package version.
    /// </summary>
    [JsonProperty("name")]
    public required string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the package summary or description.
    /// </summary>
    [JsonProperty("summary")]
    public required string Summary { get; set; }

    /// <summary>
    /// Gets or sets the dependency map for the package.
    /// </summary>
    [JsonProperty("dependencies")]
    public Dictionary<string, string> Dependencies { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of files included in the package.
    /// </summary>
    [JsonProperty("files")]
    public List<PackageFile> Files { get; set; } = [];
}