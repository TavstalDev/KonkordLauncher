using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using QRCoder;
using Tavstal.KonkordLauncher.Common.Models;

namespace Tavstal.KonkordLauncher.Desktop.Helpers;

/// <summary>
/// Provides helper methods for loading images from various sources, such as local resources or web URLs.
/// </summary>
public static class ImageHelper
{
    /// <summary>
    /// Loads a <see cref="Bitmap"/> from an application resource path.
    /// </summary>
    /// <param name="path">
    /// The resource URI to load. Expected to be a valid Avalonia asset URI (for example: "avares://AssemblyName/Assets/image.png")
    /// or any URI that <see cref="Avalonia.Platform.IAssetLoader"/> supports when running in design mode.
    /// </param>
    /// <returns> A <see cref="Bitmap"/> created from the requested resource stream.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when called outside of design mode. This helper is intended for use in design-time scenarios only
    /// (for example in XAML preview/design-time data) and will not attempt to resolve resources at runtime.
    /// </exception>
    public static BitmapEntry LoadDesignTime(string path)
    {
        return !Design.IsDesignMode ? throw new InvalidOperationException("LoadFromResource should only be used in design mode with valid resource paths.") : new BitmapEntry(path, new Bitmap(AssetLoader.Open(new Uri(path))));
    }
    
    /// <summary>
    /// Converts a Base64-encoded string to an Avalonia <see cref="Bitmap"/> object.
    /// </summary>
    /// <param name="base64Image">The Base64-encoded string representing the image.</param>
    /// <returns>A <see cref="Bitmap"/> object created from the Base64 string.</returns>
    public static Bitmap Base64ToBitmap(string base64Image)
    {
        // Remove the "data:image/png;base64," prefix if present
        var base64Data = base64Image;
        const string prefix = "data:image/png;base64,";
        if (base64Data.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            base64Data = base64Data.Substring(prefix.Length);

        // Decode Base64 to byte array
        byte[] imageBytes = Convert.FromBase64String(base64Data);

        // Load into Avalonia Bitmap
        using var ms = new MemoryStream(imageBytes);
        return new Bitmap(ms);
    }

    /// <summary>
    /// Converts an Avalonia <see cref="Bitmap"/> object to a Base64-encoded string.
    /// </summary>
    /// <param name="bitmap">The <see cref="Bitmap"/> object to convert.</param>
    /// <returns>A Base64-encoded string representing the image.</returns>
    public static string BitmapToBase64(Bitmap bitmap)
    {
        using var ms = new MemoryStream();
        bitmap.Save(ms);
        byte[] imageBytes = ms.ToArray();
        return Convert.ToBase64String(imageBytes);
    }
    
    /// <summary>
    /// Generates a QR code from the provided data and returns it as an Avalonia <see cref="Bitmap"/> object.
    /// </summary>
    /// <param name="data">The data to encode in the QR code.</param>
    /// <returns>A <see cref="Bitmap"/> object representing the generated QR code.</returns>
    public static Bitmap GenerateQrCode(string data)
    {
        var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrData);
        var qrBytes = qrCode.GetGraphic(20); // 20 is the pixels per module

        // Convert the byte array to an Avalonia IBitmap
        using var stream = new MemoryStream(qrBytes);
        return new Bitmap(stream);
    }
}