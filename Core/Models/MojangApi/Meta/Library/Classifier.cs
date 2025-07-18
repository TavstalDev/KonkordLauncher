using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta.Library;

/// <summary>
/// Represents a classifier for native libraries, including platform-specific artifacts for Windows, macOS, and Linux.
/// </summary>
public class Classifier
{
    /// <summary>
    /// Gets or sets the artifact for Windows native libraries.
    /// </summary>
    [JsonPropertyName("natives-windows"), JsonProperty("natives-windows")]
    public Artifact WindowsNatives { get; set; }

    /// <summary>
    /// Gets or sets the artifact for macOS native libraries.
    /// </summary>
    [JsonPropertyName("natives-osx"), JsonProperty("natives-osx")]
    public Artifact OsxNatives { get; set; }

    /// <summary>
    /// Gets or sets the artifact for Linux native libraries.
    /// </summary>
    [JsonPropertyName("natives-linux"), JsonProperty("natives-linux")]
    public Artifact LinuxNatives { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Classifier"/> class.
    /// </summary>
    public Classifier()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Classifier"/> class with specified platform-specific artifacts.
    /// </summary>
    /// <param name="windowsNatives">The artifact for Windows native libraries.</param>
    /// <param name="osxNatives">The artifact for macOS native libraries.</param>
    /// <param name="linuxNatives">The artifact for Linux native libraries.</param>
    public Classifier(Artifact windowsNatives, Artifact osxNatives, Artifact linuxNatives)
    {
        WindowsNatives = windowsNatives;
        OsxNatives = osxNatives;
        LinuxNatives = linuxNatives;
    }
}