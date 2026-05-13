using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using ReactiveUI.Avalonia;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;

namespace Tavstal.KonkordLauncher.Desktop.Models.Avalonia;

/// <summary>
/// Base window class used across the Konkord desktop UI that extends ReactiveUI's <see cref="ReactiveWindow{TViewModel}"/>.
/// Provides common window helpers (drag/resize, clipboard helpers, file/folder pickers) and ensures the view-model
/// is disposed when the window closes.
/// </summary>
/// <typeparam name="TViewModel">Type of the view model assigned to the window.</typeparam>
public abstract class KonkordWindow<TViewModel> : ReactiveWindow<TViewModel> where TViewModel : class
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(KonkordWindow<TViewModel>));
    
    /// <summary>
    /// Strongly-typed DataContext for this window. The setter is protected-init only to allow derived
    /// windows to assign a view-model during construction while still exposing the typed getter.
    /// </summary>
    public new TViewModel? DataContext
    {
        get => (TViewModel?)base.DataContext;
        protected init => base.DataContext = value;
    }
    
    /// <summary>
    /// Called when the window is closing. If the assigned <see cref="DataContext"/> implements <see cref="IDisposable"/>,
    /// it will be disposed here to release resources held by the view-model.
    /// </summary>
    /// <param name="e">Window closing event arguments.</param>
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        (DataContext as IDisposable)?.Dispose();
        base.OnClosing(e);
    }
    
    /// <summary>
    /// Handler to start moving the window when the left mouse button is pressed on a draggable surface.
    /// Intended to be wired to pointer pressed events on title bars or custom drag handles.
    /// </summary>
    /// <param name="sender">Event source (unused).</param>
    /// <param name="e">Pointer event args.</param>
    protected void DragStart_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }
    
    /// <summary>
    /// Handler to begin a resize drag when the user presses on a resize handle control.
    /// The control's <see cref="Control.Tag"/> must contain a valid <see cref="WindowEdge"/> value (case-insensitive).
    /// </summary>
    /// <param name="sender">Expected to be a <see cref="Control"/> with a string Tag identifying the window edge.</param>
    /// <param name="e">Pointer event arguments.</param>
    protected void ResizeHandle_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!CanResize)
            return;
    
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;
    
        if (sender is not Control { Tag: string tag })
            return;
    
        if (!Enum.TryParse(tag, ignoreCase: true, out WindowEdge edge))
            return;
    
        BeginResizeDrag(edge, e);
        e.Handled = true; // prevent bubbling to DragStart_PointerPressed
    }
    
    
    /// <summary>
    /// Sets plain text on the system clipboard asynchronously.
    /// Safely returns without throwing when the provided text is null/empty or clipboard is unavailable.
    /// </summary>
    /// <param name="text">Text to set on the clipboard.</param>
    protected async Task SetClipboardTextAsync(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        
        var topLevel = GetTopLevel(this);
        if (topLevel?.Clipboard == null)
        {
            _logger.Error("Unable to set clipboard text: TopLevel or Clipboard is null.");
            return;
        }

        await topLevel.Clipboard.SetTextAsync(text);
    }
    
    /// <summary>
    /// Copies the provided screenshot image to the system clipboard as a PNG byte payload.
    /// </summary>
    /// <param name="screenshot">Screenshot model that contains an image to copy. If <see cref="ScreenshotModel.Image"/> is null, no action is taken.</param>
    protected async Task SetClipboardImageAsync(ScreenshotModel screenshot)
    {
        if (screenshot.Image == null)
            return;

        var topLevel = GetTopLevel(this);
        if (topLevel == null)
        {
            _logger.Error("Unable to set clipboard image: TopLevel is null.");
            return;
        }
        
        var clipboard = topLevel.Clipboard;
        if (clipboard == null)
            return;

        using var ms = new MemoryStream();
        screenshot.Image.Save(ms);

        var fileFormat = DataFormat.CreateBytesApplicationFormat("image/png");
        var item = DataTransferItem.Create(fileFormat, ms.ToArray());

        var transfer = new DataTransfer();
        transfer.Add(item);
        await clipboard.SetDataAsync(transfer);
    }
    
    /// <summary>
    /// Opens a native file picker dialog and returns a single selected file path or null.
    /// </summary>
    /// <param name="title">Dialog title to display.</param>
    /// <param name="filterName">User-visible filter name (e.g. "Images").</param>
    /// <param name="patterns">File name patterns (e.g. "*.png", "*.jpg") used by the filter.</param>
    /// <returns>Local file path of the selected file, or null if none selected or an error occurred.</returns>
    protected async Task<string?> OpenFilePickerAsync(string title, string filterName, string[] patterns)
    {
        try
        {
            var topLevel = GetTopLevel(this);
            if (topLevel == null)
            {
                _logger.Error("Unable to open file picker: TopLevel is null.");
                return null;
            }

            var storageProvider = topLevel.StorageProvider;

            var options = new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>
                {
                    new(filterName)
                    {
                        Patterns = patterns
                    }
                }
            };

            var files = await storageProvider.OpenFilePickerAsync(options);
            return !files.Any() ? null : files[0].TryGetLocalPath();
        }
        catch (Exception ex)
        {
            _logger.Error($"Error opening file picker: {ex}");
            return null;
        }
    }
    
    /// <summary>
    /// Opens a native folder picker dialog and returns the selected folder path or null.
    /// </summary>
    /// <param name="title">Dialog title to display.</param>
    /// <returns>Local folder path of the selected folder, or null if none selected.</returns>
    /// <exception cref="NotSupportedException">Thrown when the platform storage provider does not support folder picking.</exception>
    protected async Task<string?> OpenFolderPickerAsync(string title)
    {
        var topLevel = GetTopLevel(this);
        if (topLevel == null)
        {
            _logger.Error("Unable to open folder picker: TopLevel is null.");
            return null;
        }

        var storageProvider = topLevel.StorageProvider;
        
        if (!storageProvider.CanPickFolder)
            throw new NotSupportedException("No folder was selected.");
        
        var options = new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        };
        
        IReadOnlyList<IStorageFolder> folders = await storageProvider.OpenFolderPickerAsync(options);
        
        if (!folders.Any())
            return null;
        
        IStorageFolder? selectedFolder = folders.FirstOrDefault();
        return selectedFolder?.TryGetLocalPath();
    }
}