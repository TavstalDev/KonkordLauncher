using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;

namespace Tavstal.KonkordLauncher.Desktop.Models.Domain;

/// <summary>
///     An icon with a name, a file path, and an image.
/// </summary>
public partial class IconDataModel : ObservableObject
{
    /// <summary>
    ///     Creates an icon model.
    /// </summary>
    /// <param name="name">The icon name.</param>
    /// <param name="path">The icon file path.</param>
    /// <param name="image">The icon image.</param>
    public IconDataModel(string name, string path, BitmapEntry image)
    {
        Name = name;
        Path = path;
        Image = image;
    }

    /// <summary>
    ///     Gets or sets the icon name.
    /// </summary>
    [ObservableProperty]
    public partial string Name { get; set; }

    /// <summary>
    ///     Gets or sets the icon file path.
    /// </summary>
    [ObservableProperty]
    public partial string Path { get; set; }

    /// <summary>
    ///     Gets or sets the icon image.
    /// </summary>
    [ObservableProperty]
    public partial BitmapEntry Image { get; set; }
}
