namespace Tavstal.KonkordLauncher.Common.Models;

/// <summary>
/// Represents the different instance providers supported by the Konkord Launcher.
/// </summary>
public enum EInstanceProvider
{
    /// <summary>
    /// The Prism Launcher instance provider.
    /// </summary>
    PrismLauncher = 0,

    /// <summary>
    /// The Modrinth instance provider.
    /// </summary>
    Modrinth = 1,

    /// <summary>
    /// The CurseForge instance provider.
    /// </summary>
    CurseForge = 2
}