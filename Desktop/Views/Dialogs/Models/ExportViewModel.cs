using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Services.Instances;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

/// <summary>
///     ViewModel for the export instance dialog.
/// </summary>
public partial class ExportViewModel : KonkordObservableObject
{
    private readonly CurseForgePackageService _curseForgePackageService = null!;
    private readonly ModrinthPackageService _modrinthPackageService = null!;
    private readonly ITranslationService _translationService = null!;
    private readonly IDialogService _dialogService = null!;
    private readonly IFilePickerService _filePickerService = null!;

    public ExportViewModel()
    {
        Provider = EInstanceProvider.MODRINTH;
        InstanceName = "Design Instance";
        InstanceVersion = "1.21.1";
        InstanceSummary = "Design-time instance";
    }

    public ExportViewModel(Instance? instance, EInstanceProvider provider,
        ITranslationService translationService, ModrinthPackageService modrinthPackageService,
        CurseForgePackageService curseForgePackageService, IDialogService dialogService,
        IFilePickerService filePickerService)
    {
        Provider = provider;
        _translationService = translationService;
        _modrinthPackageService = modrinthPackageService;
        _curseForgePackageService = curseForgePackageService;
        _filePickerService = filePickerService;
        _dialogService = dialogService;

        if (Design.IsDesignMode || instance == null) // Both indicates design mode
        {
            Items =
            [
                new ObservableFileNode("config", "config", true)
                {
                    Children =
                    [
                        new ObservableFileNode("modA", "modA", true)
                        {
                            Children =
                            [
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
                    Children =
                    [
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

    private Instance Instance { get; } = null!;
    private EInstanceProvider Provider { get; }

    [ObservableProperty] public partial bool IsInitialized { get; set; }

    [ObservableProperty] public partial bool IsExporting { get; set; }

    [ObservableProperty] public partial string InstanceName { get; set; } = null!;

    [ObservableProperty] public partial string InstanceVersion { get; set; } = null!;

    [ObservableProperty] public partial string InstanceSummary { get; set; } = null!;

    public ObservableCollection<ObservableFileNode> Items { get; } = [];

    /// <summary>
    ///     Asynchronously initializes the ViewModel by scanning the instance directory for files and folders.
    ///     Populates <see cref="Items" /> with <see cref="ObservableFileNode" /> entries.
    /// </summary>
    /// <returns>A task that completes when initialization is done.</returns>
    private async Task InitAsync()
    {
        await Task.Yield();

        // Read files and directories
        var instanceDir = Instance.GameDirectory;
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

    #region Interactions

    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();

    #endregion

    #region Commands

    /// <summary>
    ///     Requests the window to close by invoking the <see cref="CloseWindowInteraction" /> interaction.
    /// </summary>
    [RelayCommand]
    private async Task CloseWindow()
    {
        await CloseWindowInteraction.Handle(Unit.Default);
    }

    /// <summary>
    ///     Continues the export process after the user confirmed the selection.
    /// </summary>
    [RelayCommand]
    private async Task ContinueExport()
    {
        if (string.IsNullOrEmpty(InstanceName) || string.IsNullOrEmpty(InstanceVersion))
        {
            await _dialogService.ShowAlertAsync(
                _translationService.Translate("common.error"),
                _translationService.Translate("instance.export.alert.empty.nameOrVersion"),
                EAlertType.Error);
            return;
        }

        var directoryResult = await _filePickerService.OpenFolderPickerAsync(_translationService.Translate("common.select.directory"));
        if (string.IsNullOrEmpty(directoryResult))
        {
            await _dialogService.ShowAlertAsync(
                _translationService.Translate("common.error"),
                _translationService.Translate("instance.export.alert.noDirectory"),
                EAlertType.Error);
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
            await _dialogService.ShowAlertAsync(
                _translationService.Translate("common.error"),
                _translationService.Translate("instance.export.alert.fileExists"),
                EAlertType.Error);
            return;
        }

        IsExporting = true;
        var selectedFiles = ObservableFileNode.ToFileNodes(Items.ToList());
        switch (Provider)
        {
            case EInstanceProvider.CURSE_FORGE:
            {
                if (await _curseForgePackageService.ExportAsync(Instance, selectedFiles, exportPath, InstanceVersion,
                        InstanceSummary))
                {
                    IsExporting = false;
                    await _dialogService.ShowAlertAsync(
                        _translationService.Translate("common.error"),
                        _translationService.Translate("instance.export.alert.error"),
                        EAlertType.Error);
                    return;
                }

                break;
            }
            case EInstanceProvider.MODRINTH:
            {
                if (await _modrinthPackageService.ExportAsync(Instance, selectedFiles, exportPath, InstanceVersion,
                        InstanceSummary))
                {
                    IsExporting = false;
                    await _dialogService.ShowAlertAsync(
                        _translationService.Translate("common.error"),
                        _translationService.Translate("instance.export.alert.error"),
                        EAlertType.Error);
                    return;
                }

                break;
            }
        }

        IsExporting = false;

        await _dialogService.ShowAlertAsync(
            _translationService.Translate("common.success"),
            _translationService.Translate("instance.export.alert.success", exportPath),
            EAlertType.Success);
        await CloseWindowInteraction.Handle(Unit.Default);
    }

    #endregion
}
