using Avalonia;
using Tavstal.BeaconSDK.Models.Instance;

namespace Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;

public interface IScreenService
{
    PixelSize ScreenSize { get; set; }

    Resolution GetResolution();
}
