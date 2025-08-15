using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models;
using IconSelectorViewModel = Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models.IconSelectorViewModel;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

/// <summary>
/// Represents a window for selecting and managing icons in the application.
/// </summary>
public partial class IconSelectorWindow : KonkordWindow
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

        this.DataContext = new IconSelectorViewModel(this);
    }
    
    /// <summary>
    /// Releases resources associated with the <see cref="IconSelectorWindow"/>.
    /// Logs a debug message indicating that memory is being freed.
    /// </summary>
    protected override void FreeMemory()
    {
        _logger.Debug("Freeing memory for IconSelectorWindow.");
    }
    
    /// <summary>
    /// Opens a file picker dialog to select image files and copies them to the icons directory.
    /// </summary>
    /// <returns>A list of tuples containing file names and their new paths, or null if no files were selected.</returns>
    public async Task<List<(string, string)>?> OpenFilePickerAsync()
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
}