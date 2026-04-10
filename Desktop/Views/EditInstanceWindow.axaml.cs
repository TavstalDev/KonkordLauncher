using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models.InstanceConfig;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views;

/// <summary>
/// Represents the window for editing an instance in the Konkord Launcher.
/// </summary>
public partial class EditInstanceWindow : KonkordWindow<EditInstanceViewModel>
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(EditInstanceWindow));
    
    /// <summary>
    /// Initializes a new instance of the <see cref="EditInstanceWindow"/> class.
    /// Sets up the data context with an empty instance ID and initializes components.
    /// </summary>
    public EditInstanceWindow()
    {
        InitializeComponent();
        DataContext = new EditInstanceViewModel(string.Empty);
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="EditInstanceWindow"/> class with a specific instance ID.
    /// Sets up the data context, initializes components, and registers reactive handlers.
    /// </summary>
    /// <param name="instanceId">The ID of the instance to be edited.</param>
    public EditInstanceWindow(string instanceId)
    {
        InitializeComponent();
        
#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif
        
        DataContext = new EditInstanceViewModel(instanceId);
        this.WhenActivated(disposables =>
        {
            DataContext.CloseWindow.RegisterHandler(action =>
            {
                this.Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.ShowAlertDialog.RegisterHandler(async action =>
            {
                AlertWindow alertWindow = new(action.Input.Title, action.Input.Message, action.Input.Type);
                await alertWindow.ShowDialog(this);
                action.SetOutput(Unit.Default);
            }).DisposeWith(disposables);
            DataContext.ShowJavaPathSelector.RegisterHandler(async action =>
            {
                var window = new JavaSelectorWindow();
                var javaVersion = await window.ShowDialog<JavaVersionModel>(this);
                action.SetOutput(javaVersion);
            });
            DataContext.SetClipboardImage.RegisterHandler(async action =>
            {
                await SetClipboardImageAsync(action.Input);
                action.SetOutput(Unit.Default);
            }).DisposeWith(disposables);
            DataContext.SetClipboardText.RegisterHandler(async action =>
            {
                await SetClipboardTextAsync(action.Input);
                action.SetOutput(Unit.Default);
            }).DisposeWith(disposables);
            DataContext.BeginScreenshotRename.RegisterHandler(action =>
            {
                ScreenshotsTable.BeginEdit();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.BeginWorldRename.RegisterHandler(action =>
            {
                WorldsTable.BeginEdit();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.LogsScrollToEnd.RegisterHandler(action =>
            {
                LogsScrollViewer.Offset =  new Vector(0, LogsScrollViewer.Extent.Height);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            });
        });
    }
    
    /// <summary>
    /// Called when the window is opened.
    /// Updates the Rich Presence status to indicate that an instance is being edited.
    /// </summary>
    /// <param name="e">The event arguments for the opened event.</param>
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        App.UpdateRPC("Editing instance...");
    }

    /// <summary>
    /// Called when the window is closed.
    /// Updates the Rich Presence status to indicate that the user is browsing instances.
    /// </summary>
    /// <param name="e">The event arguments for the closed event.</param>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        App.UpdateRPC("Browsing instances...");
    }


    #region Action Handlers
    
    /// <summary>
    /// Handles the selection change event for the overridden account ComboBox.
    /// Updates the account ID in the instance configuration based on the selected account.
    /// </summary>
    /// <param name="sender">The source of the event, expected to be a ComboBox.</param>
    /// <param name="e">The event data containing information about the selection change.</param>
    private void OverridenAccount_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext == null)
            return;

        if (sender is ComboBox { SelectedItem: Account selectedAccount })
            DataContext.InstanceConfig.Misc.AccountId = selectedAccount.Id;
    }
    #endregion
    
    #region DataGrid Events
    
    #region Resource Packs
    
    /// <summary>
    /// Handles the event when a cell edit operation in the Resource Packs DataGrid is completed.
    /// Updates the resource pack data and triggers saving the updated list of resource packs.
    /// </summary>
    /// <param name="sender">The source of the event, typically the DataGrid.</param>
    /// <param name="e">The event data containing information about the edited cell.</param>
    private void ResourcePacksDataGrid_OnCellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        if (DataContext == null)
            return;
    
        var row = e.Row;
        if (row.DataContext is not ResourcePackModel)
            return;
    
        _logger.Debug("ResourcePack row updated. Saving...");
        DataContext.SaveResourcePacks();
    }

    #endregion
    
    #region Worlds
    
    /// <summary>
    /// Handles the event when a cell edit operation in the World DataGrid is completed.
    /// Updates the world data and triggers saving the updated list of worlds.
    /// </summary>
    /// <param name="sender">The source of the event, typically the DataGrid.</param>
    /// <param name="e">The event data containing information about the edited cell.</param>
    private void WorldDataGrid_OnCellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        if (DataContext == null)
            return;
        
        var row = e.Row;
        if (row.DataContext is not WorldModel)
            return;
        
        _logger.Debug("World row updated. Saving...");
        DataContext.SaveWorlds();
    }
    
    /// <summary>
    /// Handles the click event for selecting the Java path.
    /// Opens a folder picker dialog and updates the Java path in the InstanceConfig if a folder is selected.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data associated with the click event.</param>
    private void WorldDataGrid_OnRowEditEnded(object? sender, DataGridRowEditEndedEventArgs e)
    {
        if (DataContext == null)
            return;
        
        var row = e.Row;
        if (row.DataContext is not WorldModel)
            return;
        
        _logger.Debug("World row updated. Saving...");
        DataContext.SaveWorlds();
    }

    #endregion
    
    #region Servers
    
    /// <summary>
    /// Handles the event when a row edit operation in the Servers DataGrid is completed.
    /// Updates the server data and triggers saving the updated list of servers.
    /// </summary>
    /// <param name="sender">The source of the event, typically the DataGrid.</param>
    /// <param name="e">The event data containing information about the edited row.</param>
    private void ServersDataGrid_OnRowEditEnded(object? sender, DataGridRowEditEndedEventArgs e)
    {
        if (DataContext == null)
            return;
        
        var row = e.Row;
        if (row.DataContext is not ServerModel)
            return;
        
        _logger.Debug("Server row updated. Saving servers...");
        DataContext.SaveServers();
    }
    
    #endregion
    
    #region Screenshots
    /// <summary>
    /// Copies the provided screenshot image to the system clipboard as a PNG.
    /// </summary>
    /// <param name="screenshot">
    /// The screenshot model containing the image to copy to the clipboard.
    /// If the image is null, the operation is aborted.
    /// </param>
    public async Task SetClipboardImageAsync(ScreenshotModel screenshot)
    {
        if (screenshot.Image == null)
            return;

        var topLevel = GetTopLevel(this);
        if (topLevel?.Clipboard == null)
            return;

        using var ms = new MemoryStream();
        screenshot.Image.Save(ms);
        var dataObject = new DataObject();
        dataObject.Set("image/png", ms.ToArray());

        await topLevel.Clipboard.SetDataObjectAsync(dataObject);
    }
    
    /// <summary>
    /// Handles the event when editing of a screenshot cell is completed in the DataGrid.
    /// Renames the screenshot file if the name has been changed and updates the model accordingly.
    /// Logs warnings or errors if the operation fails.
    /// </summary>
    /// <param name="sender">The source of the event, typically the DataGrid.</param>
    /// <param name="e">The event data containing information about the edited cell.</param>
    private void ScreenshotCell_OnEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        if (e.Row.DataContext is not ScreenshotModel screenshot)
            return;
        
        string? dirPath = Path.GetDirectoryName(screenshot.Path);
        if (string.IsNullOrEmpty(dirPath))
            return;
        
        string oldPath = screenshot.Path;
        string oldName = Path.GetFileNameWithoutExtension(oldPath);
        
        // Ensure the new name is not empty
        if (string.IsNullOrEmpty(screenshot.Name))
            return;
        
        // Skip renaming if the name has not changed
        if (screenshot.Name == oldName)
            return;
        
        // Construct the new path
        string newPath = Path.Combine(dirPath, screenshot.Name + screenshot.Extension);
    
        // Check if a file with the new name already exists
        if (File.Exists(newPath))
        {
            _logger.Warn($"Failed to rename screenshot. A file with the name '{screenshot.Name}' already exists.");
            screenshot.Name = oldName;
            return;
        }
    
        // Perform the file rename operation
        try
        {
            File.Move(oldPath, newPath);
            screenshot.Path = newPath;
            _logger.Debug($"Screenshot renamed from '{oldName}' to '{screenshot.Name}'.");
        }
        catch (Exception ex)
        {
            _logger.Exc($"An error occurred while renaming the screenshot:");
            _logger.Error(ex);
        }
    }

    #endregion

    #endregion

    #region Java Path Selection

    /// <summary>
    /// Handles the click event for selecting the Java path.
    /// Opens a folder picker dialog and updates the Java path in the InstanceConfig if a folder is selected.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data associated with the click event.</param>
    private void JavaPathSelect_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext == null)
            return;

        var directoryResult = OpenFolderPickerAsync();
        directoryResult.ContinueWith(task =>
        {
            if (!task.IsCompletedSuccessfully)
                return;

            if (task.Result is not { } resultPath)
                return;

            DataContext.InstanceConfig.Java.DefaultJavaPath = resultPath;
        });
    }

    /// <summary>
    /// Opens a folder picker dialog to allow the user to select a folder.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the path of the selected folder
    /// as a string, or null if no folder was selected or if folder picking is not supported.
    /// </returns>
    private async Task<string?> OpenFolderPickerAsync()
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

        // Configure folder picker options
        var options = new FolderPickerOpenOptions
        {
            Title = TranslationManager.Translate("common.select.directory"),
            AllowMultiple = false
        };

        // Open the folder picker dialog
        IReadOnlyList<IStorageFolder> folders = await storageProvider.OpenFolderPickerAsync(options);

        // Return null if no folders were selected
        if (!folders.Any())
            return null;

        // Get the first selected folder
        IStorageFolder? selectedFolder = folders.FirstOrDefault();
        if (selectedFolder == null)
        {
            _logger.Error("No folder was selected.");
            return null;
        }

        // Return the local path of the selected folder
        return selectedFolder.Path.LocalPath;
    }

    #endregion

    #region Environment Table Events

    /// <summary>
    /// Handles the event when a row edit operation in the Environment DataGrid is completed.
    /// Updates the corresponding environment variable in the instance configuration.
    /// </summary>
    /// <param name="sender">The source of the event, typically the DataGrid.</param>
    /// <param name="e">The event data containing information about the edited row.</param>
    private void EnvironmentDataGrid_OnRowEditEnded(object? sender, DataGridRowEditEndedEventArgs e)
    {
        if (DataContext == null)
            return;

        if (e.Row.DataContext is not EnvironmentVariable environmentItem)
            return;

        DataContext.InstanceConfig.Environment[e.Row.Index] = environmentItem;
    }

    /// <summary>
    /// Handles the click event for adding a new environment variable row to the Environment DataGrid.
    /// Adds a default environment variable to the instance configuration.
    /// </summary>
    /// <param name="sender">The source of the event, typically a button.</param>
    /// <param name="e">The event data associated with the click event.</param>
    private void AddEnvironmentRow_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext == null)
            return;

        DataContext.InstanceConfig.Environment.Add(new("ENV_VAR", "env_value"));
    }

    /// <summary>
    /// Handles the click event for removing the selected environment variable row from the Environment DataGrid.
    /// Removes the environment variable at the selected index from the instance configuration.
    /// </summary>
    /// <param name="sender">The source of the event, typically a button.</param>
    /// <param name="e">The event data associated with the click event.</param>
    private void RemoveEnvironmentRow_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext == null)
            return;

        if (DataContext.SelectedEnvironmentVariableIndex is null or < 0)
            return;

        var index = DataContext.SelectedEnvironmentVariableIndex.Value;
        if (index >= DataContext.InstanceConfig.Environment.Count)
            return;

        DataContext.InstanceConfig.Environment.RemoveAt(index);
    }

    /// <summary>
    /// Handles the click event for clearing all rows in the Environment DataGrid.
    /// Removes all environment variables from the instance configuration.
    /// </summary>
    /// <param name="sender">The source of the event, typically a button.</param>
    /// <param name="e">The event data associated with the click event.</param>
    private void ClearEnvironmentTable_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext == null)
            return;

        DataContext.InstanceConfig.Environment.Clear();
    }

    #endregion
}