using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using DiscordRPC;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;
using Button = Avalonia.Controls.Button;
using MainViewModel = Tavstal.KonkordLauncher.Desktop.Views.Models.MainViewModel;

namespace Tavstal.KonkordLauncher.Desktop.Views;

// ReSharper disable once PartialTypeWithSinglePart
public partial class MainWindow : KonkordWindow<MainViewModel>
{
    // This window should not use KonkordWindow as long as it can only be opened once.
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(MainWindow));
    private DiscordRpcClient rpcClient;
    private Button _selectedButton;
    
    public MainWindow()
    {
        InitializeComponent();
        
#if DEBUG
        this.AttachDevTools(); // Attaches Avalonia Dev Tools for debugging
#endif
        
        // Instantiate your ViewModel and assign it to the DataContext
        _selectedButton = PlaySideBtn;
        DataContext = new MainViewModel();
        this.WhenActivated(disposables =>
        {
            DataContext.CloseWindow.RegisterHandler(action =>
            {
                Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.UpdateSidebarButton.RegisterHandler(action =>
            {
                HandleSidebarChange(action.Input);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.OpenFolderPicker.RegisterHandler(async action =>
            {
                var result = await OpenFolderPickerAsync();
                action.SetOutput(result);
            }).DisposeWith(disposables);
            DataContext.ShowAlertDialog.RegisterHandler(async action =>
            {
                AlertWindow alertWindow = new(action.Input.Title, action.Input.Message, action.Input.Type);
                await alertWindow.ShowDialog(this);
                action.SetOutput(Unit.Default);
            }).DisposeWith(disposables);
            DataContext.ShowConfirmDialog.RegisterHandler(async action =>
            {
                AlertWindow alertWindow = new(action.Input.Title, action.Input.Message, action.Input.Type);
                var result = await alertWindow.ShowDialog<bool>(this);
                action.SetOutput(result);
            }).DisposeWith(disposables);
            DataContext.ShowInstanceCreationDialog.RegisterHandler(async action =>
            {
                await new CreateInstanceWindow().ShowDialog(this);
                action.SetOutput(Unit.Default);
            }).DisposeWith(disposables);
            DataContext.ShowInstanceEditDialog.RegisterHandler(async action =>
            {
                EditInstanceWindow editInstanceWindow = new EditInstanceWindow(action.Input);
                var result = await editInstanceWindow.ShowDialog<bool>(this);
                action.SetOutput(Unit.Default);
                if (!result)
                    return;
                GlobalEvents.InvokeInstancesChanged();
            }).DisposeWith(disposables);
            DataContext.ShowAccountsDialog.RegisterHandler(async action =>
            {
                var dialog = new AccountsWindow();
                await dialog.ShowDialog(this);
                action.SetOutput(Unit.Default);
            }).DisposeWith(disposables);
            DataContext.ShowJavaSelectorDialog.RegisterHandler(async action =>
            {
                var window = new JavaSelectorWindow();
                var javaVersion = await window.ShowDialog<JavaVersionModel>(this);
                action.SetOutput(javaVersion);
            }).DisposeWith(disposables);
            DataContext.ShowLogsWindow.RegisterHandler(action =>
            {
                var window = new InstanceLogsWindow(action.Input);
                window.Show();
                action.SetOutput(Unit.Default);
            });
            DataContext.ShowTextInputDialog.RegisterHandler(async action =>
            {
                var dialog = new InputWindow(action.Input);
                var result = await dialog.ShowDialog<string?>(this);
                action.SetOutput(result);
            });
            DataContext.ShowIconSelectorDialog.RegisterHandler(async action =>
            {
                var dialog = new IconSelectorWindow();
                var result = await dialog.ShowDialog<string?>(this);
                action.SetOutput(result);
            });
        });

        if (Design.IsDesignMode)
            return;
        
        var screen = Screens.Primary;
        if (screen == null)
            throw new InvalidOperationException("No primary screen found."); // Ensure there is a primary screen
        var screenSize = screen.Bounds.Size;
        App.SetScreenSize(screenSize);

        // TODO: Implement an actual way to show if there is update available
        VersionLabel.Content = TranslationManager.Translate("main.sidebar.version.update.none");
    }

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
        _selectedButton.Classes.Remove("PrimaryBtn");
        _selectedButton.Classes.Add("SecondaryBtn");

        switch (sidebarType)
        {
            case ESidebarType.Play:
            {
                
                _selectedButton = PlaySideBtn;
                break;
            }
            case ESidebarType.Patch:
            {
                _selectedButton = NewsSideBtn;
                break;
            }
            case ESidebarType.Accounts:
            {
                _selectedButton = AccountsSideBtn;
                break;
            }
            case ESidebarType.Settings:
            {
                _selectedButton = SettingsSideBtn;
                break;
            }
            case ESidebarType.About:
            {
                _selectedButton = AboutSideBtn;
                break;
            }
        }
        _selectedButton.Classes.Remove("SecondaryBtn");
        _selectedButton.Classes.Add("PrimaryBtn");
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
    
    #region Event Handlers

    /// <summary>
    /// Handles the event when the window is opened. Initializes the Discord RPC client
    /// and sets the initial presence for the application.
    /// </summary>
    /// <param name="e">The event data associated with the window opening.</param>
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        rpcClient = new DiscordRpcClient("1178002101561995416");
        rpcClient.Initialize();

        // Set the initial presence
        rpcClient.SetPresence(new RichPresence
        {
            Details = "Browsing instances...",
            Timestamps = Timestamps.Now,
            Assets = new Assets
            {
                LargeImageKey = "logo",
                LargeImageText = "Konkord Launcher",
            }
        });
    }

    /// <summary>
    /// Handles the event when the window is closing. Clears and disposes of the Discord RPC client
    /// to ensure proper cleanup of resources.
    /// </summary>
    /// <param name="e">The event data associated with the window closing.</param>
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        rpcClient.SetPresence(null);
        rpcClient.Dispose();
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
        
        viewModel.CoreConfig.Launcher.Language = selectedLanguage.TwoLetterCode;
    }
    #endregion
}