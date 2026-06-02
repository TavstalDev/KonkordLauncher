using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;

namespace Tavstal.KonkordLauncher.Desktop.Models.Instance;

public partial class ResourceDownloadModel : KonkordObservableObject
{
    public required string ProjectId { get; set; }
    
    public required string Name { get; set; }
    
    public required string FileName { get; set; }
    
    public required string Version { get; set; }
    
    public required string Sha1 { get; set; }
    
    public required string Sha512 { get; set; }
    
    public required string Url { get; set; }
    
    public string? IconUrl { get; set; }
    
    public required EPlatformType Platform { get; set; }
    
    [ObservableProperty]
    public partial bool ShouldDownload { get; set; }
}