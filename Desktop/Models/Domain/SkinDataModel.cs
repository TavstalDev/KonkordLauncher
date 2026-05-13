using System;
using Avalonia.Media.Imaging;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.User;
using Tavstal.KonkordLauncher.Desktop.Helpers;

namespace Tavstal.KonkordLauncher.Desktop.Models.Domain;

/// <summary>
/// Lightweight model representing a player's skin (or cape) data for UI consumption.
/// </summary>
public class SkinDataModel : IDisposable
{
    /// <summary>
    /// Unique identifier for the skin resource (typically the Mojang-provided ID).
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Variant of the skin (for example "classic" or "slim"). Mirrors Mojang API naming.
    /// </summary>
    public string Variant { get; set; }
    
    /// <summary>
    /// Loaded image for the skin. May be null if loading failed or not provided.
    /// Dispose this object when the model is no longer used by calling <see cref="Dispose"/>.
    /// </summary>
    public Bitmap? Image { get; set; }
    
    /// <summary>
    /// Whether this skin is currently selected in the UI.
    /// </summary>
    public bool IsSelected { get; set; }
    
    /// <summary>
    /// Parameterless constructor for serializers and manual initialization.
    /// </summary>
    public SkinDataModel() { }
    
    /// <summary>
    /// Creates a new <see cref="SkinDataModel"/> with specified values.
    /// </summary>
    /// <param name="id">Skin identifier.</param>
    /// <param name="variant">Skin variant (e.g. "classic" or "slim").</param>
    /// <param name="image">Already-loaded <see cref="Bitmap"/> for the skin (can be null).</param>
    /// <param name="isSelected">Initial selected state in the UI.</param>
    public SkinDataModel(string id, string variant, Bitmap? image, bool isSelected)
    {
        Id = id;
        Variant = variant;
        Image = image;
        IsSelected = isSelected;
    }
    
    /// <summary>
    /// Convenience constructor that builds a <see cref="SkinDataModel"/> from a Mojang <see cref="Skin"/>.
    /// </summary>
    /// <param name="skin">Mojang skin object containing id, variant and URL.</param>
    /// <param name="isSelected">Initial selected state.</param>
    public SkinDataModel(Skin skin, bool isSelected = false) : this(skin.Id, skin.Variant, ImageHelper.Load(skin.Url).Result, isSelected) { }

    /// <summary>
    /// Releases resources held by this model. Disposes the <see cref="Image"/> bitmap if present.
    /// </summary>
    public void Dispose()
    {
        Image?.Dispose();
    }
}