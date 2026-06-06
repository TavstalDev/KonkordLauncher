using System.Text.Json.Serialization;

namespace Tavstal.KonkordLauncher.Core.Models.Microsoft;

/// <summary>
/// Represents the request body for Xbox XSTS (Xbox Single Sign-On Token Service).
/// </summary>
public class XboxXstsRequestBody
{
    /// <summary>
    /// Gets or sets the type of token requested.
    /// </summary>
    [JsonPropertyName("TokenType")]
    public string TokenType { get; set; }

    /// <summary>
    /// Gets or sets the relying party for which the token is being requested.
    /// </summary>
    [JsonPropertyName("RelyingParty")]
    public string RelyingParty { get; set; }

    /// <summary>
    /// Gets or sets additional properties required for the XSTS request.
    /// </summary>
    [JsonPropertyName("Properties")]
    public XboxXstsProperties Properties { get; set; }
}