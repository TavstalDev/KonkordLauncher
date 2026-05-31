using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Enums;

namespace Tavstal.KonkordLauncher.Common.Models;

/// <summary>
/// Represents a downloadable resource associated with an instance, including its identifiers,
/// download location, optional platform-specific endpoints, and file metadata.
/// </summary>
public class InstanceResource
{
    /// <summary>
    /// Gets or sets the project identifier this resource belongs to.
    /// </summary>
    [JsonProperty("projectId")]
    public required string ProjectId { get; set; }
    
    /// <summary>
    /// Gets or sets the instance identifier or name associated with this resource.
    /// </summary>
    [JsonProperty("instanceId")]
    public required string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the primary download URL for the resource.
    /// </summary>
    [JsonProperty("url")]
    public required string Url { get; set; }
    
    /// <summary>
    /// Gets or sets the optional icon path for the resource.
    /// </summary>
    [JsonProperty("iconPath")]
    public string? IconPath { get; set; }
    
    /// <summary>
    /// Gets or sets the relative or absolute path where the resource should be stored or resolved.
    /// </summary>
    [JsonProperty("path")]
    public required string Path { get; set; }
    
    /// <summary>
    /// Gets or sets the optional client-specific resource URL or reference.
    /// </summary>
    [JsonProperty("client")]
    public string? Client { get; set; }
    
    /// <summary>
    /// Gets or sets the optional server-specific resource URL or reference.
    /// </summary>
    [JsonProperty("server")]
    public string? Server { get; set; }
    
    /// <summary>
    /// Gets or sets the SHA-1 checksum for validating the resource, if available.
    /// </summary>
    [JsonProperty("sha1")]
    public string? Sha1 { get; set; }
    
    /// <summary>
    /// Gets or sets the SHA-512 checksum for validating the resource, if available.
    /// </summary>
    [JsonProperty("sha256")]
    public string? Sha512 { get; set; }
    
    /// <summary>
    /// Gets or sets the platform type that this resource targets.
    /// </summary>
    [JsonProperty("platform")]
    public EPlatformType Platform { get; set; }
    
    /// <summary>
    /// Gets or sets the type of resource represented by this object.
    /// </summary>
    [JsonProperty("type")]
    public EResourceType Type { get; set; }
    
    /// <summary>
    /// Gets or sets the size of the resource in bytes.
    /// </summary>
    [JsonProperty("fileSize")]
    public long FileSize { get; set; }
}