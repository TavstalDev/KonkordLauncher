using System;
using System.IO;
using Avalonia.Controls;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Pages.EditInstance;

public partial class ScreenshotsPage : UserControl
{
    private EditInstanceViewModel? _subscribedVm;

    public ScreenshotsPage()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (_subscribedVm != null)
        {
            _subscribedVm.OnScreenshotRenameRequested -= OnScreenshotRenameRequested;
            _subscribedVm = null;
        }

        if (DataContext is EditInstanceViewModel newVm)
        {
            _subscribedVm = newVm;
            _subscribedVm.OnScreenshotRenameRequested += OnScreenshotRenameRequested;
        }
    }

    private void OnScreenshotRenameRequested(object? sender, EventArgs e)
    {
        ScreenshotsTable.BeginEdit();
    }

    private void ScreenshotCell_OnEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        if (e.Row.DataContext is not ScreenshotModel screenshot)
            return;

        var dirPath = Path.GetDirectoryName(screenshot.Path);
        if (string.IsNullOrEmpty(dirPath))
            return;

        var oldPath = screenshot.Path;
        var oldName = Path.GetFileNameWithoutExtension(oldPath);

        // Ensure the new name is not empty
        if (string.IsNullOrEmpty(screenshot.Name))
            return;

        // Skip renaming if the name has not changed
        if (screenshot.Name == oldName)
            return;

        // Construct the new path
        var newPath = Path.Combine(dirPath, screenshot.Name + screenshot.Extension);

        // Check if a file with the new name already exists
        if (File.Exists(newPath))
        {
            screenshot.Name = oldName;
            return;
        }

        // Perform the file rename operation
        try
        {
            File.Move(oldPath, newPath);
            screenshot.Path = newPath;
        }
        catch (Exception)
        {
            if (File.Exists(newPath))
            {
                File.Move(newPath, oldPath);
                screenshot.Path = oldPath;
            }
        }
    }
}

