using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta.Library;

/// <summary>
/// Represents an artifact with metadata such as path, SHA-1 hash, size, and URL.
/// </summary>
public class Artifact
{
    /// <summary>
    /// Gets or sets the file path of the artifact.
    /// </summary>
    [JsonPropertyName("path"), JsonProperty("path")]
    public string Path { get; set; }

    /// <summary>
    /// Gets or sets the SHA-1 hash of the artifact.
    /// </summary>
    [JsonPropertyName("sha1"), JsonProperty("sha1")]
    public string Sha1 { get; set; }

    /// <summary>
    /// Gets or sets the size of the artifact in bytes.
    /// </summary>
    [JsonPropertyName("size"), JsonProperty("size")]
    public int Size { get; set; }

    /// <summary>
    /// Gets or sets the URL of the artifact.
    /// </summary>
    [JsonPropertyName("url"), JsonProperty("url")]
    public string Url { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Artifact"/> class.
    /// </summary>
    public Artifact()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Artifact"/> class with specified properties.
    /// </summary>
    /// <param name="path">The file path of the artifact.</param>
    /// <param name="sha1">The SHA-1 hash of the artifact.</param>
    /// <param name="size">The size of the artifact in bytes.</param>
    /// <param name="url">The URL of the artifact.</param>
    public Artifact(string path, string sha1, int size, string url)
    {
        Path = path;
        Sha1 = sha1;
        Size = size;
        Url = url;
    }
}