
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.ModLoaders.Fabric;

/// <summary>
/// Represents a Fabric library entry from Fabric metadata files.
/// The library is described by a Maven-style coordinate stored in <see cref="Name"/>,
/// a base URL, checksums and a size. Helper methods generate the remote URL and
/// the local repository path for the library artifact.
/// </summary>
public class FabricLibrary
{
    /// <summary>
    /// Gets or sets the Maven coordinate of the library in the form "group:artifact:version".
    /// Example: "net.fabricmc:fabric-loader:0.14.8".
    /// This value is parsed by <see cref="GetURL"/> and <see cref="GetPath"/>.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the base URL for the repository that hosts the artifact.
    /// Example: "https://maven.fabricmc.net/".
    /// The final artifact URL returned by <see cref="GetURL"/> is <c>Url + path</c>.
    /// </summary>
    [JsonProperty("url")]
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets the MD5 checksum of the artifact, if provided.
    /// </summary>
    [JsonProperty("md5")]
    public string Md5 { get; set; }

    /// <summary>
    /// Gets or sets the SHA-1 checksum of the artifact, if provided.
    /// </summary>
    [JsonProperty("sha1")]
    public string Sha1 { get; set; }

    /// <summary>
    /// Gets or sets the SHA-256 checksum of the artifact, if provided.
    /// </summary>
    [JsonProperty("sha256")]
    public string Sha256 { get; set; }

    /// <summary>
    /// Gets or sets the SHA-512 checksum of the artifact, if provided.
    /// </summary>
    [JsonProperty("sha512")]
    public string Sha512 { get; set; }

    /// <summary>
    /// Gets or sets the artifact size in bytes as reported by the metadata.
    /// </summary>
    [JsonProperty("size")]
    public int Size { get; set; }

    /// <summary>
    /// Parameterless constructor for deserialization.
    /// </summary>
    public FabricLibrary() { }

    /// <summary>
    /// Builds the full HTTP(S) URL to download the library artifact.
    /// </summary>
    /// <remarks>
    /// This method expects <see cref="Name"/> to be a colon-separated Maven coordinate:
    /// "group:artifact:version". It constructs the typical Maven path:
    /// {group with dots replaced by '/'} / {artifact} / {version} / {artifact}-{version}.jar
    /// and returns <c>Url + path</c>.
    /// 
    /// Note: if <see cref="Name"/> is not in the expected format an <see cref="IndexOutOfRangeException"/>
    /// or similar error may occur when splitting parts. Consumers should ensure valid coordinates.
    /// </remarks>
    /// <returns>The full download URL for the artifact.</returns>
    public string GetURL()
    {
        var parts = Name.Split(':') ?? [];
        if (parts.Length != 3)
            throw new FormatException($"Invalid Maven coordinate '{Name}'");
        string groupPath = parts[0].Replace('.', '/');
        string artifact = parts[1];
        string version = parts[2];
        string path = $"{groupPath}/{artifact}/{version}/{artifact}-{version}.jar";
        return Url.TrimEnd('/') + "/" + path;
    }

    /// <summary>
    /// Builds a repository-style relative path for the artifact suitable for local storage.
    /// </summary>
    /// <remarks>
    /// Produces a path in the form:
    /// {group with dots replaced by '/'} / {artifact} / {version} / {artifact}-{version}.jar
    /// Spaces in the resulting path are replaced with underscores to avoid filesystem issues.
    /// </remarks>
    /// <returns>The relative repository path for the artifact, safe for filesystem use.</returns>
    public string GetPath()
    {
        string[] parts = Name.Split(":", 3);
        char separator = '/';
        string path = parts[0].Replace('.', separator) + separator + parts[1] + separator + parts[2] + separator + parts[1] + "-" + parts[2] + ".jar";
        return path.Replace(" ", "_");
    }
}