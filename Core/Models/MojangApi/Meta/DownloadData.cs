using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;

/// <summary>
/// Represents the data for a downloadable resource, including its hash, URL, and size.
/// </summary>
public class DownloadData
{
    /// <summary>
    /// Gets or sets the SHA-1 hash of the downloadable resource.
    /// </summary>
    [JsonPropertyName("sha1"), JsonProperty("sha1")]
    public string Sha1 { get; set; }

    /// <summary>
    /// Gets or sets the URL of the downloadable resource.
    /// </summary>
    [JsonPropertyName("url"), JsonProperty("url")]
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets the size of the downloadable resource in bytes.
    /// </summary>
    [JsonPropertyName("size"), JsonProperty("size")]
    public int Size { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadData"/> class.
    /// </summary>
    public DownloadData() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadData"/> class with specified properties.
    /// </summary>
    /// <param name="sha1">The SHA-1 hash of the downloadable resource.</param>
    /// <param name="url">The URL of the downloadable resource.</param>
    /// <param name="size">The size of the downloadable resource in bytes.</param>
    public DownloadData(string sha1, string url, int size)
    {
        Sha1 = sha1;
        Url = url;
        Size = size;
    }
}