using System.Text.Json.Serialization;

namespace Tavstal.KonkordLauncher.Core.Models.Microsoft;


/// <summary>
/// Represents properties required for Xbox token authentication.
/// </summary>
public class XboxTokenProperties
{
    /// <summary>
    /// Gets or sets the authentication method used.
    /// </summary>
    [JsonPropertyName("AuthMethod")]
    public string AuthMethod { get; set; }
    
    /// <summary>
    /// Gets or sets the site name.
    /// </summary>
    [JsonPropertyName("SiteName")]
    public string SiteName { get; set; }
    
    /// <summary>
    /// Gets or sets the RPS ticket.
    /// </summary>
    [JsonPropertyName("RpsTicket")]
    public string RpsTicket { get; set; }
}