using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;

namespace Tavstal.KonkordLauncher.Desktop.Services.Storage;

/// <inheritdoc/>
public class FilePickerService : IFilePickerService
{
    private readonly ITopLevelProvider _topLevelProvider;

    public FilePickerService(ITopLevelProvider topLevelProvider)
    {
        _topLevelProvider = topLevelProvider;
    }

    /// <inheritdoc/>
    public async Task SetClipboardTextAsync(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        var topLevel = _topLevelProvider.Get();
        if (topLevel?.Clipboard == null)
            return;

        await topLevel.Clipboard.SetTextAsync(text);
    }

    /// <inheritdoc/>
    public async Task SetClipboardImageAsync(Bitmap? bitmap)
    {
        if (bitmap == null)
            return;

        var topLevel = _topLevelProvider.Get();
        if (topLevel?.Clipboard == null)
            return;

        await topLevel.Clipboard.SetBitmapAsync(bitmap);
    }

    /// <inheritdoc/>
    public async Task<string?> OpenFilePickerAsync(string title, string filterName, string[] patterns)
    {
        var topLevel = _topLevelProvider.Get();
        if (topLevel == null)
            return null;

        var storageProvider = topLevel.StorageProvider;

        var options = new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType> { new(filterName) { Patterns = patterns } }
        };

        var files = await storageProvider.OpenFilePickerAsync(options);
        return !files.Any() ? null : files[0].TryGetLocalPath();
    }

    /// <inheritdoc/>
    public async Task<List<string>?> OpenFilesPickerAsync(string title, string filterName, string[] patterns)
    {
        var topLevel = _topLevelProvider.Get();
        if (topLevel == null)
            return null;

        var storageProvider = topLevel.StorageProvider;

        var options = new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = true,
            FileTypeFilter = new List<FilePickerFileType> { new(filterName) { Patterns = patterns } }
        };

        var files = await storageProvider.OpenFilePickerAsync(options);
        if (!files.Any())
            return null;
        List<string> result = [];
        foreach (var file in files)
        {
            string? path = file.TryGetLocalPath();
            if (string.IsNullOrEmpty(path))
                continue;
            result.Add(path);
        }
        return result;
    }

    /// <inheritdoc/>
    public async Task<string?> OpenFolderPickerAsync(string title)
    {
        var topLevel = _topLevelProvider.Get();
        if (topLevel == null)
            return null;

        var storageProvider = topLevel.StorageProvider;

        if (!storageProvider.CanPickFolder)
            throw new NotSupportedException("The platform does not support folder picking.");

        var options = new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        };

        var folders = await storageProvider.OpenFolderPickerAsync(options);

        if (!folders.Any())
            return null;

        var selectedFolder = folders.FirstOrDefault();
        return selectedFolder?.TryGetLocalPath();
    }
}
