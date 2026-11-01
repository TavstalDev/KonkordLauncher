using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;

namespace Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;

/// <summary>
///     Keeps bitmaps in a cache and releases them when they are no longer used.
/// </summary>
public interface IBitmapService
{
    /// <summary>
    ///     Loads a bitmap from a resource URI or a local path.
    ///     If the bitmap is already cached, the cached copy is returned.
    /// </summary>
    /// <param name="path">The resource URI or local file path.</param>
    /// <param name="decodeWidth">The width used when the image is decoded.</param>
    /// <returns>The loaded <see cref="BitmapEntry" />. The cache keeps track of it.</returns>
    BitmapEntry GetBitmap(string path, int decodeWidth = 256);

    /// <summary>
    ///     Loads a bitmap from a Base64-encoded image string.
    ///     If the image is already cached, the cached copy is returned.
    /// </summary>
    /// <param name="key">The cache key for the image.</param>
    /// <param name="base64Image">The Base64-encoded image data.</param>
    /// <param name="decodeWidth">The width used when the image is decoded.</param>
    /// <returns>The loaded <see cref="BitmapEntry" />. The cache keeps track of it.</returns>
    BitmapEntry GetBitmapBase64(string key, string base64Image, int decodeWidth = 256);

    /// <summary>
    ///     Loads a bitmap from a web address or a local file. This is best for large files.
    /// </summary>
    /// <param name="uri">The web address or local file path.</param>
    /// <param name="decodeWidth">The width used when the image is decoded.</param>
    /// <returns>A task that returns the loaded <see cref="BitmapEntry" />.</returns>
    Task<BitmapEntry> GetBitmapAsync(string uri, int decodeWidth = 256);

    /// <summary>
    ///     Releases one use of a cached bitmap. The bitmap is removed when no uses remain.
    /// </summary>
    /// <param name="key">The cache key of the bitmap.</param>
    void Release(string key);
}
