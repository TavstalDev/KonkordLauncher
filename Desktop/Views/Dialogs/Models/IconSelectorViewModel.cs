using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Desktop.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

/// <summary>
/// ViewModel for managing and selecting icons in the application.
/// </summary>
public partial class IconSelectorViewModel : KonkordObservableObject
{
    private IconSelectorWindow? _parentWindow;
    
    /// <summary>
    /// The currently selected icon.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedIcon))]
    private IconDataModel? _selectedIcon;

    /// <summary>
    /// The collection of available icons.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<IconDataModel> _icons;

    /// <summary>
    /// Indicates whether an icon is currently selected.
    /// </summary>
    public bool HasSelectedIcon => SelectedIcon != null;

    /// <summary>
    /// Initializes a new instance of the <see cref="IconSelectorViewModel"/> class.
    /// Loads available icons from the configured directory.
    /// </summary>
    public IconSelectorViewModel(IconSelectorWindow parentWindow)
    {
        _parentWindow = parentWindow;
        _icons = [];
        _selectedIcon = null;

        // Load available icons
        var settings = LauncherHelper.GetLauncherSettings();
        var icons = Directory.GetFiles(settings.Launcher.IconsDirectoryPath);
        foreach (var iconPath in icons)
        {
            var bitmap = new Bitmap(iconPath);
            _icons.Add(new IconDataModel(Path.GetFileName(iconPath), iconPath, bitmap));
        }

        settings = null;
        icons = null;
    }

    /// <summary>
    /// Releases resources associated with the <see cref="IconSelectorViewModel"/>.
    /// Disposes of the selected icon's image and all icons in the collection, 
    /// then clears the collection and resets the selected icon.
    /// </summary>
    public override void FreeMemory()
    {
        SelectedIcon?.Image.Dispose();
        foreach (var icon in Icons)
            icon.Image.Dispose();
        Icons = [];
        SelectedIcon = null;
        _parentWindow = null;
    }
    
    #region Commands

    /// <summary>
    /// Closes the parent window and returns the currently selected icon as the result.
    /// </summary>
    [RelayCommand]
    public void OkBtn() => _parentWindow?.Close(SelectedIcon);

    /// <summary>
    /// Closes the parent window without returning any result.
    /// </summary>
    [RelayCommand]
    public void CancelBtn() => _parentWindow?.Close(null);

    /// <summary>
    /// Opens a file picker dialog to allow the user to add new icons.
    /// Adds the selected icons to the collection after loading their bitmap data.
    /// </summary>
    [RelayCommand]
    public async Task AddBtnAsync()
    {
        if (_parentWindow == null)
            return;
        
        var result = await _parentWindow.OpenFilePickerAsync();
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
        settings = null;
    }
    #endregion
}