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
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models.InstanceConfig;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views;

public partial class EditInstanceWindow : Window
{
    private readonly InstanceModel _instance;
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(EditInstanceWindow));

    // This constructor is used by the Avalonia Designer.
    public EditInstanceWindow()
    {
        InitializeComponent();

        // This check is a safeguard, but the parameterless constructor
        // is only called in design mode anyway.
        if (Design.IsDesignMode)
        {
            // Provide a mock data context for the designer to render.
            _instance = new InstanceModel();
            this.DataContext = new EditInstanceViewModel(this, _instance, new InstanceConfig());
        }
    }

    public EditInstanceWindow(InstanceModel instance)
    {
        InitializeComponent();

#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif

        if (Design.IsDesignMode)
            return;

        _instance = instance;
        this.DataContext = new EditInstanceViewModel(this, instance, instance.ConfigModel);
        var settings = LauncherHelper.GetLauncherSettings();
        HandleLanguageChange(settings.Launcher.Language);
        App.OnLanguageChanged += HandleLanguageChange;
    }

    /// <summary>
    /// Updates the UI elements with translations based on the specified language.
    /// This method handles the translation of sidebar buttons, page titles, and various settings labels.
    /// </summary>
    /// <param name="language">The language code to apply for translations.</param>
    private void HandleLanguageChange(string language)
    {

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
    
    #region DataGrid Loading Events

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
    
    /// <summary>
    /// Handles the loading event for the Resource Packs DataGrid rows.
    /// Configures a context menu for each row with options to enable/disable, remove, download, or open the folder of the resource pack.
    /// </summary>
    /// <param name="sender">The source of the event, typically the DataGrid.</param>
    /// <param name="e">The event data containing information about the row being loaded.</param>
    private void ResourcePacksDataGrid_OnLoading(object? sender, DataGridRowEventArgs e)
    {
        // Get the DataGridRow
        var row = e.Row;

        if (row.DataContext is not ResourcePackModel resourcePackItem)
            return;

        var contextMenu = new ContextMenu();

        // Add Enable/Disable MenuItem
        string enableDisableHeader = resourcePackItem.IsEnabled ? TranslationManager.Translate("common.disable") : TranslationManager.Translate("common.enable");
        var editMenuItem = new MenuItem { Header = enableDisableHeader };
        editMenuItem.Click += (_, _) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            resourcePackItem.IsEnabled = !resourcePackItem.IsEnabled;
            editMenuItem.Header = resourcePackItem.IsEnabled
                ? TranslationManager.Translate("common.disable")
                : TranslationManager.Translate("common.enable");
            viewModel.SaveResourcePacks();
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

            if (!File.Exists(resourcePackItem.Path))
                return;
            
            File.Delete(resourcePackItem.Path);
            viewModel.RefreshResourcePacks();
        };
        contextMenu.Items.Add(removeMenuItem);

        // Separator
        contextMenu.Items.Add(new Separator());

        // Add Download Packs MenuItem
        var downloadModsMenuItem = new MenuItem { Header = "Download Packs" };
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
            if (!File.Exists(resourcePackItem.Path))
                return;
            
            string? resourcePackDir = Path.GetDirectoryName(resourcePackItem.Path);
            if (string.IsNullOrEmpty(resourcePackDir) || !Directory.Exists(resourcePackDir))
                return;
            
            FileSystemHelper.OpenFolderInFileExplorer(resourcePackDir);
        };
        contextMenu.Items.Add(openFolderMenuItem);

        // Assign the ContextMenu to the row
        row.ContextMenu = contextMenu;
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
    
    /// <summary>
    /// Handles the click event for opening the Java path selector.
    /// Displays a dialog to select a Java version and updates the Java path in the InstanceConfig if a version is selected.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data associated with the click event.</param>
    private void WorldDataGrid_OnLoading(object? sender, DataGridRowEventArgs e)
    {
        // Get the DataGridRow
        var row = e.Row;

        if (row.DataContext is not WorldModel worldItem)
            return;

        var contextMenu = new ContextMenu();

        // Add Duplicate MenuItem
        var duplicateItem = new MenuItem { Header = "Duplicate" };
        duplicateItem.Click += (_, _) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;
            
            viewModel.DuplicateWorld(worldItem);
            viewModel.RefreshWorlds();
        };
        contextMenu.Items.Add(duplicateItem);

        // Add Rename MenuItem
        var renameMenuItem = new MenuItem { Header = "Rename" };
        renameMenuItem.Click += (_, _) =>
        {
            WorldsTable.BeginEdit();
        };
        contextMenu.Items.Add(renameMenuItem);

        // Add Delete MenuItem
        var deleteMenuItem = new MenuItem { Header = "Delete" };
        deleteMenuItem.Click += (_, _) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            FileSystemHelper.DeleteDirectory(worldItem.Path);
            viewModel.RefreshWorlds();
        };
        contextMenu.Items.Add(deleteMenuItem);

        // Separator
        contextMenu.Items.Add(new Separator());

        // Add Copy Seed MenuItem
        var copySeedMenuItem = new MenuItem { Header = "Copy Seed" };
        copySeedMenuItem.Click += (_, _) =>
        {
            var topLevel = GetTopLevel(this);
            if (topLevel?.Clipboard == null)
                return;

            topLevel.Clipboard.SetTextAsync(worldItem.Seed.ToString());
        };
        contextMenu.Items.Add(copySeedMenuItem);

        // Add Open Folder MenuItem
        var openFolderMenuItem = new MenuItem { Header = "Open Folder" };
        openFolderMenuItem.Click += (_, _) =>
        {
            if (!Directory.Exists(worldItem.Path))
                return;

            FileSystemHelper.OpenFolderInFileExplorer(worldItem.Path);
        };
        contextMenu.Items.Add(openFolderMenuItem);

        // Assign the ContextMenu to the row
        row.ContextMenu = contextMenu;
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

    /// <summary>
    /// Handles the loading event for the Server DataGrid rows.
    /// Configures a context menu for each row with options to join a server or remove it from the list.
    /// </summary>
    /// <param name="sender">The source of the event, typically the DataGrid.</param>
    /// <param name="e">The event data containing information about the row being loaded.</param>
    private void ServerDataGrid_OnLoading(object? sender, DataGridRowEventArgs e)
    {
        // Get the DataGridRow
        var row = e.Row;

        if (row.DataContext is not ServerModel serverItem)
            return;

        var contextMenu = new ContextMenu();

        // Add Join MenuItem
        var joinMenuItem = new MenuItem { Header = "Join" };
        joinMenuItem.Click += async (_, _) =>
        {
            if (this is Window { Owner: MainWindow parentWindow })
                await _instance.LaunchAsync(parentWindow, serverItem.Ip);
            this.Close();
        };
        contextMenu.Items.Add(joinMenuItem);

        // Add Remove MenuItem
        var removeItem = new MenuItem { Header = "Remove" };
        removeItem.Click += (_, _) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            viewModel.Servers.Remove(serverItem);
        };
        contextMenu.Items.Add(removeItem);

        // Assign the ContextMenu to the row
        row.ContextMenu = contextMenu;
    }
    #endregion
    
    #region Screenshots

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

    /// <summary>
    /// Handles the loading event for the Screenshot DataGrid rows.
    /// Configures a context menu for each row with options to copy, delete, rename, 
    /// or open the folder containing the screenshot.
    /// </summary>
    /// <param name="sender">The source of the event, typically the DataGrid.</param>
    /// <param name="e">The event data containing information about the row being loaded.</param>
    private void ScreenshotDataGrid_OnLoading(object? sender, DataGridRowEventArgs e)
    {
        // Get the DataGridRow
        var row = e.Row;
        if (row.DataContext is not ScreenshotModel screenshotItem)
            return;

        var contextMenu = new ContextMenu();

        // Add Copy MenuItem
        var copyMenuItem = new MenuItem { Header = "Copy" };
        copyMenuItem.Click += (_, _) =>
        {
            if (screenshotItem?.Image == null)
                return;

            var topLevel = GetTopLevel(this);
            if (topLevel?.Clipboard == null)
                return;

            using var ms = new MemoryStream();
            screenshotItem.Image.Save(ms);
            var dataObject = new DataObject();
            dataObject.Set("image/png", ms.ToArray());

            topLevel.Clipboard.SetDataObjectAsync(dataObject);
        };
        contextMenu.Items.Add(copyMenuItem);

        // Add Delete MenuItem
        var deleteItem = new MenuItem { Header = "Delete" };
        deleteItem.Click += (_, _) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            if (!File.Exists(screenshotItem.Path))
                return;

            File.Delete(screenshotItem.Path);
            viewModel.RefreshScreenshots();
        };
        contextMenu.Items.Add(deleteItem);

        // Add Rename MenuItem
        var renameItem = new MenuItem { Header = "Rename" };
        renameItem.Click += (_, _) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            if (viewModel.SelectedScreenshot == null || _instance.GameDirectory == null)
                return;

            ScreenshotsTable.BeginEdit();
        };
        contextMenu.Items.Add(renameItem);

        // Add Open Folder MenuItem
        var openFolderItem = new MenuItem { Header = "Open Folder" };
        openFolderItem.Click += (_, _) =>
        {
            if (this.DataContext is not EditInstanceViewModel viewModel)
                return;

            if (viewModel.SelectedScreenshot == null || _instance.GameDirectory == null)
                return;

            string screenshotDir = Path.Combine(_instance.GameDirectory, "screenshots");
            if (!Directory.Exists(screenshotDir))
                return;

            FileSystemHelper.OpenFolderInFileExplorer(screenshotDir);
        };
        contextMenu.Items.Add(openFolderItem);

        // Assign the ContextMenu to the row
        row.ContextMenu = contextMenu;
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