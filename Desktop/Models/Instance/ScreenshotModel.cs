using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Core.Helpers.IO;

namespace Tavstal.KonkordLauncher.Desktop.Models.Instance;

/// <summary>
/// Represents a model for a screenshot, including its name, extension, path, image, and size.
/// </summary>
public partial class ScreenshotModel : ObservableObject
{
    /// <summary>
    /// The name of the screenshot file without its extension.
    /// </summary>
    [ObservableProperty]
    public partial string Name { get; set; }

    /// <summary>
    /// The file extension of the screenshot (e.g., .png, .jpg).
    /// </summary>
    [ObservableProperty]
    public partial string Extension { get; set; }

    /// <summary>
    /// The full file path of the screenshot.
    /// </summary>
    [ObservableProperty]
    public partial string Path { get; set; }

    /// <summary>
    /// The bitmap image representation of the screenshot.
    /// </summary>
    [ObservableProperty]
    public partial Bitmap? Image { get; set; }

    /// <summary>
    /// The size of the screenshot file in bytes.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormatedSize))]
    public partial long Size { get; set; }

    /// <summary>
    /// Gets the formatted size of the screenshot file as a human-readable string.
    /// </summary>
    public string FormatedSize => FileSystemHelper.GetFormattedSize(Size);

    /// <summary>
    /// Initializes a new instance of the <see cref="ScreenshotModel"/> class.
    /// </summary>
    /// <param name="path">The full file path of the screenshot.</param>
    /// <param name="image">The bitmap image representation of the screenshot.</param>
    /// <param name="size">The size of the screenshot file in bytes.</param>
    public ScreenshotModel(string path, Bitmap? image, long size)
    {
        var fileName = System.IO.Path.GetFileName(path);
        string extension = System.IO.Path.GetExtension(fileName);
        if (fileName.Contains(extension))
            fileName = fileName.Replace(extension, string.Empty);

        Name = fileName;
        Extension = extension;
        Path = path;
        Image = image;
        Size = size;
    }
}