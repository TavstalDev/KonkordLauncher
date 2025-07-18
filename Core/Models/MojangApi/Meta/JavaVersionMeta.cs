using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;

/// <summary>
/// Represents metadata for a specific Java version required by Minecraft.
/// </summary>
public class JavaVersionMeta
{
    /// <summary>
    /// Gets or sets the component name of the Java version.
    /// </summary>
    [JsonPropertyName("component"), JsonProperty("component")]
    public string Component { get; set; }

    /// <summary>
    /// Gets or sets the major version number of the Java version.
    /// </summary>
    [JsonPropertyName("majorVersion"), JsonProperty("majorVersion")]
    public int MajorVersion { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaVersionMeta"/> class with specified component and major version.
    /// </summary>
    /// <param name="component">The component name of the Java version.</param>
    /// <param name="majorVersion">The major version number of the Java version.</param>
    public JavaVersionMeta(string component, int majorVersion)
    {
        Component = component;
        MajorVersion = majorVersion;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaVersionMeta"/> class.
    /// </summary>
    public JavaVersionMeta()
    {
    }
}