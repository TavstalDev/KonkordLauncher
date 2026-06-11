using Tavstal.KonkordLauncher.Core.Enums;

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
    
    /// <summary>
    /// Gets the kind of mod loader this manifest represents.
    /// </summary>
    EMinecraftKind LoaderKind { get; }

    /// <summary>
    /// Determines whether the provided game version is compatible with this mod manifest's game version.
    /// </summary>
    /// <param name="gameVersion">The game version to compare against the manifest's game version.</param>
    /// <returns>True if the provided game version is considered compatible; otherwise, false.</returns>
    public abstract bool EqualsGameVersion(string gameVersion);
}