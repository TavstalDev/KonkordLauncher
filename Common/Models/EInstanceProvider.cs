namespace Tavstal.KonkordLauncher.Common.Models;

/// <summary>
/// Represents the different instance providers supported by the Konkord Launcher.
/// </summary>
public enum EInstanceProvider
{
    /// <summary>
    /// The Konkord instance provider.
    /// </summary>
    Konkord = 0,

    /// <summary>
    /// The Prism Launcher instance provider.
    /// </summary>
    PrismLauncher = 1,

    /// <summary>
    /// The Modrinth instance provider.
    /// </summary>
    Modrinth = 2,

    /// <summary>
    /// The CurseForge instance provider.
    /// </summary>
    CurseForge = 3
}