using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.EditInstance;

public partial class EditInstanceViewModel_Screenshots  : KonkordObservableObject
{
    private readonly EditInstanceViewModel _parent;
    
    public ObservableCollection<ScreenshotModel> Screenshots { get; set; } = [];
    [ObservableProperty]
    public partial ScreenshotModel? SelectedScreenshot { get; set; }

    public EditInstanceViewModel_Screenshots(EditInstanceViewModel parent)
    {
        _parent = parent;
    }
    
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        foreach (var screenshot in Screenshots)
            screenshot.Image?.Dispose();
        Screenshots.Clear();
        SelectedScreenshot?.Image?.Dispose();
        SelectedScreenshot = null;
    }
    
    public async Task InitAsync(CancellationToken cancellationToken = default)
    {
        RefreshScreenshots();
    }
    
    #region Commands

    /// <summary>
    /// Copies the specified screenshot to the system clipboard.
    /// </summary>
    /// <param name="screenshot">The screenshot to copy to the clipboard.</param>
    [RelayCommand]
    private async Task CopyToClipboard(ScreenshotModel screenshot) => await _parent.SetClipboardImage.Handle(screenshot);

    /// <summary>
    /// Deletes the specified screenshot file from the file system and refreshes the screenshot list.
    /// </summary>
    /// <param name="screenshot">The screenshot to delete.</param>
    [RelayCommand]
    private void Delete(ScreenshotModel screenshot)
    {
        if (!File.Exists(screenshot.Path))
            return;

        File.Delete(screenshot.Path);
        RefreshScreenshots();
    }

    /// <summary>
    /// Initiates the renaming process for the specified screenshot by enabling edit mode in the screenshots table.
    /// </summary>
    /// <param name="screenshot">The screenshot to rename.</param>
    [RelayCommand]
    private async Task Rename(ScreenshotModel screenshot) => await _parent.BeginScreenshotRename.Handle(Unit.Default);

    /// <summary>
    /// Opens the directory containing the screenshots in the file explorer.
    /// </summary>
    /// <param name="screenshot">The screenshot whose directory to open.</param>
    [RelayCommand]
    private void OpenDir(ScreenshotModel screenshot)
    {
        if (string.IsNullOrEmpty(_parent.GameDirectory))
            return;
    
        string screenshotDir = Path.Combine(_parent.GameDirectory, "screenshots");
        if (!Directory.Exists(screenshotDir))
            return;

        FileSystemHelper.OpenFolderInFileExplorer(screenshotDir);
    }

    #endregion
    
    /// <summary>
    /// Refreshes the list of screenshots by scanning the game directory for PNG files
    /// and updating the Screenshots collection with their metadata and image data.
    /// </summary>
    public void RefreshScreenshots()
    {
        if (_parent.GameDirectory == null)
            return;

        string screenshotDir = Path.Combine(_parent.GameDirectory, "screenshots");
        if (!Directory.Exists(screenshotDir))
            return;
        
        foreach (var screenshot in Screenshots)
        {
            // Dispose of the image to free memory
            screenshot.Image?.Dispose();
        }
        Screenshots.Clear();
        var screenshots = Directory.GetFiles(screenshotDir, "*.png");
        foreach (var screenshot in screenshots)
        {
            var bytes = File.ReadAllBytes(screenshot);
            var newScreenshot = new ScreenshotModel(screenshot, new Bitmap(screenshot), bytes.LongLength);
            Screenshots.Add(newScreenshot);
        }
    }
}