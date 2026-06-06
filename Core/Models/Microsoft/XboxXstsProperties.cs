using System.Text.Json.Serialization;

namespace Tavstal.KonkordLauncher.Core.Models.Microsoft;

/// <summary>
/// Represents properties for Xbox XSTS.
/// </summary>
public class XboxXstsProperties
{
    /// <summary>
    /// Gets or sets the sandbox ID.
    /// </summary>
    [JsonPropertyName("SandboxId")]
    public string SandboxId { get; set; }
    
    /// <summary>
    /// Gets or sets the user tokens.
    /// </summary>
    [JsonPropertyName("UserTokens")]
    public string[] UserTokens { get; set; }
}