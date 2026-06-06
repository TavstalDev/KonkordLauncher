using System.Text.Json.Serialization;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.Requests;

/// <summary>
/// Represents the request body for showing a cape.
/// </summary>
public class ShowCapeRequestBody
{
    /// <summary>
    /// Gets or sets the ID of the cape to show.
    /// </summary>
    [JsonPropertyName("capeId")]
    public string CapeId { get; set; }
}