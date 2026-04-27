using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;

namespace Tavstal.KonkordLauncher.Desktop.Models.Avalonia;

public abstract class KonkordWindow<TViewModel> : ReactiveWindow<TViewModel> where TViewModel : class
{
    public new TViewModel? DataContext
    {
        get => (TViewModel?)base.DataContext;
        protected init => base.DataContext = value;
    }
    
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        (DataContext as IDisposable)?.Dispose();
        base.OnClosing(e);
    }
    
    protected void DragStart_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }
    
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
    
    
    protected async Task SetClipboardTextAsync(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        
        var topLevel = GetTopLevel(this);
        if (topLevel?.Clipboard == null)
            return;

        await topLevel.Clipboard.SetTextAsync(text);
    }
    
    protected async Task<string?> OpenFilePickerAsync(string title, string filterName, string[] patterns)
    {
        if (VisualRoot is not TopLevel topLevel)
            return null;

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
        return !files.Any() ? null : files[0].Path.AbsolutePath;
    }
    
    protected async Task<string?> OpenFolderPickerAsync(string title)
    {
        if (VisualRoot is not TopLevel topLevel)
            return null;

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
        return selectedFolder?.Path.AbsolutePath;
    }
}