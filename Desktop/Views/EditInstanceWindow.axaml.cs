using System;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models.InstanceConfig;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Accounts;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;
using Tavstal.KonkordLauncher.Desktop.Views.Models;
using JavaVersionModel = Tavstal.KonkordLauncher.Desktop.Models.Domain.JavaVersionModel;

namespace Tavstal.KonkordLauncher.Desktop.Views;

/// <summary>
/// Represents the window for editing an instance in the Konkord Launcher.
/// </summary>
public partial class EditInstanceWindow : KonkordWindow<EditInstanceViewModel>
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(EditInstanceWindow));
    private Button _selectedInstanceTab;
    private Button _selectedSettingsTab;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="EditInstanceWindow"/> class.
    /// Sets up the data context with an empty instance ID and initializes components.
    /// </summary>
    public EditInstanceWindow() : this(string.Empty) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="EditInstanceWindow"/> class with a specific instance ID.
    /// Sets up the data context, initializes components, and registers reactive handlers.
    /// </summary>
    /// <param name="instanceId">The ID of the instance to be edited.</param>
    public EditInstanceWindow(string instanceId)
    {
        InitializeComponent();
        
        DataContext = new EditInstanceViewModel(instanceId);
        _selectedInstanceTab = LogsTabBtn;
        _selectedSettingsTab = JavaSettingsBtn;
        
        this.WhenActivated(disposables =>
        {
            DataContext.MinimizeWindowInteraction.RegisterHandler(action =>
            {
                WindowState = WindowState.Minimized;
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.MaximizeWindowInteraction.RegisterHandler(action =>
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.TabSwitchInteraction.RegisterHandler(action =>
            {
                HandleTabSwitch(action.Input);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.SettingsTabSwitchInteraction.RegisterHandler(action =>
            {
                HandleSettingsTabSwitch(action.Input);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.ShowAlertDialog.RegisterHandler(async action =>
            {
                AlertWindow alertWindow = new(action.Input.Title, action.Input.Message, action.Input.Type);
                await alertWindow.ShowDialog(this);
                action.SetOutput(Unit.Default);
            }).DisposeWith(disposables);
            DataContext.ShowDirPickerInteraction.RegisterHandler(async action =>
            {
                var result = await OpenFolderPickerAsync(action.Input);
                action.SetOutput(result);
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
        GlobalEvents.InvokeInstancesChanged();
    }

    /// <summary>
    /// Switches the active main tab in the Edit Instance window and updates the visual state of the tab buttons.
    /// </summary>
    /// <param name="tab">The target tab to switch to. Must be one of the values from the <see cref="EEditInstanceTab"/> enum.</param>
    private void HandleTabSwitch(EEditInstanceTab tab)
    {
        if (DataContext is not { } viewModel)
            return;
        
        if (viewModel.EditInstanceTab == tab)
            return;

        viewModel.EditInstanceTab = tab;
        _selectedInstanceTab.Classes.Remove("SettingsTabBtnActive");

        switch (tab)
        {
            case EEditInstanceTab.LOGS:
            {
                _selectedInstanceTab = LogsTabBtn;
                break;
            }
            case EEditInstanceTab.MODS:
            {
                _selectedInstanceTab = ModsTabBtn;
                break;
            }
            case EEditInstanceTab.RESOURCE_PACKS:
            {
                _selectedInstanceTab = ResourcePacksTabBtn;
                break;
            }
            case EEditInstanceTab.SHADER_PACKS:
            {
                _selectedInstanceTab = ShaderPacksTabBtn;
                break;
            }
            case EEditInstanceTab.WORLDS:
            {
                _selectedInstanceTab = WorldsTabBtn;
                break;
            }
            case EEditInstanceTab.SERVERS:
            {
                _selectedInstanceTab = ServersTabBtn;
                break;
            }
            case EEditInstanceTab.SCREENSHOTS:
            {
                _selectedInstanceTab = ScreenshotsTabBtn;
                break;
            }
            case EEditInstanceTab.SETTINGS:
            {
                _selectedInstanceTab = SettingsTabBtn;
                break;
            }
        }

        _selectedInstanceTab.Classes.Add("SettingsTabBtnActive");
    }

    /// <summary>
    /// Switches the active settings sub-tab in the Instance Settings section and updates the visual state
    /// of the settings tab buttons.
    /// </summary>
    /// <param name="tab">The target settings tab to switch to. Must be one of the values from the <see cref="EInstanceSettingsTab"/> enum.</param>
    private void HandleSettingsTabSwitch(EInstanceSettingsTab tab)
    {
        if (DataContext is not { } viewModel)
            return;
        
        if (viewModel.InstanceSettingsTab == tab)
            return;

        viewModel.InstanceSettingsTab = tab;
        _selectedSettingsTab.Classes.Remove("SettingsTabBtnActive");

        switch (tab)
        {
            case EInstanceSettingsTab.JAVA:
            {
                _selectedSettingsTab = JavaSettingsBtn;
                break;
            }
            case EInstanceSettingsTab.GAME:
            {
                _selectedSettingsTab = GameSettingsBtn;
                break;
            }
            case EInstanceSettingsTab.CUSTOM_COMMAND:
            {
                _selectedSettingsTab = CustomCommandSettingsBtn;
                break;
            }
            case EInstanceSettingsTab.ENVIRONMENT:
            {
                _selectedSettingsTab = EnvironmentSettingsBtn;
                break;
            }
            case EInstanceSettingsTab.MISC:
            {
                _selectedSettingsTab = MiscSettingsBtn;
                break;
            }
        }

        _selectedSettingsTab.Classes.Add("SettingsTabBtnActive");
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
            DataContext.Settings.InstanceConfig.Misc.AccountId = selectedAccount.Id;
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
        DataContext.ResourcePacks.SaveResourcePacks();
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
        DataContext.Worlds.SaveWorlds();
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
        DataContext.Worlds.SaveWorlds();
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
        DataContext.Servers.SaveServers();
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

    #endregion

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

        DataContext.Settings.InstanceConfig.Environment[e.Row.Index] = environmentItem;
    }

    /// <summary>
    /// Handles the click event for adding a new environment variable row to the Environment DataGrid.
    /// Adds a default environment variable to the instance configuration.
    /// </summary>
    /// <param name="sender">The source of the event, typically a button.</param>
    /// <param name="e">The event data associated with the click event.</param>
    private void AddEnvironmentRow_OnClick(object? sender, RoutedEventArgs e) => DataContext?.Settings.InstanceConfig.Environment.Add(new("ENV_VAR", "env_value"));

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

        if (DataContext.Settings.SelectedEnvironmentVariableIndex is null or < 0)
            return;

        var index = DataContext.Settings.SelectedEnvironmentVariableIndex.Value;
        if (index >= DataContext.Settings.InstanceConfig.Environment.Count)
            return;

        DataContext.Settings.InstanceConfig.Environment.RemoveAt(index);
    }

    /// <summary>
    /// Handles the click event for clearing all rows in the Environment DataGrid.
    /// Removes all environment variables from the instance configuration.
    /// </summary>
    /// <param name="sender">The source of the event, typically a button.</param>
    /// <param name="e">The event data associated with the click event.</param>
    private void ClearEnvironmentTable_OnClick(object? sender, RoutedEventArgs e) => DataContext?.Settings.InstanceConfig.Environment.Clear();

    #endregion
}