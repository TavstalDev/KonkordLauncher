using System.Text.Json.Serialization;


namespace Tavstal.KonkordLauncher.Common.Models.Java;

/// <summary>
/// Represents a Java version with its major version, full version string, architecture, and installation path.
/// </summary>
public class JavaVersion
{
    /// <summary>
    /// Gets or sets the major version of Java.
    /// </summary>
    [JsonPropertyName("major")]
    public int Major { get; set; }

    /// <summary>
    /// Gets or sets the full version string of Java.
    /// </summary>
    [JsonPropertyName("version")]
    public required string Version { get; set; }

    /// <summary>
    /// Gets or sets the architecture of the Java installation (e.g., x86, x64).
    /// </summary>
    [JsonPropertyName("architecture")]
    public required string Architecture { get; set; }

    /// <summary>
    /// Gets or sets the file system path to the Java installation.
    /// </summary>
    [JsonPropertyName("path")]
    public required string Path { get; set; }
}