using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;

namespace Tavstal.KonkordLauncher.Desktop.Models.Instance;

/// <summary>
/// Represents a downloadable resource associated with a project instance, including metadata and download state.
/// </summary>
public partial class ResourceDownloadModel : KonkordObservableObject
{
    /// <summary>
    /// Gets or sets the project identifier.
    /// </summary>
    public required string ProjectId { get; set; }
    
    /// <summary>
    /// Gets or sets the display name of the resource.
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the file name of the resource.
    /// </summary>
    public required string FileName { get; set; }
    
    /// <summary>
    /// Gets or sets the version string of the resource.
    /// </summary>
    public required string Version { get; set; }
    
    /// <summary>
    /// Gets or sets the SHA-1 hash of the resource file.
    /// </summary>
    public required string Sha1 { get; set; }
    
    /// <summary>
    /// Gets or sets the SHA-512 hash of the resource file.
    /// </summary>
    public required string Sha512 { get; set; }
    
    /// <summary>
    /// Gets or sets the download URL of the resource.
    /// </summary>
    public required string Url { get; set; }
    
    /// <summary>
    /// Gets or sets the URL for the resource's icon.
    /// </summary>
    public string? IconUrl { get; set; }
    
    /// <summary>
    /// Gets or sets the target platform for this resource.
    /// </summary>
    public required EPlatformType Platform { get; set; }
    
    /// <summary>
    /// Gets or sets whether this resource should be downloaded.
    /// </summary>
    [ObservableProperty]
    public partial bool ShouldDownload { get; set; }
}