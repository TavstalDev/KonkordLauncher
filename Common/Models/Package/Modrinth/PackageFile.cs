
using System.Text.Json.Serialization;


namespace Tavstal.KonkordLauncher.Common.Models.Package.Modrinth;


/// <summary>
/// Represents a single file entry inside a Modrinth package.
/// </summary>
public class PackageFile
{
    /// <summary>
    /// Gets or sets the relative path of the file in the package.
    /// </summary>
    [JsonPropertyName("path")]
    public required string Path { get; set; }
    
    /// <summary>
    /// Gets or sets the hash map for the file.
    /// </summary>
    [JsonPropertyName("hashes")]
    public Dictionary<string, string> Hashes { get; set; } = new();

    /// <summary>
    /// Gets or sets the environment flags for the file.
    /// </summary>
    [JsonPropertyName("env")]
    public Dictionary<string, string> Env { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of download URLs for the file.
    /// </summary>
    [JsonPropertyName("downloads")]
    public List<string> Downloads { get; set; } = [];
    
    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    [JsonPropertyName("fileSize")]
    public long FileSize { get; set; }
}