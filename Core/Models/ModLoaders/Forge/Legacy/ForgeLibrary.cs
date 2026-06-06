
using System.Text.Json.Serialization;


namespace Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge.Legacy;

/// <summary>
/// Represents a Forge library with its metadata and utility methods.
/// </summary>
public class ForgeLibrary
{
    /// <summary>
    /// Gets or sets the name of the Forge library.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the URL of the Forge library.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the library is required on the client side.
    /// </summary>
    [JsonPropertyName("clientreq")]
    public bool? ClientRequires { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the library is required on the server side.
    /// </summary>
    [JsonPropertyName("serverreq")]
    public bool? ServerRequires { get; set; }

    /// <summary>
    /// Gets or sets the list of checksums for the Forge library.
    /// </summary>
    [JsonPropertyName("checksums")]
    public List<string>? Checksums { get; set; }

    /// <summary>
    /// Gets the URL of the Forge library, optionally using a legacy base URL.
    /// </summary>
    /// <param name="isLegacy">Indicates whether to use the legacy base URL.</param>
    /// <returns>The constructed URL of the library, or <c>null</c> if the URL is not set and not in legacy mode.</returns>
    public string? GetUrl(bool isLegacy = false)
    {
        if (Url == null)
        {
            if (isLegacy)
                Url = "https://libraries.minecraft.net/";
            else
                return null;
        }

        string[] rawUrl = Name.Split(':');

        return Path.Combine(Url, rawUrl[0].Replace('.', '/'), rawUrl[1], rawUrl[2], $"{rawUrl[1]}-{rawUrl[2]}.jar").Replace("\\", "/");
    }

    /// <summary>
    /// Gets the file path of the Forge library based on its name.
    /// </summary>
    /// <returns>The constructed file path of the library.</returns>
    public string GetPath()
    {
        string[] rawUrl = Name.Split(':');

        return Path.Combine(rawUrl[0].Replace('.', '/'), rawUrl[1], rawUrl[2], $"{rawUrl[1]}-{rawUrl[2]}.jar").Replace("\\", "/");
    }
}