using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using IconSelectorViewModel = Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models.IconSelectorViewModel;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

/// <summary>
/// Represents a window for selecting and managing icons in the application.
/// </summary>
public partial class IconSelectorWindow : KonkordWindow<IconSelectorViewModel>
{
    private readonly ICustomLogger _logger;
    private readonly ITranslationService _translationService;
    private readonly ILauncherStore _launcherStore;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="IconSelectorWindow"/> class.
    /// Sets up the DataContext and handles language changes.
    /// </summary>
    public IconSelectorWindow()
    {
        var services = Program.ServiceProvider;
        _logger = services.GetRequiredService<ICustomLogger<IconSelectorWindow>>();
        _translationService = services.GetRequiredService<ITranslationService>();
        _launcherStore = services.GetRequiredService<ILauncherStore>();
        InitializeComponent();

        DataContext = new IconSelectorViewModel();
        this.WhenActivated(disposables =>
        {
            DataContext.MinimizeWindowInteraction.RegisterHandler(action =>
            {
                WindowState = WindowState.Minimized;
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.MaximizeWindowInteraction.RegisterHandler(action =>
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close(action.Input);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.ShowFilePicker.RegisterHandler(async action =>
            {
                var result = await OpenFilePickerAsync();
                action.SetOutput(result);
            }).DisposeWith(disposables);
        });
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
            _logger.LogError("Folder picking is not supported on this platform.");
            return null;
        }
        
        var options = new FilePickerOpenOptions
        {
            Title = _translationService.Translate("common.select.file"),
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

        var settings = await _launcherStore.GetSettingsAsync();
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
                _logger.LogError($"Failed to copy file {file.Name} to icons directory: {ex.Message}");
            }
        }
        
        return result;
    }
}