using System.Text.Json.Serialization;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;

namespace Tavstal.KonkordLauncher.Desktop.Models.DTOs;

/// <summary>
///     Holds a downloadable resource of an instance.
/// </summary>
public class InstanceResource
{
    /// <summary>
    ///     Gets or sets the ID of the project.
    /// </summary>
    [JsonPropertyName("projectId")]
    public required string ProjectId { get; set; }

    /// <summary>
    ///     Gets or sets the optional version ID.
    /// </summary>
    [JsonPropertyName("versionId")]
    public string? VersionId { get; set; }

    /// <summary>
    ///     Gets or sets the name of the resource.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    ///     Gets or sets the main download URL.
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; set; }

    /// <summary>
    ///     Gets or sets the optional icon path.
    /// </summary>
    [JsonPropertyName("iconPath")]
    public string? IconPath { get; set; }

    /// <summary>
    ///     Gets or sets the path where the resource file is stored.
    /// </summary>
    [JsonPropertyName("path")]
    public required string Path { get; set; }

    /// <summary>
    ///     Gets or sets the optional client URL.
    /// </summary>
    [JsonPropertyName("client")]
    public string? Client { get; set; }

    /// <summary>
    ///     Gets or sets the optional server URL.
    /// </summary>
    [JsonPropertyName("server")]
    public string? Server { get; set; }

    /// <summary>
    ///     Gets or sets the optional SHA-1 checksum.
    /// </summary>
    [JsonPropertyName("sha1")]
    public string? Sha1 { get; set; }

    /// <summary>
    ///     Gets or sets the optional SHA-512 checksum.
    /// </summary>
    [JsonPropertyName("sha256")]
    public string? Sha512 { get; set; }

    /// <summary>
    ///     Gets or sets the platform that the resource targets.
    /// </summary>
    [JsonPropertyName("platform")]
    public EPlatformType Platform { get; set; }

    /// <summary>
    ///     Gets or sets the type of the resource.
    /// </summary>
    [JsonPropertyName("type")]
    public EResourceType Type { get; set; }

    /// <summary>
    ///     Gets or sets the file size in bytes.
    /// </summary>
    [JsonPropertyName("fileSize")]
    public long FileSize { get; set; }

    /// <summary>
    ///     Gets or sets the optional file ID. For CurseForge files, this is the CurseForge file ID.
    /// </summary>
    [JsonPropertyName("fileId")]
    public string? FileId { get; set; }
}
