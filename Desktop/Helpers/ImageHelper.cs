using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using QRCoder;

namespace Tavstal.KonkordLauncher.Desktop.Helpers;

/// <summary>
/// Provides helper methods for loading images from various sources, such as local resources or web URLs.
/// </summary>
public static class ImageHelper
{
    private static readonly HttpClient _httpClient = new();
    
    /// <summary>
    /// Loads an image from the specified file path or URI.
    /// </summary>
    /// <param name="path">The file path or URI of the image to load.</param>
    /// <returns>A <see cref="Bitmap"/> object if the image is successfully loaded; otherwise, null.</returns>
    public static async Task<Bitmap?> Load(string path) => await Load(new Uri(path));

    /// <summary>
    /// Loads an image from the specified URI, either from the web or a local resource.
    /// </summary>
    /// <param name="resourceUri">The URI of the image to load.</param>
    /// <returns>A <see cref="Bitmap"/> object if the image is successfully loaded; otherwise, null.</returns>
    public static async Task<Bitmap?> Load(Uri resourceUri)
    {
        if (resourceUri.ToString().StartsWith("http"))
            return await LoadFromWeb(resourceUri) ?? null;
        return LoadFromResource(resourceUri);
    }

    /// <summary>
    /// Loads an image from a local resource.
    /// </summary>
    /// <param name="resourceUri">The URI of the local resource to load.</param>
    /// <returns>A <see cref="Bitmap"/> object representing the loaded image.</returns>
    public static Bitmap LoadFromResource(Uri resourceUri)
    {
        return new Bitmap(AssetLoader.Open(resourceUri));
    }

    /// <summary>
    /// Loads an image from a web URL.
    /// </summary>
    /// <param name="url">The URL of the image to load.</param>
    /// <returns>A <see cref="Bitmap"/> object if the image is successfully downloaded and loaded; otherwise, null.</returns>
    public static async Task<Bitmap?> LoadFromWeb(Uri url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsByteArrayAsync();
            return new Bitmap(new MemoryStream(data));
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"An error occurred while downloading image '{url}' : {ex.Message}");
            return null;
        }
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