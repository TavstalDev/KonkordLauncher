using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.Instance;

/// <summary>
/// Represents the resolution of a display or window, defined by its width (X) and height (Y).
/// </summary>
[Serializable]
public class Resolution
{
    /// <summary>
    /// Gets or sets the width of the resolution in pixels.
    /// </summary>
    [JsonPropertyName("x"), JsonProperty("x")]
    public uint X { get; set; }

    /// <summary>
    /// Gets or sets the height of the resolution in pixels.
    /// </summary>
    [JsonPropertyName("y"), JsonProperty("y")]
    public uint Y { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Resolution"/> class with default values.
    /// </summary>
    public Resolution() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Resolution"/> class with specified width and height.
    /// </summary>
    /// <param name="x">The width of the resolution in pixels.</param>
    /// <param name="y">The height of the resolution in pixels.</param>
    public Resolution(uint x, uint y)
    {
        X = x;
        Y = y;
    }
}