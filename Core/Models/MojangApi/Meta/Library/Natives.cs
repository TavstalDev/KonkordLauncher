using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta.Library;

/// <summary>
/// Represents the native configurations for different operating systems.
/// </summary>
public class Natives
{
    /// <summary>
    /// Gets or sets the native configuration for Windows.
    /// </summary>
    [JsonPropertyName("windows"), JsonProperty("windows")]
    public string Windows { get; set; }

    /// <summary>
    /// Gets or sets the native configuration for macOS.
    /// </summary>
    [JsonPropertyName("osx"), JsonProperty("osx")]
    public string Osx { get; set; }

    /// <summary>
    /// Gets or sets the native configuration for Linux.
    /// </summary>
    [JsonPropertyName("linux"), JsonProperty("linux")]
    public string Linux { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Natives"/> class.
    /// </summary>
    public Natives() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Natives"/> class with specified configurations for Windows, macOS, and Linux.
    /// </summary>
    /// <param name="windows">The native configuration for Windows.</param>
    /// <param name="osx">The native configuration for macOS.</param>
    /// <param name="linux">The native configuration for Linux.</param>
    public Natives(string windows, string osx, string linux)
    {
        Windows = windows;
        Osx = osx;
        Linux = linux;
    }
}