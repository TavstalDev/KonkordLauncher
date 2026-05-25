
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Common.Models.MetaCache;

/// <summary>
/// Represents a cached metadata entry stored on disk.
/// </summary>
public class MetaCache
{
    /// <summary>
    /// Gets or sets the unique identifier of the cached item.
    /// </summary>
    [JsonProperty("id")]
    public required string Id { get; set; }
    
    /// <summary>
    /// Gets or sets the type of cached metadata.
    /// </summary>
    [JsonProperty("type")]
    public required EMetaCacheType Type { get; set; }
    
    /// <summary>
    /// Gets or sets the file path where the cached data is stored.
    /// </summary>
    [JsonProperty("path")]
    public required string Path { get; set; }
    
    /// <summary>
    /// Gets or sets the UTC timestamp at which the cache entry expires.
    /// </summary>
    [JsonProperty("valid_until")]
    public required DateTime ValidUntil { get; set; }
    
    /// <summary>
    /// Gets or sets the last modification timestamp known for the cached resource.
    /// </summary>
    [JsonProperty("last_modified_at")]
    public DateTime? LastModifiedAt { get; set; }
    
    /// <summary>
    /// Determines whether the cache entry is still valid.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the current UTC time is earlier than <see cref="ValidUntil"/>
    /// and the cached file exists at <see cref="Path"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsValid() => DateTime.UtcNow < ValidUntil && File.Exists(Path);
}