using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Tavstal.KonkordLauncher.Desktop.Helpers;

/// <summary>
/// Provides helper methods for loading images from various sources, such as local resources or web URLs.
/// </summary>
public static class ImageHelper
{
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
        if (resourceUri.PathAndQuery.StartsWith("http"))
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
        using var httpClient = new HttpClient();
        try
        {
            var response = await httpClient.GetAsync(url);
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
}