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

/// <summary>
///     Handles the screenshots tab of the instance editor.
/// </summary>
public partial class EditInstanceViewModel_Screenshots : KonkordObservableObject
{
    private readonly IBitmapService _bitmapService = null!;
    private readonly IFilePickerService _filePickerService = null!;
    private readonly Func<string?> _getGameDirectory = () => null;

    /// <summary>
    ///     Creates an empty view-model.
    /// </summary>
    public EditInstanceViewModel_Screenshots() { }

    /// <summary>
    ///     Creates the view-model for one instance.
    /// </summary>
    /// <param name="getGameDirectory">Returns the game directory of the instance.</param>
    /// <param name="bitmapService">The service used to load and release images.</param>
    /// <param name="filePickerService">The service used to work with files and the clipboard.</param>
    public EditInstanceViewModel_Screenshots(Func<string?> getGameDirectory, IBitmapService bitmapService,
        IFilePickerService filePickerService) : this()
    {
        _getGameDirectory = getGameDirectory;
        _bitmapService = bitmapService;
        _filePickerService = filePickerService;
    }

    /// <summary>
    ///     Gets or sets the screenshots found in the instance folder.
    /// </summary>
    public ObservableCollection<ScreenshotModel> Screenshots { get; set; } = [];

    /// <summary>
    ///     Gets or sets the selected screenshot.
    /// </summary>
    [ObservableProperty] public partial ScreenshotModel? SelectedScreenshot { get; set; }

    /// <summary>
    ///     Releases the loaded images and clears the list.
    /// </summary>
    /// <param name="disposing">True when the object is disposed by code.</param>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        foreach (var screenshot in Screenshots)
            _bitmapService.Release(screenshot.Path);
        Screenshots.Clear();
        SelectedScreenshot?.Image?.Dispose();
        SelectedScreenshot = null;
    }

    /// <summary>
    ///     Loads the screenshots of the instance.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that completes when the screenshots are loaded.</returns>
    public Task InitAsync(CancellationToken cancellationToken = default)
    {
        RefreshScreenshots();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Reads the screenshots folder again and rebuilds the list.
    /// </summary>
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
