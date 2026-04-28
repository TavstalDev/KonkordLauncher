using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Markdig;
using Modrinth.Models;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Desktop.Helpers;

namespace Tavstal.KonkordLauncher.Desktop.Models.Instance;

public class ModPackModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Bitmap? Icon { get; set; }
    public string RawPage { get; set; }
    public ObservableCollection<string> Versions { get; set; }
    public ObservableCollection<string> Tags { get; set; }
    
    public static async Task<ModPackModel> FromModrinthProjectAsync(SearchResult project)
    {
        return await Task.Run(async () => 
        {
            var iconTask = project.IconUrl != null 
                ? ImageHelper.LoadFromWeb(new Uri(project.IconUrl)) 
                : Task.FromResult<Bitmap?>(null);
            
            var dataTask = ModrinthHelper.GetProjectAsync(project.ProjectId);

            await Task.WhenAll(iconTask, dataTask);

            var projectData = await dataTask;
            string rawPage = projectData != null 
                ? Markdown.ToHtml(projectData.Body) 
                : "No body was provided.";

            return new ModPackModel
            {
                Name = project.Title ?? "Unknown",
                Description = project.Description ?? "Unknown description",
                Icon = await iconTask,
                RawPage = rawPage,
                Versions = new ObservableCollection<string>(project.Versions),
                Tags = new ObservableCollection<string>(project.Categories)
            };
        });
    }
}