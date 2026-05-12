using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

public partial class ExportViewModel : KonkordObservableObject
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(ExportViewModel));
    public Instance Instance { get; }
    public EInstanceProvider Provider { get; }
    
    [ObservableProperty]
    public partial bool IsInitialized { get; set; }
    [ObservableProperty]
    public partial string InstanceName { get; set; }
    [ObservableProperty]
    public partial string InstanceVersion { get; set; }
    [ObservableProperty]
    public partial string InstanceSummary { get; set; }
    public ObservableCollection<FileNode> Items { get; } = new();
    
    #region Interactions
    public Interaction<Unit, Unit> MinimizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> MaximizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();
    #endregion

    public ExportViewModel(Instance? instance, EInstanceProvider provider)
    {
        Provider = provider;
        if (Design.IsDesignMode || instance == null) // Both indicates design mode
        {
            Items =
            [
                new FileNode("config", "config", true)
                {
                    Children = [ 
                        new FileNode("modA", "modA", true)
                        {
                            Children = [ 
                                new FileNode("modASubConfig.json", "modASubConfig.json", false)
                            ]
                        },
                        new FileNode("modA.json", "modA.json", false),
                        new FileNode("modB.json", "modB.json", false),
                        new FileNode("modC.json", "modC.json", false)
                    ]
                },
                new FileNode("mods", "mods", true)
                {
                    Children = [
                        new FileNode("modA.jar", "modA.jar", false),
                        new FileNode("modB.jar", "modB.jar", false),
                        new FileNode("modC.jar", "modC.jar", false)
                    ]
                },
                new FileNode("resourcepacks", "resourcepacks", true),
                new FileNode("shaderpacks", "shaderpacks", true),
                new FileNode("commands_history.txt", "commands_history.txt", false),
                new FileNode("options.txt", "options.txt", false),
                new FileNode("servers.dat", "server.dat", false)
            ];
            return;
        }

        Instance = instance;
        InstanceName = instance.Name;
        InstanceVersion = "1.0.0";
        _ = InitAsync();
    }
    
    private async Task InitAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        
        // Read files and directories
        string? instanceDir = Instance.GameDirectory;
        // Can't export empty instance
        if (string.IsNullOrEmpty(instanceDir) || !Directory.Exists(instanceDir))
            return;
        
        var directoryInfo = new DirectoryInfo(instanceDir);
        foreach (var subDir in directoryInfo.GetDirectories("*", SearchOption.TopDirectoryOnly))
        {
            var node = FileNode.FromDirectory(subDir.FullName);
            Items.Add(node);
        }

        foreach (var file in directoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly))
            Items.Add(new FileNode(file.Name, file.FullName, false));
        
        IsInitialized = true;
        _logger.Info("Finished loading instance files for export.");
    }
    
    #region Commands

    /// <summary>
    /// Requests the window to minimize by invoking the <see cref="MinimizeWindowInteraction"/> interaction.
    /// </summary>
    [RelayCommand]
    public async Task MinimizeWindow() => await MinimizeWindowInteraction.Handle(Unit.Default);

    /// <summary>
    /// Requests the window to toggle maximize/restore by invoking the <see cref="MaximizeWindowInteraction"/> interaction.
    /// </summary>
    [RelayCommand]
    public async Task MaximizeWindow() => await MaximizeWindowInteraction.Handle(Unit.Default);

    /// <summary>
    /// Requests the window to close by invoking the <see cref="CloseWindowInteraction"/> interaction.
    /// </summary>
    [RelayCommand]
    public async Task CloseWindow() => await CloseWindowInteraction.Handle(Unit.Default);

    [RelayCommand]
    public async Task ContinueExport()
    {
        // TODO
    }

    #endregion
}