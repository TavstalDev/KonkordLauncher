using System.Threading.Tasks;
using Modrinth.Models;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Services.Content;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;

namespace Tavstal.KonkordLauncher.Desktop.Models.Instance;

public class ModPackPreviewModel
{
    public string ProjectId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public BitmapEntry Icon { get; set; } = new(null, null);

    public string[] GameVersions { get; set; } = [];

    public string[] Tags { get; set; } = [];

    public static async Task<ModPackPreviewModel> FromModrinthProjectAsync(Project project,
        IMetaCacheService metaCacheService, IBitmapService bitmapService)
    {
        BitmapEntry icon;
        if (string.IsNullOrEmpty(project.IconUrl))
            icon = await bitmapService.GetBitmapAsync("avares://KonkordLauncher/Assets/Images/default_world.png", 35);
        else
        {
            var localIcon = await metaCacheService.GetImageAsync(project.IconUrl);
            if (localIcon == null)
                icon = await bitmapService.GetBitmapAsync("avares://KonkordLauncher/Assets/Images/default_world.png",
                    35);
            else
                icon = localIcon;
        }

        return new ModPackPreviewModel
        {
            ProjectId = project.Id,
            Name = project.Title,
            Description = project.Description,
            Icon = icon,
            GameVersions = project.GameVersions,
            Tags = [.. project.Categories]
        };
    }

    public void Dispose(IBitmapService bitmapService)
    {
        Icon.Dispose(bitmapService);
    }
}
