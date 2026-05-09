namespace Tavstal.KonkordLauncher.Common.Models;

/// <summary>
/// Types of resources that can be associated with an instance (used for categorizing and handling
/// different kinds of installable or loadable assets).
/// </summary>
public enum EResourceType
{
    /// <summary>
    /// A resource pack containing textures, models or other client-side asset overrides.
    /// </summary>
    RESOURCE_PACK = 0,
    
    /// <summary>
    /// A mod that alters game logic, adds features or changes behavior (typically loaded by a mod loader).
    /// </summary>
    MOD = 1,
    
    /// <summary>
    /// A shader pack that modifies the rendering pipeline to change lighting, shadows and visual effects.
    /// </summary>
    SHADER_PACK = 2
}