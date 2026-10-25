using DiscordRPC;

namespace Tavstal.KonkordLauncher.Desktop.Services.Presentation;

public interface IDiscordPresenceService
{
    void Update(string details, Timestamps? timestamps = null);

    void Clear();
}
