namespace Tavstal.KonkordLauncher.Core.Enums;

/// <summary>
/// Represents the different kinds of Minecraft profiles supported by the launcher.
/// </summary>
public enum EMinecraftKind
{
    /// <summary>
    /// Represents the Vanilla version of Minecraft.
    /// </summary>
    VANILLA = 0,

    /// <summary>
    /// Represents the NeoForge modded version of Minecraft.
    /// </summary>
    NEOFORGE = 1,
    
    /// <summary>
    /// Represents the Forge modded version of Minecraft.
    /// </summary>
    FORGE = 2,

    /// <summary>
    /// Represents the Fabric modded version of Minecraft.
    /// </summary>
    FABRIC = 3,

    /// <summary>
    /// Represents the Quilt modded version of Minecraft.
    /// </summary>
    QUILT = 4
}