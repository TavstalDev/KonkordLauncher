using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tavstal.BeaconSDK.Helpers.IO;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Constants;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;
using IconDataModel = Tavstal.KonkordLauncher.Desktop.Models.Domain.IconDataModel;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

public partial class IconSelectorViewModel : KonkordObservableObject
{
    private readonly IBitmapService _bitmapService = null!;
    private readonly IFilePickerService _filePickerService = null!;
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IBeaconLogger _logger = null!;
    private readonly ITranslationService _translationService = null!;

    public IconSelectorViewModel()
    {
        SelectedIcon = null;
    }

    public IconSelectorViewModel(IBeaconLogger<IconSelectorViewModel> logger, ITranslationService translationService,
        ILauncherStore launcherStore, IBitmapService bitmapService, IFilePickerService filePickerService)
    {
        if (Design.IsDesignMode)
            return;

        _logger = logger;
        _translationService = translationService;
        _launcherStore = launcherStore;
        _bitmapService = bitmapService;
        _filePickerService = filePickerService;

        _ = InitAsync();
    }

    #region Interaction

    public Interaction<string?, Unit> CloseWindowInteraction { get; } = new();

    #endregion

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        SelectedIcon?.Image.Dispose(_bitmapService);
        foreach (var icon in Icons)
            icon.Image.Dispose(_bitmapService);

        Icons.Clear();
        SelectedIcon = null;
    }

    /// <summary>
    ///     Initializes the view model by loading available icons.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to allow the operation to be cancelled.</param>
    private async Task InitAsync(CancellationToken cancellationToken = default)
    {
        // Load available icons
        var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);
        var icons = Directory.GetFiles(settings.Launcher.IconsDirectoryPath);
        foreach (var iconPath in icons)
        {
            var bitmap = await _bitmapService.GetBitmapAsync(iconPath, BitmapSizes.IconSelectorIcon);
            Icons.Add(new IconDataModel(Path.GetFileName(iconPath), iconPath, bitmap));
        }
    }

    #region Observable Properties

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedIcon))]
    public partial IconDataModel? SelectedIcon { get; set; }

    public ObservableCollection<IconDataModel> Icons { get; } = [];

    public bool HasSelectedIcon => SelectedIcon != null;

    #endregion

    #region Commands

    [RelayCommand]
    private async Task CloseWindow()
    {
        await CloseWindowInteraction.Handle(null);
    }

    [RelayCommand]
    private async Task OkBtn()
    {
        await CloseWindowInteraction.Handle(SelectedIcon?.Path);
    }

    [RelayCommand]
    private async Task CancelBtn()
    {
        await CloseWindowInteraction.Handle(null);
    }

    [RelayCommand]
    private async Task AddBtnAsync()
    {
        var result = await _filePickerService.OpenFilesPickerAsync(_translationService.Translate("common.select.file"),
            "PNGs", ["*.png"]);
        if (result == null)
            return;

        var settings = await _launcherStore.GetSettingsAsync();
        foreach (var path in result)
            try
            {
                var fileName = Path.GetFileName(path);
                var newPath = Path.Combine(settings.Launcher.IconsDirectoryPath, fileName);
                if (File.Exists(newPath))
                    continue;
                File.Copy(path, newPath);

                var bitmap = await _bitmapService.GetBitmapAsync(newPath, BitmapSizes.IconSelectorIcon);
                Icons.Add(new IconDataModel(fileName, newPath, bitmap));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to copy file {path} to icons directory: {ex.Message}");
            }
    }

    [RelayCommand]
    private void RemoveBtn()
    {
        if (SelectedIcon == null)
            return;

        File.Delete(SelectedIcon.Path);

        var icon = Icons.FirstOrDefault(x => x == SelectedIcon);
        icon?.Image.Dispose(_bitmapService);
        Icons.Remove(SelectedIcon);

        SelectedIcon.Image.Dispose(_bitmapService);
        SelectedIcon = null;
    }

    /// <summary>
    ///     Opens the directory containing the icons in the system's file explorer.
    /// </summary>
    [RelayCommand]
    private async Task OpenDirectoryAsync()
    {
        var settings = await _launcherStore.GetSettingsAsync();
        FileSystemHelper.OpenFolderInFileExplorer(settings.Launcher.IconsDirectoryPath);
    }

    #endregion
}
