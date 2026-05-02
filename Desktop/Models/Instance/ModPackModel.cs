using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Markdig;
using Modrinth.Models;
using Tavstal.KonkordLauncher.Common.Helpers;

namespace Tavstal.KonkordLauncher.Desktop.Models.Instance;

public class ModPackModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Bitmap? Icon { get; set; }
    public string RawPage { get; set; }
    public ObservableCollection<Version> Versions { get; set; }
    public ObservableCollection<string> Tags { get; set; }
    
    public static async Task<ModPackModel> FromModrinthProjectAsync(Project project, List<Version> versions)
    {
        return await Task.Run(async () => 
        {
            var iconTask = project.IconUrl != null 
                ? MetaCacheHelper.GetImageAsync(project.IconUrl) 
                : Task.FromResult<Bitmap?>(null);
            
            string rawPage = Markdown.ToHtml(project.Body);

            return new ModPackModel
            {
                Name = project.Title,
                Description = project.Description,
                Icon = await iconTask,
                RawPage = rawPage,
                Versions = new ObservableCollection<Version>(versions),
                Tags = new ObservableCollection<string>(project.Categories)
            };
        });
    }
}