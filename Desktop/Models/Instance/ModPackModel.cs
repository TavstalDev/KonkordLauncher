using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Markdig;
using Microsoft.Extensions.DependencyInjection;
using Modrinth.Models;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Desktop.Models.Instance;

/// <summary>
/// Lightweight view model representing a mod pack (project) for display in the desktop UI.
/// </summary>
public class ModPackModel
{
    /// <summary>
    /// Human-readable title of the mod pack (project).
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Short description text provided by the project.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional icon for the project as a bitmap. May be null if the project has no icon or the fetch failed.
    /// </summary>
    public BitmapEntry Icon { get; set; }
    
    /// <summary>
    /// URL to the project's icon image
    /// </summary>
    public string? IconUrl { get; set; }
    
    /// <summary>
    /// The project's body converted to HTML. Intended to be rendered in the UI.
    /// </summary>
    public string RawPage { get; set; } =  string.Empty;

    /// <summary>
    /// Collection of versions associated with the project.
    /// </summary>
    public ObservableCollection<Version> Versions { get; set; } = [];

    /// <summary>
    /// Categories/tags associated with the project.
    /// </summary>
    public ObservableCollection<string> Tags { get; set; } = [];
    
    /// <summary>
    /// Create a <see cref="ModPackModel"/> from a Modrinth <see cref="Project"/> and a list of <see cref="Version"/>s.
    /// </summary>
    /// <param name="project">The Modrinth project model to convert.</param>
    /// <param name="versions">The versions for the project to include in the model.</param>
    /// <returns>A task that completes with a populated <see cref="ModPackModel"/>.</returns>
    public static async Task<ModPackModel> FromModrinthProjectAsync(Project project, List<Version> versions)
    {
        var fallback = Task.FromResult(new BitmapEntry(null, null));
        var iconTask = project.IconUrl != null 
            ? Program.ServiceProvider.GetRequiredService<IMetaCacheService>().GetImageAsync(project.IconUrl)!
            : fallback;
        
        string rawPage = Markdown.ToHtml(project.Body);
        
        return new ModPackModel
        {
            Name = project.Title,
            Description = project.Description,
            Icon = await iconTask,
            IconUrl = project.IconUrl,
            RawPage = rawPage,
            Versions = new ObservableCollection<Version>(versions),
            Tags = new ObservableCollection<string>(project.Categories)
        };
    }
}