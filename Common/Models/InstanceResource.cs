using System.Text.Json.Serialization;

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
    [JsonPropertyName("projectId")]
    public required string ProjectId { get; set; }
    
    /// <summary>
    /// Gets or sets the optional version identifier this resource is associated with, if applicable.
    /// </summary>
    [JsonPropertyName("versionId")]
    public string? VersionId { get; set; }
    
    /// <summary>
    /// Gets or sets the instance identifier or name associated with this resource.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the primary download URL for the resource.
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; set; }
    
    /// <summary>
    /// Gets or sets the optional icon path for the resource.
    /// </summary>
    [JsonPropertyName("iconPath")]
    public string? IconPath { get; set; }
    
    /// <summary>
    /// Gets or sets the relative or absolute path where the resource should be stored or resolved.
    /// </summary>
    [JsonPropertyName("path")]
    public required string Path { get; set; }
    
    /// <summary>
    /// Gets or sets the optional client-specific resource URL or reference.
    /// </summary>
    [JsonPropertyName("client")]
    public string? Client { get; set; }
    
    /// <summary>
    /// Gets or sets the optional server-specific resource URL or reference.
    /// </summary>
    [JsonPropertyName("server")]
    public string? Server { get; set; }
    
    /// <summary>
    /// Gets or sets the SHA-1 checksum for validating the resource, if available.
    /// </summary>
    [JsonPropertyName("sha1")]
    public string? Sha1 { get; set; }
    
    /// <summary>
    /// Gets or sets the SHA-512 checksum for validating the resource, if available.
    /// </summary>
    [JsonPropertyName("sha256")]
    public string? Sha512 { get; set; }
    
    /// <summary>
    /// Gets or sets the platform type that this resource targets.
    /// </summary>
    [JsonPropertyName("platform")]
    public EPlatformType Platform { get; set; }
    
    /// <summary>
    /// Gets or sets the type of resource represented by this object.
    /// </summary>
    [JsonPropertyName("type")]
    public EResourceType Type { get; set; }
    
    /// <summary>
    /// Gets or sets the size of the resource in bytes.
    /// </summary>
    [JsonPropertyName("fileSize")]
    public long FileSize { get; set; }
    
    /// <summary>
    /// Gets or sets the optional file identifier for this resource, which can be used for tracking or referencing the file in storage or download systems.
    /// </summary>
    /// <remarks>
    /// This field can be used to store the CurseForge file ID, which is a unique identifier for files hosted on CurseForge.
    /// </remarks>
    [JsonPropertyName("fileId")]
    public string? FileId { get; set; }
}