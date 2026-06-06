using System.Text.Json.Serialization;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.Requests;

/// <summary>
/// Represents the request body for changing a skin.
/// </summary>
public class ChangeSkinRequestBody
{
    /// <summary>
    /// Gets or sets the variant of the skin.
    /// </summary>
    [JsonPropertyName("variant")]
    public string Variant { get; set; }

    /// <summary>
    /// Gets or sets the URL of the skin.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; }
}