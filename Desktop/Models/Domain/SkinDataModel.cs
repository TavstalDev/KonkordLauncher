using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;

namespace Tavstal.KonkordLauncher.Desktop.Models.Domain;

/// <summary>
///     A player skin that the UI can show.
/// </summary>
public class SkinDataModel
{
    /// <summary>
    ///     Creates an empty skin model.
    /// </summary>
    public SkinDataModel() { }

    /// <summary>
    ///     Creates a skin model with the given values.
    /// </summary>
    /// <param name="id">The skin ID.</param>
    /// <param name="variant">The skin variant, such as "classic" or "slim".</param>
    /// <param name="image">The skin image.</param>
    /// <param name="isSelected">Whether the skin is selected at start.</param>
    public SkinDataModel(string id, string variant, BitmapEntry image, bool isSelected)
    {
        Id = id;
        Variant = variant;
        Image = image;
        IsSelected = isSelected;
    }

    /// <summary>
    ///     Gets or sets the skin ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the skin variant, such as "classic" or "slim".
    /// </summary>
    public string Variant { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the skin image.
    /// </summary>
    public BitmapEntry Image { get; set; } = new(null, null);

    /// <summary>
    ///     Gets or sets a value that indicates whether the skin is selected.
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    ///     Releases the skin image.
    /// </summary>
    /// <param name="bitmapService">The service used to release the image.</param>
    public void Dispose(IBitmapService bitmapService)
    {
        if (Image.IsEmpty())
            return;

        Image.Dispose(bitmapService);
    }
}
