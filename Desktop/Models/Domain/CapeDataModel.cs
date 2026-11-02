using Avalonia.Media.Imaging;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;

namespace Tavstal.KonkordLauncher.Desktop.Models.Domain;

/// <summary>
///     A cape that the UI can show, with an ID, alias, image, and selection state.
/// </summary>
public class CapeDataModel
{
    /// <summary>
    ///     Creates an empty cape model.
    /// </summary>
    public CapeDataModel() { }

    /// <summary>
    ///     Creates a cape model with the given values.
    /// </summary>
    /// <param name="id">The cape ID.</param>
    /// <param name="alias">The cape alias. Can be empty.</param>
    /// <param name="image">The image to show.</param>
    /// <param name="isSelected">Whether the cape is selected at start.</param>
    public CapeDataModel(string id, string alias, BitmapEntry image, bool isSelected)
    {
        Id = id;
        Alias = alias;
        Image = image;
        IsSelected = isSelected;
    }

    /// <summary>
    ///     Gets or sets the cape ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the cape alias.
    /// </summary>
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the cape image.
    /// </summary>
    public BitmapEntry Image { get; set; } = new(null, null);

    /// <summary>
    ///     Gets or sets a value that indicates whether the cape is selected.
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    ///     Releases the cape image.
    /// </summary>
    /// <param name="bitmapService">The service used to release the image.</param>
    public void Dispose(IBitmapService bitmapService)
    {
        if (Image.IsEmpty())
            return;

        Image.Dispose(bitmapService);
    }
}
