using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactiveUI;
using ReactiveUI.Primitives;
using Tavstal.BeaconSDK.Models.Accounts;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs.InstanceConfig;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Services;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views;

public partial class EditInstanceWindow : KonkordWindow<EditInstanceViewModel>
{
    private readonly IDiscordPresenceService _discordPresenceService = null!;
    private readonly IEventService _eventService = null!;
    private readonly string _instanceId = null!;
    private readonly IBeaconLogger _logger = null!;
    private Button _selectedInstanceTab;
    private Button _selectedSettingsTab;

    public EditInstanceWindow()
    {
        InitializeComponent();
        _selectedInstanceTab = LogsTabBtn;
        _selectedSettingsTab = JavaSettingsBtn;
    }

    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public EditInstanceWindow(EditInstanceViewModel viewModel, IBeaconLogger<EditInstanceWindow> logger,
        IEventService eventService, IDiscordPresenceService discordPresenceService,
        IBitmapService bitmapService) : base(bitmapService)
    {
        InitializeComponent();
        _selectedInstanceTab = LogsTabBtn;
        _selectedSettingsTab = JavaSettingsBtn;

        if (Design.IsDesignMode)
            return;

        _logger = logger;
        _eventService = eventService;
        _discordPresenceService = discordPresenceService;
        _instanceId = viewModel.Instance.Id;
        DataContext = viewModel;

        this.WhenActivated(disposables =>
        {
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
                LogsScrollViewer.Offset = new Vector(0, LogsScrollViewer.Extent.Height);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
        });
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        _discordPresenceService.Update("Editing instance...");
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _discordPresenceService.Update("Browsing instances...");
        _eventService.RaiseInstanceUpdated(_instanceId);
    }

    private void HandleTabSwitch(EEditInstanceTab tab)
    {
        if (DataContext is not { } viewModel)
            return;

        if (viewModel.EditInstanceTab == tab)
            return;

        viewModel.EditInstanceTab = tab;
        _selectedInstanceTab.Classes.Remove("SettingsTabBtnActive");

        _selectedInstanceTab = tab switch
        {
            EEditInstanceTab.LOGS => LogsTabBtn,
            EEditInstanceTab.MODS => ModsTabBtn,
            EEditInstanceTab.RESOURCE_PACKS => ResourcePacksTabBtn,
            EEditInstanceTab.SHADER_PACKS => ShaderPacksTabBtn,
            EEditInstanceTab.WORLDS => WorldsTabBtn,
            EEditInstanceTab.SERVERS => ServersTabBtn,
            EEditInstanceTab.SCREENSHOTS => ScreenshotsTabBtn,
            EEditInstanceTab.SETTINGS => SettingsTabBtn,
            _ => _selectedInstanceTab
        };

        _selectedInstanceTab.Classes.Add("SettingsTabBtnActive");
    }

    private void HandleSettingsTabSwitch(EInstanceSettingsTab tab)
    {
        if (DataContext is not { } viewModel)
            return;

        if (viewModel.InstanceSettingsTab == tab)
            return;

        viewModel.InstanceSettingsTab = tab;
        _selectedSettingsTab.Classes.Remove("SettingsTabBtnActive");

        _selectedSettingsTab = tab switch
        {
            EInstanceSettingsTab.JAVA => JavaSettingsBtn,
            EInstanceSettingsTab.GAME => GameSettingsBtn,
            EInstanceSettingsTab.CUSTOM_COMMAND => CustomCommandSettingsBtn,
            EInstanceSettingsTab.ENVIRONMENT => EnvironmentSettingsBtn,
            EInstanceSettingsTab.MISC => MiscSettingsBtn,
            _ => _selectedSettingsTab
        };

        _selectedSettingsTab.Classes.Add("SettingsTabBtnActive");
    }

    #region Action Handlers

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

    private void ResourcePacksDataGrid_OnCellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        if (DataContext == null)
            return;

        var row = e.Row;
        if (row.DataContext is not ResourceBaseModel)
            return;

        _logger.LogDebug("ResourcePack row updated. Saving...");
        DataContext.ResourcePacks.SaveResourcePacks();
    }

    #endregion

    #region Worlds

    private void WorldDataGrid_OnCellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        if (DataContext == null)
            return;

        var row = e.Row;
        if (row.DataContext is not WorldModel)
            return;

        _logger.LogDebug("World row updated. Saving...");
        DataContext.Worlds.SaveWorlds();
    }

    private void WorldDataGrid_OnRowEditEnded(object? sender, DataGridRowEditEndedEventArgs e)
    {
        if (DataContext == null)
            return;

        var row = e.Row;
        if (row.DataContext is not WorldModel)
            return;

        _logger.LogDebug("World row updated. Saving...");
        DataContext.Worlds.SaveWorlds();
    }

    #endregion

    #region Servers

    private void ServersDataGrid_OnRowEditEnded(object? sender, DataGridRowEditEndedEventArgs e)
    {
        if (DataContext == null)
            return;

        var row = e.Row;
        if (row.DataContext is not ServerModel)
            return;

        _logger.LogDebug("Server row updated. Saving servers...");
        DataContext.Servers.SaveServers();
    }

    #endregion

    #region Screenshots

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
            _logger.LogWarning(
                $"Failed to rename screenshot. A file with the name '{screenshot.Name}' already exists.");
            screenshot.Name = oldName;
            return;
        }

        // Perform the file rename operation
        try
        {
            File.Move(oldPath, newPath);
            screenshot.Path = newPath;
            _logger.LogDebug($"Screenshot renamed from '{oldName}' to '{screenshot.Name}'.");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "An error occurred while renaming the screenshot:");
        }
    }

    #endregion

    #endregion

    #region Environment Table Events

    private void EnvironmentDataGrid_OnRowEditEnded(object? sender, DataGridRowEditEndedEventArgs e)
    {
        if (DataContext == null)
            return;

        if (e.Row.DataContext is not EnvironmentVariable environmentItem)
            return;

        DataContext.Settings.InstanceConfig.Environment[e.Row.Index] = environmentItem;
    }

    private void AddEnvironmentRow_OnClick(object? sender, RoutedEventArgs e)
    {
        DataContext?.Settings.InstanceConfig.Environment.Add(new EnvironmentVariable("ENV_VAR", "env_value"));
    }

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

    private void ClearEnvironmentTable_OnClick(object? sender, RoutedEventArgs e)
    {
        DataContext?.Settings.InstanceConfig.Environment.Clear();
    }

    #endregion
}
