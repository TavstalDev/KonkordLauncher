using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;

/// <summary>
/// Represents metadata for a Minecraft asset, including its ID, hash, size, and download URL.
/// </summary>
public class AssetMeta
{
    /// <summary>
    /// Gets or sets the unique identifier for the asset.
    /// </summary>
    [JsonPropertyName("id"), JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the SHA-1 hash of the asset.
    /// </summary>
    [JsonPropertyName("sha1"), JsonProperty("sha1")]
    public string Sha1 { get; set; }

    /// <summary>
    /// Gets or sets the size of the asset in bytes.
    /// </summary>
    [JsonPropertyName("size"), JsonProperty("size")]
    public int Size { get; set; }

    /// <summary>
    /// Gets or sets the total size of the asset in bytes.
    /// </summary>
    [JsonPropertyName("totalSize"), JsonProperty("totalSize")]
    public int TotalSize { get; set; }

    /// <summary>
    /// Gets or sets the URL to download the asset.
    /// </summary>
    [JsonPropertyName("url"), JsonProperty("url")]
    public string Url { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AssetMeta"/> class.
    /// </summary>
    public AssetMeta()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AssetMeta"/> class with specified properties.
    /// </summary>
    /// <param name="id">The unique identifier for the asset.</param>
    /// <param name="sha1">The SHA-1 hash of the asset.</param>
    /// <param name="size">The size of the asset in bytes.</param>
    /// <param name="totalSize">The total size of the asset in bytes.</param>
    /// <param name="url">The URL to download the asset.</param>
    public AssetMeta(string id, string sha1, int size, int totalSize, string url)
    {
        Id = id;
        Sha1 = sha1;
        Size = size;
        TotalSize = totalSize;
        Url = url;
    }
}