using Avalonia.Media.Imaging;
using Tavstal.KonkordLauncher.Common.Models;

namespace Tavstal.KonkordLauncher.Common.Services.Abstractions;

/// <summary>
/// Centralized service to manage Bitmap lifecycles, caching, and proactive unloading.
/// </summary>
public interface IBitmapService
{
    /// <summary>
    /// Loads a bitmap from a URI (e.g., avares://) or local path.
    /// Returns an existing instance if already cached.
    /// </summary>
    /// <returns>
    /// An <see cref="Bitmap"/> that wraps the loaded bitmap and participates in the service-managed cache/lifecycle.
    /// </returns>
    BitmapEntry GetBitmap(string path);

    /// <summary>
    /// Retrieves a cached bitmap representation for the provided Base64-encoded image string.
    /// If the image has already been decoded and cached, the existing cached instance is returned.
    /// </summary>
    /// <returns>
    /// An <see cref="Bitmap"/> that wraps the decoded bitmap and participates in the
    /// service-managed cache/lifecycle.
    /// </returns>
    BitmapEntry GetBitmapBase64(string key, string base64Image);

    /// <summary>
    /// Asynchronously loads a bitmap, ideal for web URLs or large local files.
    /// </summary>
    /// <returns>
    /// A task that resolves to an <see cref="Bitmap"/> wrapping the loaded bitmap, which participates in the service-managed cache/lifecycle.
    /// </returns>
    Task<BitmapEntry> GetBitmapAsync(string uri);

    /// <summary>
    /// Releases a previously-acquired bitmap reference from the service's cache using the provided key.
    /// </summary>
    /// <param name="key">The unique cache key that identifies the bitmap to release.</param>
    void Release(string key);
}