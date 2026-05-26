using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Package;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

/// <summary>
/// ViewModel for the export instance dialog.
/// </summary>
public partial class ExportViewModel : KonkordObservableObject
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(ExportViewModel));
    public Instance Instance { get; }
    public EInstanceProvider Provider { get; }
    
    [ObservableProperty]
    public partial bool IsInitialized { get; set; }
    [ObservableProperty]
    public partial bool IsExporting { get; set; }
    [ObservableProperty]
    public partial string InstanceName { get; set; }
    [ObservableProperty]
    public partial string InstanceVersion { get; set; }
    [ObservableProperty]
    public partial string InstanceSummary { get; set; }
    public ObservableCollection<ObservableFileNode> Items { get; } = new();
    
    #region Interactions
    public Interaction<Unit, Unit> MinimizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> MaximizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();
    public Interaction<Unit, string?> OpenFolderPickerInteraction { get; } = new();
    public Interaction<Alert, Unit> ShowAlertDialogInteraction { get; } = new();
    #endregion

    /// <summary>
    /// Creates a new instance of <see cref="ExportViewModel"/>.
    /// </summary>
    /// <param name="instance">The instance to export; if null or in design mode, the ViewModel will populate sample data.</param>
    /// <param name="provider">The instance provider which influences export format (e.g. CurseForge).</param>
    public ExportViewModel(Instance? instance, EInstanceProvider provider)
    {
        Provider = provider;
        if (Design.IsDesignMode || instance == null) // Both indicates design mode
        {
            Items =
            [
                new ObservableFileNode("config", "config", true)
                {
                    Children = [ 
                        new ObservableFileNode("modA", "modA", true)
                        {
                            Children = [ 
                                new ObservableFileNode("modASubConfig.json", "modASubConfig.json", false)
                            ]
                        },
                        new ObservableFileNode("modA.json", "modA.json", false),
                        new ObservableFileNode("modB.json", "modB.json", false),
                        new ObservableFileNode("modC.json", "modC.json", false)
                    ]
                },
                new ObservableFileNode("mods", "mods", true)
                {
                    Children = [
                        new ObservableFileNode("modA.jar", "modA.jar", false),
                        new ObservableFileNode("modB.jar", "modB.jar", false),
                        new ObservableFileNode("modC.jar", "modC.jar", false)
                    ]
                },
                new ObservableFileNode("resourcepacks", "resourcepacks", true),
                new ObservableFileNode("shaderpacks", "shaderpacks", true),
                new ObservableFileNode("commands_history.txt", "commands_history.txt", false),
                new ObservableFileNode("options.txt", "options.txt", false),
                new ObservableFileNode("servers.dat", "server.dat", false)
            ];
            return;
        }
        
        Instance = instance;
        InstanceName = instance.Name;
        InstanceVersion = "1.0.0";
        _ = InitAsync();
    }
    
    /// <summary>
    /// Asynchronously initializes the ViewModel by scanning the instance directory for files and folders.
    /// Populates <see cref="Items"/> with <see cref="ObservableFileNode"/> entries.
    /// </summary>
    /// <returns>A task that completes when initialization is done.</returns>
    private async Task InitAsync()
    {
        await Task.Yield();
        
        // Read files and directories
        string? instanceDir = Instance.GameDirectory;
        // Can't export empty instance
        if (string.IsNullOrEmpty(instanceDir) || !Directory.Exists(instanceDir))
            return;

        ObservableCollection<ObservableFileNode> localItems = [];
        
        var directoryInfo = new DirectoryInfo(instanceDir);
        foreach (var subDir in directoryInfo.GetDirectories("*", SearchOption.TopDirectoryOnly))
        {
            var node = ObservableFileNode.FromDirectory(subDir.FullName);
            if (subDir.FullName.EndsWith("config"))
                node.IsChecked = true;
            localItems.Add(node);
        }

        foreach (var file in directoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly))
        {
            var node = new ObservableFileNode(file.Name, file.FullName, false);
            if (file.FullName.EndsWith("options.txt") || file.FullName.EndsWith("servers.dat"))
                node.IsChecked = true;
            localItems.Add(node);
        }
        
        localItems.OrderByDescending(x => x.IsDirectory).ThenBy(x => x.Name).ToList().ForEach(x =>
        {
            Items.Add(x);
        });

        IsInitialized = true;
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

    /// <summary>
    /// Continues the export process after the user confirmed the selection.
    /// </summary>
    [RelayCommand]
    public async Task ContinueExport()
    {
        if (string.IsNullOrEmpty(InstanceName) || string.IsNullOrEmpty(InstanceVersion))
        {
            await ShowAlertDialogInteraction.Handle(new Alert(TranslationManager.Translate("common.error"), TranslationManager.Translate("instance.export.alert.empty.name.or.version"), EAlertType.Error));
            return;
        }

        var directoryResult = await OpenFolderPickerInteraction.Handle(Unit.Default);
        if (string.IsNullOrEmpty(directoryResult))
        {
            await ShowAlertDialogInteraction.Handle(new Alert(TranslationManager.Translate("common.error"), TranslationManager.Translate("instance.export.alert.no.directory"), EAlertType.Error));
            return;
        }

        string exportPath;
        switch (Provider)
        {
            case EInstanceProvider.CURSE_FORGE:
            {
                exportPath = Path.Combine(directoryResult, $"{InstanceName}-{InstanceVersion}.zip");
                break;
            }
            default:
            {
                exportPath = Path.Combine(directoryResult, $"{InstanceName}-{InstanceVersion}.mrpack");
                break;
            }
        }
        
        if (File.Exists(exportPath))
        {
            await ShowAlertDialogInteraction.Handle(new Alert(TranslationManager.Translate("common.error"), TranslationManager.Translate("instance.export.alert.file.exists"), EAlertType.Error));
            return;
        }

        IsExporting = true;
        List<FileNode> selectedFiles = ObservableFileNode.ToFileNodes(Items.ToList());
        // TODO : User service
        // !await InstanceHelper.ExportAsync(Instance, selectedFiles, exportPath, Provider, InstanceVersion,
        // InstanceSummary)
        if (true)
        {
            IsExporting = false;
            await ShowAlertDialogInteraction.Handle(new Alert(TranslationManager.Translate("common.error"), TranslationManager.Translate("instance.export.alert.error"), EAlertType.Error));
            return;
        }
        IsExporting = false;
        
        await ShowAlertDialogInteraction.Handle(new Alert(TranslationManager.Translate("common.success"), TranslationManager.Translate("instance.export.alert.success", exportPath), EAlertType.Success));
        await CloseWindowInteraction.Handle(Unit.Default);
    }

    #endregion
}