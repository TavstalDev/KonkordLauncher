using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using QRCoder;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;

namespace Tavstal.KonkordLauncher.Desktop.Helpers;

/// <summary>
///     Helpers for loading images from application resources or encoded data.
/// </summary>
public static class ImageHelper
{
    /// <summary>
    ///     Loads a <see cref="Bitmap" /> from an application resource path.
    /// </summary>
    /// <param name="path">The Avalonia asset URI of the image, such as "avares://AssemblyName/Assets/image.png".</param>
    /// <returns>The loaded image.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the method is not called in design mode.
    /// </exception>
    public static BitmapEntry LoadDesignTime(string path)
    {
        return !Design.IsDesignMode
            ? throw new InvalidOperationException(
                "LoadFromResource should only be used in design mode with valid resource paths.")
            : new BitmapEntry(path, new Bitmap(AssetLoader.Open(new Uri(path))));
    }

    /// <summary>
    ///     Converts a Base64 string into a <see cref="Bitmap" />.
    /// </summary>
    /// <param name="base64Image">The Base64 string of the image.</param>
    /// <returns>The created image.</returns>
    public static Bitmap Base64ToBitmap(string base64Image)
    {
        // Remove the "data:image/png;base64," prefix if present
        var base64Data = base64Image;
        const string prefix = "data:image/png;base64,";
        if (base64Data.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            base64Data = base64Data.Substring(prefix.Length);

        // Decode Base64 to byte array
        var imageBytes = Convert.FromBase64String(base64Data);

        // Load into Avalonia Bitmap
        using var ms = new MemoryStream(imageBytes);
        return new Bitmap(ms);
    }

    /// <summary>
    ///     Creates a QR code image from the given data.
    /// </summary>
    /// <param name="data">The data to encode.</param>
    /// <returns>The generated QR code image.</returns>
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
