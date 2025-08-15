using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Tavstal.KonkordLauncher.Common.Models.InstanceConfig;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views;

/// <summary>
/// Represents the EditInstanceWindow, a partial class inheriting from Avalonia's Window class.
/// Provides functionality for editing an instance configuration.
/// </summary>
public partial class EditInstanceWindow : KonkordWindow
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(EditInstanceWindow));
    
    public EditInstanceWindow()
    {
        InitializeComponent();
        this.DataContext = new EditInstanceViewModel(this, string.Empty);
    }
    
    public EditInstanceWindow(string instanceId)
    {
        InitializeComponent();
        
#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif
        
        this.DataContext = new EditInstanceViewModel(this, instanceId);
    }

    protected override void FreeMemory()
    {
        _logger.Debug("EditInstanceWindow memory cleared.");
    }

    #region Action Handlers
    
    #region Resource Pack Buttons
    private void DownloadResourcePacks_OnClick(object? sender, RoutedEventArgs e)
    {
        
    }

    /// <summary>
    /// Handles the click event for viewing the Resource Packs folder.
    /// Opens the folder containing the resource packs in the file explorer if it exists.
    /// </summary>
    /// <param name="sender">The source of the event, typically a button.</param>
    /// <param name="e">The event data associated with the click event.</param>
    private void ViewResourcePacksFolder_OnClick(object? sender, RoutedEventArgs e)
    {
        if (this.DataContext is not EditInstanceViewModel viewModel)
            return;

        if (viewModel.GameDirectory == null)
            return;
        
        string? resourcePacksDir = Path.Combine(viewModel.GameDirectory, "resourcepacks");
        if (!Directory.Exists(resourcePacksDir))
            return;

        FileSystemHelper.OpenFolderInFileExplorer(resourcePacksDir);
    }
    #endregion
    
    /// <summary>
    /// Handles the selection change event for the overridden account ComboBox.
    /// Updates the account ID in the instance configuration based on the selected account.
    /// </summary>
    /// <param name="sender">The source of the event, expected to be a ComboBox.</param>
    /// <param name="e">The event data containing information about the selection change.</param>
    private void OverridenAccount_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (this.DataContext is not EditInstanceViewModel viewModel)
            return;

        if (sender is ComboBox { SelectedItem: Account selectedAccount })
            viewModel.InstanceConfig.Misc.AccountId = selectedAccount.Id;
    }
    #endregion
    
    #region DataGrid Events

    #region Mods
    
    private void ModsDataGrid_OnLoading(object? sender, DataGridRowEventArgs e)
    {
        // Get the DataGridRow
        var row = e.Row;

        if (row.DataContext is not ModModel modItem)
            return;

        var contextMenu = new ContextMenu();

        // Add Enable/Disable MenuItem
        string enableDisableHeader = modItem.IsEnabled ? "Disable" : "Enable";
        var editMenuItem = new MenuItem { Header = enableDisableHeader };
        editMenuItem.Click += (_, _) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(editMenuItem);

        // Separator
        contextMenu.Items.Add(new Separator());

        // Add Check For Update MenuItem
        var checkUpdateMenuItem = new MenuItem { Header = "Check for Update" };
        checkUpdateMenuItem.Click += (_, _) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(checkUpdateMenuItem);

        // Add Change Version MenuItem
        var changeVersionMenuItem = new MenuItem { Header = "Change Version" };
        changeVersionMenuItem.Click += (_, _) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(changeVersionMenuItem);

        // Separator
        contextMenu.Items.Add(new Separator());

        // Add Remove MenuItem
        var removeMenuItem = new MenuItem { Header = "Remove" };
        removeMenuItem.Click += (_, _) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(removeMenuItem);

        // Separator
        contextMenu.Items.Add(new Separator());

        // Add Download Mods MenuItem
        var downloadModsMenuItem = new MenuItem { Header = "Download Mods" };
        downloadModsMenuItem.Click += (_, _) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(downloadModsMenuItem);

        // Add Open Folder MenuItem
        var openFolderMenuItem = new MenuItem { Header = "Open Folder" };
        openFolderMenuItem.Click += (_, _) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(openFolderMenuItem);

        // Assign the ContextMenu to the row
        row.ContextMenu = contextMenu;
    }

    #endregion
    
    #region Resource Packs
    
    /// <summary>
    /// Handles the event when a cell edit operation in the Resource Packs DataGrid is completed.
    /// Updates the resource pack data and triggers saving the updated list of resource packs.
    /// </summary>
    /// <param name="sender">The source of the event, typically the DataGrid.</param>
    /// <param name="e">The event data containing information about the edited cell.</param>
    private void ResourcePacksDataGrid_OnCellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        if (this.DataContext is not EditInstanceViewModel viewModel)
            return;
    
        var row = e.Row;
        if (row.DataContext is not ResourcePackModel)
            return;
    
        _logger.Debug("ResourcePack row updated. Saving...");
        viewModel.SaveResourcePacks();
    }

    #endregion

    #region Shaders
    
    private void ShaderDataGrid_OnLoading(object? sender, DataGridRowEventArgs e)
    {
        // Get the DataGridRow
        var row = e.Row;

        if (row.DataContext is not ShaderPackModel shaderPackItem)
            return;

        var contextMenu = new ContextMenu();

        // Add Enable/Disable MenuItem
        string enableDisableHeader = shaderPackItem.IsEnabled ? "Disable" : "Enable";
        var editMenuItem = new MenuItem { Header = enableDisableHeader };
        editMenuItem.Click += (_, _) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(editMenuItem);

        // Separator
        contextMenu.Items.Add(new Separator());

        // Add Remove MenuItem
        var removeMenuItem = new MenuItem { Header = "Remove" };
        removeMenuItem.Click += (_, _) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(removeMenuItem);

        // Separator
        contextMenu.Items.Add(new Separator());

        // Add Download Packs MenuItem
        var downloadModsMenuItem = new MenuItem { Header = "Download Shaders" };
        downloadModsMenuItem.Click += (_, _) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(downloadModsMenuItem);

        // Add Open Folder MenuItem
        var openFolderMenuItem = new MenuItem { Header = "Open Folder" };
        openFolderMenuItem.Click += (_, _) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            // TODO: Handle click event
        };
        contextMenu.Items.Add(openFolderMenuItem);

        // Assign the ContextMenu to the row
        row.ContextMenu = contextMenu;
    }

    #endregion
    
    #region Worlds

    /// <summary>
    /// Copies the provided seed value to the system clipboard as a string.
    /// </summary>
    /// <param name="seed">The seed value to copy to the clipboard.</param>
    public void CopySeedToClipboard(long seed)
    {
        var topLevel = GetTopLevel(this);
        if (topLevel?.Clipboard == null)
            return;

        topLevel.Clipboard.SetTextAsync(seed.ToString());
    }
    
    /// <summary>
    /// Handles the event when a cell edit operation in the World DataGrid is completed.
    /// Updates the world data and triggers saving the updated list of worlds.
    /// </summary>
    /// <param name="sender">The source of the event, typically the DataGrid.</param>
    /// <param name="e">The event data containing information about the edited cell.</param>
    private void WorldDataGrid_OnCellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        if (this.DataContext is not EditInstanceViewModel viewModel)
            return;
        
        var row = e.Row;
        if (row.DataContext is not WorldModel)
            return;
        
        _logger.Debug("World row updated. Saving...");
        viewModel.SaveWorlds();
    }
    
    /// <summary>
    /// Handles the click event for selecting the Java path.
    /// Opens a folder picker dialog and updates the Java path in the InstanceConfig if a folder is selected.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data associated with the click event.</param>
    private void WorldDataGrid_OnRowEditEnded(object? sender, DataGridRowEditEndedEventArgs e)
    {
        if (this.DataContext is not EditInstanceViewModel viewModel)
            return;
        
        var row = e.Row;
        if (row.DataContext is not WorldModel)
            return;
        
        _logger.Debug("World row updated. Saving...");
        viewModel.SaveWorlds();
    }

    #endregion
    
    #region Servers

    /// <summary>
    /// Handles the click event for adding a new server to the server list.
    /// Validates the input fields for server name and address before adding a new server
    /// to the `Servers` collection in the view model.
    /// </summary>
    /// <param name="sender">The source of the event, typically a button.</param>
    /// <param name="e">The event data associated with the click event.</param>
    private void AddServer_OnClick(object? sender, RoutedEventArgs e)
    {
        if (this.DataContext is not EditInstanceViewModel viewModel)
            return;
        
        if (string.IsNullOrEmpty(ServerNameInput.Text) || string.IsNullOrEmpty(ServerAddressInput.Text))
            return;
        
        viewModel.Servers.Add(new ServerModel(ServerNameInput.Text, ServerAddressInput.Text, 0, 0, null));
    }
    
    /// <summary>
    /// Handles the event when a row edit operation in the Servers DataGrid is completed.
    /// Updates the server data and triggers saving the updated list of servers.
    /// </summary>
    /// <param name="sender">The source of the event, typically the DataGrid.</param>
    /// <param name="e">The event data containing information about the edited row.</param>
    private void ServersDataGrid_OnRowEditEnded(object? sender, DataGridRowEditEndedEventArgs e)
    {
        if (this.DataContext is not EditInstanceViewModel viewModel)
            return;
        
        var row = e.Row;
        if (row.DataContext is not ServerModel)
            return;
        
        _logger.Debug("Server row updated. Saving servers...");
        viewModel.SaveServers();
    }
    
    #endregion
    
    #region Screenshots

    public void SetClipboardImage(ScreenshotModel screenshot)
    {
        if (screenshot?.Image == null)
            return;

        var topLevel = GetTopLevel(this);
        if (topLevel?.Clipboard == null)
            return;

        using var ms = new MemoryStream();
        screenshot.Image.Save(ms);
        var dataObject = new DataObject();
        dataObject.Set("image/png", ms.ToArray());

        topLevel.Clipboard.SetDataObjectAsync(dataObject);
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
        if (DataContext is not EditInstanceViewModel viewModel)
            return;

        var directoryResult = OpenFolderPickerAsync();
        directoryResult.ContinueWith(task =>
        {
            if (!task.IsCompletedSuccessfully)
                return;

            if (task.Result is not { } resultPath)
                return;

            viewModel.InstanceConfig.Java.DefaultJavaPath = resultPath;
        });
    }

    /// <summary>
    /// Handles the click event for opening the Java path selector.
    /// Displays a dialog to select a Java version and updates the Java path in the InstanceConfig if a version is selected.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data associated with the click event.</param>
    private async void JavaOpenPathSelector_OnClick(object? sender, RoutedEventArgs e)
    {
        // TODO: Replace async void with async Task
        var window = new JavaSelectorWindow();
        var javaVersion = await window.ShowDialog<JavaVersionModel>(this);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (javaVersion == null)
            return;

        if (DataContext is not EditInstanceViewModel viewModel)
            return;

        viewModel.InstanceConfig.Java.DefaultJavaPath = javaVersion.Path;
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
        if (this.DataContext is not EditInstanceViewModel viewModel)
            return;

        if (e.Row.DataContext is not EnvironmentVariable environmentItem)
            return;

        viewModel.InstanceConfig.Environment[e.Row.Index] = environmentItem;
    }

    /// <summary>
    /// Handles the click event for adding a new environment variable row to the Environment DataGrid.
    /// Adds a default environment variable to the instance configuration.
    /// </summary>
    /// <param name="sender">The source of the event, typically a button.</param>
    /// <param name="e">The event data associated with the click event.</param>
    private void AddEnvironmentRow_OnClick(object? sender, RoutedEventArgs e)
    {
        if (this.DataContext is not EditInstanceViewModel viewModel)
            return;

        viewModel.InstanceConfig.Environment.Add(new("ENV_VAR", "env_value"));
    }

    /// <summary>
    /// Handles the click event for removing the selected environment variable row from the Environment DataGrid.
    /// Removes the environment variable at the selected index from the instance configuration.
    /// </summary>
    /// <param name="sender">The source of the event, typically a button.</param>
    /// <param name="e">The event data associated with the click event.</param>
    private void RemoveEnvironmentRow_OnClick(object? sender, RoutedEventArgs e)
    {
        if (this.DataContext is not EditInstanceViewModel viewModel)
            return;

        if (viewModel.SelectedEnvironmentVariableIndex is null or < 0)
            return;

        var index = viewModel.SelectedEnvironmentVariableIndex.Value;
        if (index >= viewModel.InstanceConfig.Environment.Count)
            return;

        viewModel.InstanceConfig.Environment.RemoveAt(index);
    }

    /// <summary>
    /// Handles the click event for clearing all rows in the Environment DataGrid.
    /// Removes all environment variables from the instance configuration.
    /// </summary>
    /// <param name="sender">The source of the event, typically a button.</param>
    /// <param name="e">The event data associated with the click event.</param>
    private void ClearEnvironmentTable_OnClick(object? sender, RoutedEventArgs e)
    {
        if (this.DataContext is not EditInstanceViewModel viewModel)
            return;

        viewModel.InstanceConfig.Environment.Clear();
    }

    #endregion
}