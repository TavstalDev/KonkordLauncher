using Avalonia;
using Tavstal.BeaconSDK.Models.Instance;

namespace Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;

/// <summary>
///     Provides the current screen size.
/// </summary>
public interface IScreenService
{
    /// <summary>
    ///     Gets or sets the current screen size in pixels.
    /// </summary>
    PixelSize ScreenSize { get; set; }

    /// <summary>
    ///     Gets the screen size as a resolution.
    /// </summary>
    /// <returns>The current screen resolution.</returns>
    Resolution GetResolution();
}
