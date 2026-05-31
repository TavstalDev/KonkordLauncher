using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;
using Button = Avalonia.Controls.Button;
using JavaVersionModel = Tavstal.KonkordLauncher.Desktop.Models.Domain.JavaVersionModel;
using MainViewModel = Tavstal.KonkordLauncher.Desktop.Views.Models.MainViewModel;

namespace Tavstal.KonkordLauncher.Desktop.Views;

// ReSharper disable once PartialTypeWithSinglePart
public partial class MainWindow : KonkordWindow<MainViewModel>
{
    // This window should not use KonkordWindow as long as it can only be opened once.
    private readonly ICustomLogger _logger;
    private readonly Dictionary<string, InstanceLogsWindow> _logWindows = new(); 
    private readonly Dictionary<string, EditInstanceWindow> _openEditWindows = new();
    private Button _selectedSideBarButton;
    private Button _selectedSettingsTabButton;
    private Button _selectedAboutTabButton;
    
    public MainWindow()
    {
        InitializeComponent();
        
        _selectedSideBarButton = PlaySideBtn;
        _selectedSettingsTabButton = LauncherSettingsBtn;
        _selectedAboutTabButton = AboutInfoBtn;
        
        if (Design.IsDesignMode)
            return;
        
        var services = Program.ServiceProvider;
        _logger = services.GetRequiredService<ICustomLogger<MainWindow>>();
        var translationService = services.GetRequiredService<ITranslationService>();
        
        DataContext = new MainViewModel();
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
            DataContext.SwitchSidebarBtnInteraction.RegisterHandler(action =>
            {
                HandleSidebarChange(action.Input);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.OpenFolderPickerInteraction.RegisterHandler(async action =>
            {
                var result = await OpenFolderPickerAsync(translationService.Translate("common.select.directory"));
                action.SetOutput(result);
            }).DisposeWith(disposables);
            DataContext.OpenImagePickerInteraction.RegisterHandler(async action =>
            {
                var result = await OpenFilePickerAsync(translationService.Translate("common.select.file"), "PNGs", ["*.png"]);
                action.SetOutput(result);
            }).DisposeWith(disposables);
            DataContext.ShowAlertDialogInteraction.RegisterHandler(async action =>
            {
                AlertWindow alertWindow = new(action.Input.Title, action.Input.Message, action.Input.Type);
                await alertWindow.ShowDialog(this);
                action.SetOutput(Unit.Default);
            }).DisposeWith(disposables);
            DataContext.ShowConfirmDialogInteraction.RegisterHandler(async action =>
            {
                AlertWindow alertWindow = new(action.Input.Title, action.Input.Message, action.Input.Type);
                var result = await alertWindow.ShowDialog<bool>(this);
                action.SetOutput(result);
            }).DisposeWith(disposables);
            DataContext.ShowInstanceCreationDialogInteraction.RegisterHandler(async action =>
            {
                await new CreateInstanceWindow().ShowDialog(this);
                action.SetOutput(Unit.Default);
            }).DisposeWith(disposables);
            DataContext.ShowInstanceEditDialogInteraction.RegisterHandler(action =>
            {
                try
                {
                    action.SetOutput(Unit.Default);
                    InstanceModel instance = action.Input;
                    if (_openEditWindows.TryGetValue(instance.Id, out var window))
                    {
                        window.Activate();
                        if (window.WindowState == WindowState.Minimized)
                            window.WindowState = WindowState.Normal;
                        return Task.CompletedTask;
                    }
                    EditInstanceWindow editInstanceWindow = new EditInstanceWindow(instance);
                    editInstanceWindow.Show(this);
                    _openEditWindows.Add(instance.Id, editInstanceWindow);
                    editInstanceWindow.Closed += (_, _) => _openEditWindows.Remove(instance.Id);
                    return Task.CompletedTask;
                }
                catch (Exception exception)
                {
                    return Task.FromException(exception);
                }
            }).DisposeWith(disposables);
            DataContext.ShowAccountsDialogInteraction.RegisterHandler(async action =>
            {
                var dialog = new AccountsWindow();
                await dialog.ShowDialog(this);
                action.SetOutput(Unit.Default);
            }).DisposeWith(disposables);
            DataContext.ShowJavaSelectorDialogInteraction.RegisterHandler(async action =>
            {
                var window = new JavaSelectorWindow();
                var javaVersion = await window.ShowDialog<JavaVersionModel>(this);
                action.SetOutput(javaVersion);
            }).DisposeWith(disposables);
            DataContext.ShowLogsWindowInteraction.RegisterHandler(action =>
            {
                string instanceId = action.Input;
                var window = new InstanceLogsWindow(instanceId);
                window.Show();
                _logWindows[instanceId] = window;
                action.SetOutput(Unit.Default);
            }).DisposeWith(disposables);
            DataContext.CloseLogsWindowInteraction.RegisterHandler(action =>
            {
                var window = _logWindows.GetValueOrDefault(action.Input);
                window?.Close();
                if (window != null)
                    _logWindows.Remove(action.Input);
                action.SetOutput(Unit.Default);
            }).DisposeWith(disposables);
            DataContext.ShowTextInputDialogInteraction.RegisterHandler(async action =>
            {
                var dialog = new InputWindow(action.Input);
                var result = await dialog.ShowDialog<string?>(this);
                action.SetOutput(result);
            }).DisposeWith(disposables);
            DataContext.ShowIconSelectorDialogInteraction.RegisterHandler(async action =>
            {
                var dialog = new IconSelectorWindow();
                var result = await dialog.ShowDialog<string?>(this);
                action.SetOutput(result);
            }).DisposeWith(disposables);
            DataContext.UpdateSettingsTabButtonInteraction.RegisterHandler(action =>
            {
                HandleSettingsTabChange(action.Input);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.SwitchAboutTabInteractionInteraction.RegisterHandler(action =>
            {
                HandleAboutTabChange(action.Input);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.ExportModrinthInstanceInteraction.RegisterHandler(async action =>
            {
                var instance = action.Input;
                ExportWindow exportWindow = new ExportWindow(instance, EInstanceProvider.MODRINTH);
                await exportWindow.ShowDialog(this);
                action.SetOutput(Unit.Default);
            }).DisposeWith(disposables);
            DataContext.ExportCurseForgeInstanceInteraction.RegisterHandler(async action =>
            {
                var instance = action.Input;
                ExportWindow exportWindow = new ExportWindow(instance, EInstanceProvider.CURSE_FORGE);
                await exportWindow.ShowDialog(this);
                action.SetOutput(Unit.Default);
            }).DisposeWith(disposables);
        });
        
        var screen = Screens.Primary;
        if (screen == null)
            throw new InvalidOperationException("No primary screen found."); // Ensure there is a primary screen
        var screenSize = screen.Bounds.Size;
        App.SetScreenSize(screenSize);
    }
    
    #region Events

    /// <summary>
    /// Handles the event when the window is opened. Initializes the Discord RPC client
    /// and sets the initial presence for the application.
    /// </summary>
    /// <param name="e">The event data associated with the window opening.</param>
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        App.UpdateRPC("Browsing instances...");
    }

    /// <summary>
    /// Handles the event when the window is closing. Clears and disposes of the Discord RPC client
    /// to ensure proper cleanup of resources.
    /// </summary>
    /// <param name="e">The event data associated with the window closing.</param>
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        App.ClearRPC();
        base.OnClosing(e);
    }
    
    /// <summary>
    /// Handles the selection of a language from a ComboBox and updates the application's language setting.
    /// </summary>
    /// <param name="sender">The ComboBox that triggered the event.</param>
    /// <param name="e">The event data associated with the selection change.</param>
    private void Language_OnSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not { } viewModel)
            return;
        
        if (sender is not ComboBox { SelectedItem: Language selectedLanguage })
            return;
        
        viewModel.Config.CoreConfig.Launcher.Language = selectedLanguage.TwoLetterCode;
    }
    #endregion

