using Avalonia;
using Tavstal.BeaconSDK.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;

namespace Tavstal.KonkordLauncher.Desktop.Services.Presentation;

/// <inheritdoc />
public class ScreenService : IScreenService
{
    /// <inheritdoc />
    public PixelSize ScreenSize { get; set; } = new(1920, 1080);

    /// <inheritdoc />
    public Resolution GetResolution()
    {
        return new Resolution((uint)ScreenSize.Width, (uint)ScreenSize.Height);
    }
}
