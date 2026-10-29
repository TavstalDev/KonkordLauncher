using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tavstal.BeaconSDK.Helpers.IO;
using Tavstal.KonkordLauncher.Desktop.Constants;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.EditInstance;

public partial class EditInstanceViewModel_Screenshots : KonkordObservableObject
{
    private readonly IBitmapService _bitmapService = null!;
    private readonly IFilePickerService _filePickerService = null!;
    private readonly Func<string?> _getGameDirectory = () => null;

    public EditInstanceViewModel_Screenshots() { }

    public EditInstanceViewModel_Screenshots(Func<string?> getGameDirectory, IBitmapService bitmapService,
        IFilePickerService filePickerService) : this()
    {
        _getGameDirectory = getGameDirectory;
        _bitmapService = bitmapService;
        _filePickerService = filePickerService;
    }

    public ObservableCollection<ScreenshotModel> Screenshots { get; set; } = [];

    [ObservableProperty] public partial ScreenshotModel? SelectedScreenshot { get; set; }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        foreach (var screenshot in Screenshots)
            screenshot.Image?.Dispose();
        Screenshots.Clear();
        SelectedScreenshot?.Image?.Dispose();
        SelectedScreenshot = null;
    }

    public Task InitAsync(CancellationToken cancellationToken = default)
    {
        RefreshScreenshots();
        return Task.CompletedTask;
    }

    public void RefreshScreenshots()
    {
        var gameDir = _getGameDirectory.Invoke();
        if (string.IsNullOrEmpty(gameDir))
            return;

        var screenshotDir = Path.Combine(gameDir, "screenshots");
        if (!Directory.Exists(screenshotDir))
            return;

        foreach (var screenshot in Screenshots)
            _bitmapService.Release(screenshot.Path);
        Screenshots.Clear();
        var screenshots = Directory.GetFiles(screenshotDir, "*.png");
        foreach (var screenshot in screenshots)
        {
            var bytes = File.ReadAllBytes(screenshot);
            var screen = _bitmapService.GetBitmap(screenshot, BitmapSizes.ScreenshotIcon);
            var newScreenshot = new ScreenshotModel(screenshot, screen.Value, bytes.LongLength);
            Screenshots.Add(newScreenshot);
        }
    }

    #region Commands

    [RelayCommand]
    private async Task CopyToClipboard(ScreenshotModel screenshot)
    {
        if (!File.Exists(screenshot.Path))
            return;

        // The cached image is only a small preview, so load the original for the clipboard.
        using var bitmap = new Bitmap(screenshot.Path);
        await _filePickerService.SetClipboardImageAsync(bitmap);
    }

    [RelayCommand]
    private void Delete(ScreenshotModel screenshot)
    {
        if (!File.Exists(screenshot.Path))
            return;

        File.Delete(screenshot.Path);
        RefreshScreenshots();
    }

    [RelayCommand]
    private void OpenDir(ScreenshotModel screenshot)
    {
        var gameDir = _getGameDirectory.Invoke();
        if (string.IsNullOrEmpty(gameDir))
            return;

        var screenshotDir = Path.Combine(gameDir, "screenshots");
        if (!Directory.Exists(screenshotDir))
            return;

        FileSystemHelper.OpenFolderInFileExplorer(screenshotDir);
    }

    #endregion
}
