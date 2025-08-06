using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Desktop.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

/// <summary>
/// ViewModel for managing and selecting icons in the application.
/// </summary>
public partial class IconSelectorViewModel : ObservableObject
{
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
    public IconSelectorViewModel()
    {
        _icons = new ObservableCollection<IconDataModel>();
        _selectedIcon = null;

        // Load available icons
        var settings = LauncherHelper.GetLauncherSettings();
        var icons = Directory.GetFiles(settings.Launcher.IconsDirectoryPath);
        foreach (var iconPath in icons)
        {
            var bitmap = new Bitmap(iconPath);
            _icons.Add(new IconDataModel(Path.GetFileName(iconPath), iconPath, bitmap));
        }
    }
}