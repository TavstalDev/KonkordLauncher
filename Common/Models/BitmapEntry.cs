using Avalonia.Media.Imaging;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Common.Models;

/// <summary>
/// Represents an entry in a bitmap cache, pairing a string key with a <see cref="Bitmap"/> value.
/// </summary>
public class BitmapEntry
{
    /// <summary>
    /// Gets the key associated with this bitmap entry.
    /// </summary>
    public string? Key { get; private set; }
    
    /// <summary>
    /// Gets the bitmap value associated with this entry.
    /// </summary>
    public Bitmap? Value { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BitmapEntry"/> class.
    /// </summary>
    /// <param name="key">The key to associate with the bitmap.</param>
    /// <param name="value">The bitmap value.</param>
    public BitmapEntry(string? key, Bitmap? value)
    {
        Key = key;
        Value = value;
    }
    
    /// <summary>
    /// Sets or updates the key and value of this entry.
    /// </summary>
    /// <param name="key">The new key.</param>
    /// <param name="value">The new bitmap value.</param>
    public void SetValue(string? key, Bitmap? value)
    {
        Key = key;
        Value = value;
    }
    
    /// <summary>
    /// Determines whether this entry is empty (null value or null/empty key).
    /// </summary>
    /// <returns><c>true</c> if the entry is empty; otherwise, <c>false</c>.</returns>
    public bool IsEmpty()
    {
        return Value == null || string.IsNullOrEmpty(Key);
    }

    /// <summary>
    /// Releases the bitmap from the provided bitmap service and clears the entry.
    /// </summary>
    /// <param name="service">The bitmap service used to release the bitmap resource.</param>
    public void Dispose(IBitmapService service)
    {
        if (Key != null)
            service.Release(Key);
        Key = null;
        Value = null;
    }
}