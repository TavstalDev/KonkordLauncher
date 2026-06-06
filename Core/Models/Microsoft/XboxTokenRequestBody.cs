using System.Text.Json.Serialization;

namespace Tavstal.KonkordLauncher.Core.Models.Microsoft;

/// <summary>
/// Represents the request body for obtaining an Xbox token.
/// </summary>
public class XboxTokenRequestBody
{
    /// <summary>
    /// Gets or sets the type of token to obtain.
    /// </summary>
    [JsonPropertyName("TokenType")]
    public string TokenType { get; set; }

    /// <summary>
    /// Gets or sets the relying party for the token request.
    /// </summary>
    [JsonPropertyName("RelyingParty")]
    public string RelyingParty { get; set; }

    /// <summary>
    /// Gets or sets additional properties required for the token request.
    /// </summary>
    [JsonPropertyName("Properties")]
    public XboxTokenProperties Properties { get; set; }
}