using System.Collections.ObjectModel;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;

namespace Tavstal.KonkordLauncher.Desktop.ViewModels;

public class EditInstanceViewModel : ViewModelBase
{
    public ObservableCollection<ModModel> Mods { get; set; }
    
    public ObservableCollection<ResourcePackModel> ResourcePacks { get; set; }
    
    public ObservableCollection<ShaderPackModel> ShaderPacks { get; set; }
    
    public ObservableCollection<WorldModel> Worlds { get; set; }
    
    public ObservableCollection<ServerModel> Servers { get; set; }
    
    public ObservableCollection<ScreenshotModel> Screenshots { get; set; }

    public ObservableDictionary<string, string> EnvironmentVariables { get; set; } = new();
    
    public EditInstanceViewModel()
    {
        Mods =
        [
            new()
            {
                IsEnabled = true,
                Name = "Mod 1",
                LastModified = "Yesterday",
                Provider = "Provider A",
                Size = 123456789,
                Version = "1.0.0",
            }
        ];
        ResourcePacks =
        [
            new()
            {
                Name = "Resource Pack 1",
                LastModified = "Yesterday",
                Provider = "Provider A",
                Size = 123456789,
                Version = "1.0.0",
            }
        ];
        ShaderPacks =
        [
            new()
            {
                Name = "Shader Pack 1",
                LastModified = "Yesterday",
                Provider = "Provider A",
                Size = 123456789,
                Version = "1.0.0",
            }
        ];
        Worlds =
        [
            new()
            {
                Name = "World 1",
                Gamemode = "Survival",
                LastPlayed = "Today",
                Size = 123456789,
            }
        ];
        Servers =
        [
            new()
            {
                Name = "Server 1",
                Address = "server1.example.com"
            }
        ];
        EnvironmentVariables =
        [
            new ("JAVA_HOME", "C:\\Program Files\\Java\\jdk-17"),
        ];
    }
}