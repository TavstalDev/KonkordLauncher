using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation;

namespace Tavstal.KonkordLauncher.Desktop.Models.Domain;

/// <summary>
///     Lightweight model representing a player's skin (or cape) data for UI consumption.
/// </summary>
public class SkinDataModel
{
    /// <summary>
    ///     Parameterless constructor for serializers and manual initialization.
    /// </summary>
    public SkinDataModel() { }

    /// <summary>
    ///     Creates a new <see cref="SkinDataModel" /> with specified values.
    /// </summary>
    /// <param name="id">Skin identifier.</param>
    /// <param name="variant">Skin variant (e.g. "classic" or "slim").</param>
    /// <param name="image">Loaded skin image. May be null if not available.</param>
    /// <param name="isSelected">Initial selected state in the UI.</param>
    public SkinDataModel(string id, string variant, BitmapEntry image, bool isSelected)
    {
        Id = id;
        Variant = variant;
        Image = image;
        IsSelected = isSelected;
    }

    /// <summary>
    ///     Unique identifier for the skin resource (typically the Mojang-provided ID).
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    ///     Variant of the skin (for example "classic" or "slim"). Mirrors Mojang API naming.
    /// </summary>
    public string Variant { get; set; } = string.Empty;

    /// <summary>
    ///     Loaded image for the skin. May be null if loading failed or not provided.
    ///     Dispose this object when the model is no longer used by calling <see cref="Dispose" />.
    /// </summary>
    public BitmapEntry Image { get; set; } = new(null, null);

    /// <summary>
    ///     Whether this skin is currently selected in the UI.
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    ///     Releases resources held by this model. Disposes the <see cref="Image" /> bitmap if present.
    /// </summary>
    public void Dispose(IBitmapService bitmapService)
    {
        if (Image.IsEmpty())
            return;

        Image.Dispose(bitmapService);
    }
}
