using Avalonia.Media.Imaging;
using Tavstal.KonkordLauncher.Core.Helpers;

namespace Tavstal.KonkordLauncher.Desktop.Models.Instance;

/// <summary>
/// Represents a screenshot model containing metadata and image data.
/// </summary>
public class ScreenshotModel
{
    /// <summary>
    /// Gets or sets the name of the screenshot.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the image of the screenshot as a bitmap.
    /// </summary>
    public Bitmap? Image { get; set; }

    /// <summary>
    /// Gets or sets the size of the screenshot in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Gets the formatted size of the screenshot as a human-readable string.
    /// </summary>
    public string FormatedSize => FileSystemHelper.GetFormatedSize(Size);
}