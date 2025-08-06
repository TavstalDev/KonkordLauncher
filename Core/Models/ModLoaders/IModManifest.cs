namespace Tavstal.KonkordLauncher.Core.Models.ModLoaders;

/// <summary>
/// Defines the structure for a mod manifest, including version and game version details.
/// </summary>
public interface IModManifest
{
    /// <summary>
    /// Gets or sets the version of the mod loader.
    /// </summary>
    string Version { get; set; }

    /// <summary>
    /// Gets or sets the game version associated with the mod loader.
    /// </summary>
    string GameVersion { get; set; }
}