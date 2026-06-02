using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Markdig;
using Microsoft.Extensions.DependencyInjection;
using Modrinth.Models;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.IO;

namespace Tavstal.KonkordLauncher.Desktop.Models.Instance;

public partial class ResourceBaseModel : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;
    
    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;
    
    [ObservableProperty]
    public partial string RawPage { get; set; } = string.Empty;
    
    [ObservableProperty]
    public partial Bitmap? Icon { get; set; }
    
    [ObservableProperty]
    public partial string? IconUrl { get; set; }
    
    [ObservableProperty]
    public partial string? FilePath { get; set; }
    
    [ObservableProperty, NotifyPropertyChangedFor(nameof(FormattedSize))]
    public partial long FileSize { get; set; }
    
    [ObservableProperty]
    public partial bool IsEnabled { get; set; }
    
    [ObservableProperty]
    public partial bool IsSelected { get; set; }
    
    [ObservableProperty]
    public partial bool IsInstalled { get; set; }
    
    [ObservableProperty]
    public partial EPlatformType? Platform { get; set; }
    
    [ObservableProperty]
    public partial string? ProjectId { get; set; }

    public ObservableCollection<Version> Versions { get; set; } = [];
    
    public ObservableCollection<string> Tags { get; set; } = [];
    
    public string FormattedSize => FileSystemHelper.GetFormattedSize(FileSize);
    
    public static async Task<ResourceBaseModel> FromModrinthProjectAsync(Project project, List<Version> versions)
    {
        return await Task.Run(async () => 
        {
            var iconTask = project.IconUrl != null 
                ? Program.ServiceProvider.GetRequiredService<IMetaCacheService>().GetImageAsync(project.IconUrl) 
                : Task.FromResult<Bitmap?>(null);
            
            string rawPage = Markdown.ToHtml(project.Body);
            
            return new ResourceBaseModel
            {
                ProjectId = project.Id,
                Name = project.Title,
                Description = project.Description,
                Icon = await iconTask,
                IconUrl = project.IconUrl,
                RawPage = rawPage,
                Versions = new ObservableCollection<Version>(versions),
                Tags = new ObservableCollection<string>(project.Categories),
                IsEnabled = true,
                Platform = EPlatformType.MODRINTH,
                FilePath = null
            };
        });
    }
}