    /// <summary>
    /// Handles the logic for changing the active sidebar section in the main window.
    /// Updates the ViewModel's current page index, manages the visual state of sidebar buttons,
    /// and ensures the correct button is highlighted as active.
    /// </summary>
    /// <param name="sidebarType">The sidebar section to switch to.</param>
    private void HandleSidebarChange(ESidebarType sidebarType)
    {
        if (DataContext is not { } viewModel)
            return;
        
        if (viewModel.CurrentPageIndex == sidebarType)
            return;

        viewModel.CurrentPageIndex = sidebarType;
        _selectedSideBarButton.Classes.Remove("SideBarActiveBtn");

        _selectedSideBarButton = sidebarType switch
        {
            ESidebarType.Play => PlaySideBtn,
            ESidebarType.Patch => NewsSideBtn,
            ESidebarType.Accounts => AccountsSideBtn,
            ESidebarType.Settings => SettingsSideBtn,
            ESidebarType.About => AboutSideBtn,
            ESidebarType.Skins => SkinsSideBtn,
            _ => _selectedSideBarButton
        };

        _selectedSideBarButton.Classes.Add("SideBarActiveBtn");
    }
    
    /// <summary>
    /// Updates the active settings tab in the main window and synchronizes the corresponding
    /// tab button's visual state.
    /// </summary>
    /// <param name="tabType">The settings tab to switch to.</param>
    private void HandleSettingsTabChange(ESettingsTab tabType)
    {
        if (DataContext is not { } viewModel)
            return;
        
        if (viewModel.CurrentSettingsTab == tabType)
            return;

        viewModel.CurrentSettingsTab = tabType;
        _selectedSettingsTabButton.Classes.Remove("SettingsTabBtnActive");
        _selectedSettingsTabButton = tabType switch
        {
            ESettingsTab.LAUNCHER => LauncherSettingsBtn,
            ESettingsTab.MINECRAFT => MinecraftSettingsBtn,
            ESettingsTab.JAVA => JavaSettingsBtn,
            ESettingsTab.MISC => MiscSettingsBtn,
            _ => _selectedSettingsTabButton
        };
        _selectedSettingsTabButton.Classes.Add("SettingsTabBtnActive");
    }

    /// <summary>
    /// Updates the active tab in the About section and synchronizes the corresponding
    /// tab button's visual state.
    /// </summary>
    /// <param name="tabType">The about tab to switch to.</param>
    private void HandleAboutTabChange(EAboutTab tabType)
    {
        if (DataContext is not { } viewModel)
            return;
        
        if (viewModel.CurrentAboutTab == tabType)
            return;

        viewModel.CurrentAboutTab = tabType;
        _selectedAboutTabButton.Classes.Remove("SettingsTabBtnActive");
        switch (tabType)
        {
            case EAboutTab.ABOUT:
            {
                _selectedAboutTabButton = AboutInfoBtn;
                break;
            }
            case EAboutTab.CREDITS:
            {
                _selectedAboutTabButton = CreditsInfoBtn;
                break;
            }
            case EAboutTab.LICENSE:
            {
                _selectedAboutTabButton = LicenseInfoBtn;
                break;
            }
        }
        _selectedAboutTabButton.Classes.Add("SettingsTabBtnActive");
    }
}