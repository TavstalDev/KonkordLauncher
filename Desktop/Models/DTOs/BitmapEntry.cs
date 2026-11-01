using Avalonia.Media.Imaging;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;

namespace Tavstal.KonkordLauncher.Desktop.Models.DTOs;

/// <summary>
///     Holds a bitmap and the key it is stored under in the bitmap cache.
/// </summary>
public class BitmapEntry
{
    /// <summary>
    ///     Creates a new entry.
    /// </summary>
    /// <param name="key">The key of the bitmap.</param>
    /// <param name="value">The bitmap.</param>
    public BitmapEntry(string? key, Bitmap? value)
    {
        Key = key;
        Value = value;
    }

    /// <summary>
    ///     Gets the key of the bitmap.
    /// </summary>
    public string? Key { get; private set; }

    /// <summary>
    ///     Gets the bitmap.
    /// </summary>
    public Bitmap? Value { get; private set; }

    /// <summary>
    ///     Sets a new key and bitmap for this entry.
    /// </summary>
    /// <param name="key">The new key.</param>
    /// <param name="value">The new bitmap.</param>
    public void SetValue(string? key, Bitmap? value)
    {
        Key = key;
        Value = value;
    }

    /// <summary>
    ///     Returns whether the entry has no key or no bitmap.
    /// </summary>
    /// <returns><see langword="true" /> if the entry has no key or no bitmap; otherwise, <see langword="false" />.</returns>
    public bool IsEmpty()
    {
        return Value == null || string.IsNullOrEmpty(Key);
    }

    /// <summary>
    ///     Releases the bitmap from the cache and clears the entry.
    /// </summary>
    /// <param name="service">The bitmap service that owns the bitmap.</param>
    public void Dispose(IBitmapService service)
    {
        if (Key != null)
            service.Release(Key);
        Key = null;
        Value = null;
    }
}
