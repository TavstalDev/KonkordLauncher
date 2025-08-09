using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Enums;

namespace Tavstal.KonkordLauncher.Common.Models;

/// <summary>
/// Represents a Java mirror configuration, including version, URL, operating system, and architecture details.
/// </summary>
public class JavaMirror
{
    /// <summary>
    /// Gets or sets the major version of the Java runtime.
    /// </summary>
    [JsonPropertyName("majorVersion"), JsonProperty("majorVersion")]
    public int MajorVersion { get; set; }

    /// <summary>
    /// Gets or sets the URL of the Java mirror.
    /// </summary>
    [JsonPropertyName("mirrorUrl"), JsonProperty("mirrorUrl")]
    public string MirrorUrl { get; set; }

    /// <summary>
    /// Gets or sets the operating system for which the Java runtime is intended.
    /// </summary>
    [JsonPropertyName("operatingSystem"), JsonProperty("operatingSystem")]
    public EOperatingSystem OperatingSystem { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the Java runtime is ARM-based.
    /// </summary>
    [JsonPropertyName("isArmBased"), JsonProperty("isArmBased")]
    public bool IsArmBased { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaMirror"/> class.
    /// </summary>
    public JavaMirror() {}

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaMirror"/> class with the specified parameters.
    /// </summary>
    /// <param name="majorVersion">The major version of the Java runtime.</param>
    /// <param name="mirrorUrl">The URL of the Java mirror.</param>
    /// <param name="operatingSystem">The operating system for the Java runtime.</param>
    /// <param name="isArmBased">Indicates whether the Java runtime is ARM-based. Defaults to <c>false</c>.</param>
    public JavaMirror(int majorVersion, string mirrorUrl, EOperatingSystem operatingSystem, bool isArmBased = false)
    {
        MajorVersion = majorVersion;
        MirrorUrl = mirrorUrl;
        OperatingSystem = operatingSystem;
        IsArmBased = isArmBased;
    }
}