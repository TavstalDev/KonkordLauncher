using DiscordRPC;

namespace Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;

public interface IDiscordPresenceService
{
    void Update(string details, Timestamps? timestamps = null);

    void Clear();
}
