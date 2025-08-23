namespace Tavstal.KonkordLauncher.Core.Models.Endpoints;

/// <summary>
/// Provides endpoint URLs for accessing Konkord-related resources.
/// </summary>
public static class KonkordEndpoints
{
    /// <summary>
    /// The URL for retrieving the latest release of Konkord.
    /// </summary>
    public const string LatestRelease = "https://api.github.com/repos/TavstalDev/KonkordLauncher/releases/latest";

    /// <summary>
    /// The URL for retrieving all releases of Konkord.
    /// </summary>
    public const string AllReleases = "https://api.github.com/repos/TavstalDev/KonkordLauncher/releases";
}