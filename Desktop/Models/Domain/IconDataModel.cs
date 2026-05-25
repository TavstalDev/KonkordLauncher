using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.KonkordLauncher.Desktop.Models.Domain;

/// <summary>
/// Represents a data model for an icon, containing its name and image.
/// </summary>
public partial class IconDataModel : ObservableObject
{
    /// <summary>
    /// The name of the icon.
    /// </summary>
    [ObservableProperty]
    public partial string Name { get; set; }

    /// <summary>
    /// The file path associated with the icon.
    /// </summary>
    [ObservableProperty]
    public partial string Path { get; set; }

    /// <summary>
    /// The image associated with the icon.
    /// </summary>
    [ObservableProperty] private Bitmap _image;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="IconDataModel"/> class.
    /// </summary>
    /// <param name="name">The name of the icon.</param>
    /// <param name="path">The file path of the icon.</param>
    /// <param name="image">The image associated with the icon.</param>
    public IconDataModel(string name, string path, Bitmap image)
    {
        Name = name;
        Path = path;
        _image = image;
    }
}