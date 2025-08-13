using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models;
using IconSelectorViewModel = Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models.IconSelectorViewModel;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

/// <summary>
/// Represents a window for selecting and managing icons in the application.
/// </summary>
public partial class IconSelectorWindow : Window
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(IconSelectorWindow));
    
    /// <summary>
    /// Initializes a new instance of the <see cref="IconSelectorWindow"/> class.
    /// Sets up the DataContext and handles language changes.
    /// </summary>
    public IconSelectorWindow()
    {
        InitializeComponent();

#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif

        this.DataContext = new IconSelectorViewModel();
    }
    
    /// <summary>
    /// Opens a file picker dialog to select image files and copies them to the icons directory.
    /// </summary>
    /// <returns>A list of tuples containing file names and their new paths, or null if no files were selected.</returns>
    private async Task<List<(string, string)>?> OpenFilePickerAsync()
    {
        // Ensure the VisualRoot is a TopLevel object
        if (VisualRoot is not TopLevel topLevel)
            return null;

        var storageProvider = topLevel.StorageProvider;

        // Check if folder picking is supported on the current platform
        if (!storageProvider.CanPickFolder)
        {
            _logger.Error("Folder picking is not supported on this platform.");
            return null;
        }
        
        var options = new FilePickerOpenOptions
        {
            Title = TranslationManager.Translate("common.select.file"),
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>
            {
                new("PNG Images")
                {
                    Patterns = new List<string> { "*.png" }
                }
            }
        };
        
        var files = await storageProvider.OpenFilePickerAsync(options);
        if (!files.Any())
            return null;

        var settings = await LauncherHelper.GetLauncherSettingsAsync();
        List<(string, string)> result = [];
        foreach (var file in files)
        {
            try
            {
                var newPath = System.IO.Path.Combine(settings.Launcher.IconsDirectoryPath, file.Name);
                if (System.IO.File.Exists(newPath))
                    continue;
                System.IO.File.Copy(file.Path.AbsolutePath, newPath);
                result.Add(new(file.Name, newPath));
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to copy file {file.Name} to icons directory: {ex.Message}");
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Handles the click event for the OK button.
    /// Closes the window and returns the selected icon.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void OkBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        if (this.DataContext is not IconSelectorViewModel vm)
            return;

        this.Close(vm.SelectedIcon);
    }
    
    /// <summary>
    /// Handles the click event for the Cancel button.
    /// Closes the window without returning a selected icon.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void CancelBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        this.Close(null);
    }

    /// <summary>
    /// Handles the click event for the Add button.
    /// Opens the folder picker, adds selected icons to the ViewModel, and updates the UI.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private async void AddBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        // TODO: Replace async void
        if (this.DataContext is not IconSelectorViewModel vm)
            return;
    
        var result = await OpenFilePickerAsync();
        if (result == null)
            return;

        foreach (var elem in result)
        {
            var bitmap = new Bitmap(elem.Item2);
            vm.Icons.Add(new IconDataModel(elem.Item1, elem.Item2, bitmap));
        }
    }

    /// <summary>
    /// Handles the click event for the Remove button.
    /// Deletes the selected icon from the file system and removes it from the ViewModel.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void RemoveBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        if (this.DataContext is not IconSelectorViewModel vm)
            return;
        
        if (vm.SelectedIcon == null)
            return;

        System.IO.File.Delete(vm.SelectedIcon.Path);
        vm.Icons.Remove(vm.SelectedIcon);
        vm.SelectedIcon = null;
    }

    /// <summary>
    /// Handles the click event for the Open Folder button.
    /// Opens the icons directory in the file explorer.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void OpenFolderBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        var settings = LauncherHelper.GetLauncherSettings();
        FileSystemHelper.OpenFolderInFileExplorer(settings.Launcher.IconsDirectoryPath);
    }
}