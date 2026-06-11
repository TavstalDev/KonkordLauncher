using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Markdig;
using Microsoft.Extensions.DependencyInjection;
using Modrinth.Models;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.IO;

namespace Tavstal.KonkordLauncher.Desktop.Models.Instance;

/// <summary>
/// Represents a base model for a downloadable resource, providing observable properties for UI binding and a factory method to create instances from a Modrinth project.
/// </summary>
public partial class ResourceBaseModel : ObservableObject
{
    /// <summary>
    /// Gets or sets the display name of the resource.
    /// </summary>
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the description of the resource.
    /// </summary>
    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the raw HTML page content rendered from the project's Markdown body.
    /// </summary>
    [ObservableProperty]
    public partial string RawPage { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the bitmap icon for the resource.
    /// </summary>
    [ObservableProperty]
    public partial BitmapEntry Icon { get; set; }
    
    /// <summary>
    /// Gets or sets the URL of the resource's icon.
    /// </summary>
    [ObservableProperty]
    public partial string? IconUrl { get; set; }
    
    /// <summary>
    /// Gets or sets the local file path of the installed resource.
    /// </summary>
    [ObservableProperty]
    public partial string? FilePath { get; set; }
    
    /// <summary>
    /// Gets or sets the file size in bytes. Notifies the <see cref="FormattedSize"/> property on change.
    /// </summary>
    [ObservableProperty, NotifyPropertyChangedFor(nameof(FormattedSize))]
    public partial long FileSize { get; set; }
    
    /// <summary>
    /// Gets or sets whether the resource is enabled.
    /// </summary>
    [ObservableProperty]
    public partial bool IsEnabled { get; set; }
    
    /// <summary>
    /// Gets or sets whether the resource is selected in the UI.
    /// </summary>
    [ObservableProperty]
    public partial bool IsSelected { get; set; }
    
    /// <summary>
    /// Gets or sets whether the resource is installed locally.
    /// </summary>
    [ObservableProperty]
    public partial bool IsInstalled { get; set; }
    
    /// <summary>
    /// Gets or sets the target platform type (e.g. Modrinth).
    /// </summary>
    [ObservableProperty]
    public partial EPlatformType? Platform { get; set; }
    
    /// <summary>
    /// Gets or sets the project identifier from the source platform.
    /// </summary>
    [ObservableProperty]
    public partial string? ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the collection of available versions for this resource.
    /// </summary>
    public ObservableCollection<Version> Versions { get; set; } = [];
    
    /// <summary>
    /// Gets or sets the collection of tags or categories associated with this resource.
    /// </summary>
    public ObservableCollection<string> Tags { get; set; } = [];
    
    /// <summary>
    /// Gets the formatted file size string (e.g. "1.5 MB").
    /// </summary>
    public string FormattedSize => FileSystemHelper.GetFormattedSize(FileSize);
    
    /// <summary>
    /// Creates a <see cref="ResourceBaseModel"/> from a Modrinth project and its versions.
    /// </summary>
    /// <param name="project">The Modrinth project to import.</param>
    /// <param name="versions">The list of versions associated with the project.</param>
    /// <returns>A new <see cref="ResourceBaseModel"/> populated with the project data.</returns>
    public static async Task<ResourceBaseModel> FromModrinthProjectAsync(Project project, List<Version> versions)
    {
        var fallback = Task.FromResult(new BitmapEntry(null, null));
        var iconTask = project.IconUrl != null
            ? Program.ServiceProvider.GetRequiredService<IMetaCacheService>().GetImageAsync(project.IconUrl)!
            : fallback;
        
        var markdownTask = Task.Run(() => Markdown.ToHtml(project.Body));

        await Task.WhenAll(iconTask, markdownTask);
        
        return new ResourceBaseModel
        {
            ProjectId = project.Id,
            Name = project.Title,
            Description = project.Description,
            Icon = iconTask.Result,
            IconUrl = project.IconUrl,
            RawPage = markdownTask.Result,
            Versions = new ObservableCollection<Version>(versions),
            Tags = new ObservableCollection<string>(project.Categories),
            IsEnabled = true,
            Platform = EPlatformType.MODRINTH,
            FilePath = null
        };
    }
}