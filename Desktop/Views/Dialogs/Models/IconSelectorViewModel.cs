using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using IconDataModel = Tavstal.KonkordLauncher.Desktop.Models.Domain.IconDataModel;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

/// <summary>
/// ViewModel for managing and selecting icons in the application.
/// </summary>
public partial class IconSelectorViewModel : KonkordObservableObject
{
    #region Observable Properties
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedIcon))]
    private IconDataModel? _selectedIcon;

    public ObservableCollection<IconDataModel> Icons { get; } = new();
    
    public bool HasSelectedIcon => SelectedIcon != null;
    #endregion
    
    #region Interaction
    public Interaction<Unit, Unit> MinimizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> MaximizeWindowInteraction { get; } = new();
    public Interaction<string?, Unit> CloseWindowInteraction { get; } = new();
    public Interaction<Unit, List<(string, string)>?> ShowFilePicker { get; }  = new();
    #endregion
    
    
    /// <summary>
    /// Initializes a new instance of the <see cref="IconSelectorViewModel"/> class.
    /// Loads the available icons from the configured directory and populates the icons collection.
    /// </summary>
    public IconSelectorViewModel()
    {
        _ = InitAsync();
    }
    
    /// <summary>
    /// Releases the resources used by the <see cref="IconSelectorViewModel"/> and performs cleanup operations.
    /// </summary>
    /// <param name="disposing">
    /// A boolean value indicating whether the method is being called directly or indirectly by a finalizer.
    /// If true, the method has been called directly or indirectly by a user's code. Managed and unmanaged resources can be disposed.
    /// If false, the method has been called by the runtime from inside the finalizer, and only unmanaged resources can be disposed.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        SelectedIcon?.Image.Dispose();
        foreach (var icon in Icons)
            icon.Image.Dispose();
        Icons.Clear();
        SelectedIcon = null;
    }

    private async Task InitAsync(CancellationToken cancellationToken = default)
    {
        // Load available icons
        var settings = await LauncherHelper.GetLauncherSettingsAsync(cancellationToken);
        var icons = Directory.GetFiles(settings.Launcher.IconsDirectoryPath);
        foreach (var iconPath in icons)
        {
            var bitmap = new Bitmap(iconPath);
            Icons.Add(new IconDataModel(Path.GetFileName(iconPath), iconPath, bitmap));
        }
    }
    
    #region Commands
    [RelayCommand]
    public async Task MinimizeWindow() => await MinimizeWindowInteraction.Handle(Unit.Default);

    [RelayCommand]
    public async Task MaximizeWindow() => await MaximizeWindowInteraction.Handle(Unit.Default);

    [RelayCommand]
    public async Task CloseWindow() => await CloseWindowInteraction.Handle(null);

    /// <summary>
    /// Closes the parent window and returns the currently selected icon as the result.
    /// </summary>
    [RelayCommand]
    public async Task OkBtn() => await CloseWindowInteraction.Handle(SelectedIcon?.Path);

    /// <summary>
    /// Closes the parent window without returning any result.
    /// </summary>
    [RelayCommand]
    public async Task CancelBtn() => await CloseWindowInteraction.Handle(null);

    /// <summary>
    /// Opens a file picker dialog to allow the user to add new icons.
    /// Adds the selected icons to the collection after loading their bitmap data.
    /// </summary>
    [RelayCommand]
    public async Task AddBtnAsync()
    {
        var result = await ShowFilePicker.Handle(Unit.Default);
        if (result == null)
            return;

        foreach (var elem in result)
        {
            var bitmap = new Bitmap(elem.Item2);
            Icons.Add(new IconDataModel(elem.Item1, elem.Item2, bitmap));
        }
    }

    /// <summary>
    /// Removes the currently selected icon from the collection and deletes its file.
    /// Disposes of the icon's image and resets the selected icon.
    /// </summary>
    [RelayCommand]
    public void RemoveBtn()
    {
        if (SelectedIcon == null)
            return;

        File.Delete(SelectedIcon.Path);
        
        var icon = Icons.FirstOrDefault(x => x == SelectedIcon);
        icon?.Image.Dispose();
        Icons.Remove(SelectedIcon);
        
        SelectedIcon.Image.Dispose();
        SelectedIcon = null;
    }
    
    /// <summary>
    /// Opens the directory containing the icons in the system's file explorer.
    /// </summary>
    [RelayCommand]
    public async Task OpenDirectoryAsync()
    {
        var settings = await LauncherHelper.GetLauncherSettingsAsync();
        FileSystemHelper.OpenFolderInFileExplorer(settings.Launcher.IconsDirectoryPath);
    }
    #endregion
}