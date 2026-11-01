using DiscordRPC;

namespace Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;

/// <summary>
///     Updates the Discord Rich Presence of the launcher.
/// </summary>
public interface IDiscordPresenceService
{
    /// <summary>
    ///     Sets the activity text that Discord shows for the launcher.
    /// </summary>
    /// <param name="details">The text shown under the launcher name.</param>
    /// <param name="timestamps">The optional start or end time.</param>
    void Update(string details, Timestamps? timestamps = null);

    /// <summary>
    ///     Clears the Discord activity.
    /// </summary>
    void Clear();
}
