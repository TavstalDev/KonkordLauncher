using System.Text.Json.Serialization;

namespace Tavstal.KonkordLauncher.Core.Models.Microsoft;

/// <summary>
/// Represents the request body for accessing Minecraft.
/// </summary>
public class MinecraftAccessRequestBody
{
    /// <summary>
    /// Gets or sets the identity token used for authentication.
    /// </summary>
    [JsonPropertyName("identityToken")]
    public string IdentityToken { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to ensure legacy mode is enabled.
    /// </summary>
    [JsonPropertyName("ensureLegacyEnabled")] 
    public bool EnsureLegacyEnabled { get; set; }
}