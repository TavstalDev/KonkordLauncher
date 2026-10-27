using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace Tavstal.KonkordLauncher.Desktop.Services.Storage;

public interface IFilePickerService
{
    Task SetClipboardTextAsync(string text);

    Task SetClipboardImageAsync(Bitmap? bitmap);

    Task<string?> OpenFilePickerAsync(string title, string filterName, string[] patterns);

    Task<List<string>?> OpenFilesPickerAsync(string title, string filterName, string[] patterns);

    Task<string?> OpenFolderPickerAsync(string title);
}
