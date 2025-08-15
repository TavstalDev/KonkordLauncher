using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Services;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;
using JavaVersionModel = Tavstal.KonkordLauncher.Desktop.Models.JavaVersionModel;
using MainViewModel = Tavstal.KonkordLauncher.Desktop.Views.Models.MainViewModel;

namespace Tavstal.KonkordLauncher.Desktop.Views;

// ReSharper disable once PartialTypeWithSinglePart
public partial class MainWindow : Window
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(MainWindow));
    private Button _selectedButton;
    
    public MainWindow()
    {
        InitializeComponent();
        
#if DEBUG
        this.AttachDevTools(); // Attaches Avalonia Dev Tools for debugging
#endif
        
        // Instantiate your ViewModel and assign it to the DataContext
        this.DataContext = new MainViewModel(this);
        _selectedButton = PlaySideBtn;
        
        if (Design.IsDesignMode)
            return;
        
        var screen = Screens.Primary;
        if (screen == null)
            throw new InvalidOperationException("No primary screen found."); // Ensure there is a primary screen
        var screenSize = screen.Bounds.Size;
        App.SetScreenSize(screenSize);

        var settings = LauncherHelper.GetLauncherSettings();
        HandleLanguageChange(settings.Launcher.Language);
        App.OnLanguageChanged += HandleLanguageChange;
    }

    #region Methods

    /// <summary>
    /// Handles the logic for changing the active sidebar section in the main window.
    /// Updates the ViewModel's current page index, manages the visual state of sidebar buttons,
    /// and ensures the correct button is highlighted as active.
    /// </summary>
    /// <param name="sidebarType">The sidebar section to switch to.</param>
    private void HandleSidebarChange(ESidebarType sidebarType)
    {
        if (DataContext is not MainViewModel viewModel)
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
    #endregion

    #region Event Handlers

    #region App
    /// <summary>
    /// Updates the UI elements with translations based on the specified language.
    /// This method handles the translation of sidebar buttons, page titles, and various settings labels.
    /// </summary>
    /// <param name="language">The language code to apply for translations.</param>
    private void HandleLanguageChange(string language)
    {
        // TODO: Remove
        VersionLabel.Content = TranslationManager.Translate("main.sidebar.version.update.none");
    }

    #endregion

    #region Sidebar Button Click Handlers
    /// <summary>
/// Handles the click event for the "Play" sidebar button.
/// Switches the sidebar to the "Play" section.
/// </summary>
/// <param name="sender">The source of the event, typically a button.</param>
/// <param name="e">The event data associated with the button click.</param>
public void OnPlaySideButtonClick(object? sender, RoutedEventArgs e)
{
    HandleSidebarChange(ESidebarType.Play);
}

/// <summary>
/// Handles the click event for the "News" sidebar button.
/// Switches the sidebar to the "News" section.
/// </summary>
/// <param name="sender">The source of the event, typically a button.</param>
/// <param name="e">The event data associated with the button click.</param>
public void OnNewsSideButtonClick(object? sender, RoutedEventArgs e)
{
    HandleSidebarChange(ESidebarType.Patch);
}

/// <summary>
/// Handles the click event for the "Accounts" sidebar button.
/// Switches the sidebar to the "Accounts" section.
/// </summary>
/// <param name="sender">The source of the event, typically a button.</param>
/// <param name="e">The event data associated with the button click.</param>
public void OnAccountsSideButtonClick(object? sender, RoutedEventArgs e)
{
    HandleSidebarChange(ESidebarType.Accounts);
}

/// <summary>
/// Handles the click event for the "Settings" sidebar button.
/// Switches the sidebar to the "Settings" section.
/// </summary>
/// <param name="sender">The source of the event, typically a button.</param>
/// <param name="e">The event data associated with the button click.</param>
public void OnSettingsSideButtonClick(object? sender, RoutedEventArgs e)
{
    HandleSidebarChange(ESidebarType.Settings);
}

/// <summary>
/// Handles the click event for the "About" sidebar button.
/// Switches the sidebar to the "About" section.
/// </summary>
/// <param name="sender">The source of the event, typically a button.</param>
/// <param name="e">The event data associated with the button click.</param>
private void OnAboutSideButtonClick(object? sender, RoutedEventArgs e)
{
    HandleSidebarChange(ESidebarType.About);
}
    #endregion

    #region Instance Handlers

    /// <summary>
    /// Handles the click event for adding a new instance. 
    /// Opens the CreateInstanceWindow dialog to allow the user to create a new instance.
    /// </summary>
    /// <param name="sender">The source of the event, typically a button.</param>
    /// <param name="e">The event data associated with the button click.</param>
    private void AddInstance_OnClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new CreateInstanceWindow();
        dialog.ShowDialog(this);
    }
    
    #endregion
    
    #region Account Button Click Handlers
    /// <summary>
    /// Handles the click event for adding a new account. Opens the AccountsWindow dialog.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void AddAccount_OnClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new AccountsWindow();
        dialog.ShowDialog(this);
    }

    /// <summary>
    /// Handles the click event for selecting an account. Updates the selected account ID in the ViewModel.
    /// </summary>
    /// <param name="sender">The source of the event, expected to be a Button with an Account as its DataContext.</param>
    /// <param name="e">The event data.</param>
    private void SelectAccount_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
            return;

        if (sender is not Button { DataContext: Account account })
            return;
        
        if (viewModel.AccountData.SelectedAccountId == account.Id)
            return;
        
        viewModel.AccountData.SelectedAccountId = account.Id;
        App.InvokeAccountsChanged();
    }

    /// <summary>
    /// Handles the click event for refreshing an account. Currently not implemented.
    /// </summary>
    /// <param name="sender">The source of the event, expected to be a Button with an Account as its DataContext.</param>
    /// <param name="e">The event data.</param>
    private void RefreshAccount_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
            return;

        if (sender is not Button { DataContext: Account account })
            return;

        if (!account.CanExpire || string.IsNullOrEmpty(account.RefreshToken))
            return;

        if (MicrosoftAuthService.AuthStatus != EAuthStatus.NONE)
            return;
        
        if (account.AccessTokenExpireDate > DateTime.Now)
            return;
            
        Task.Run(async () =>
        {
            if (!await MicrosoftAuthService.RefreshLoginAsync(account.RefreshToken))
            {
                _logger.Error($"Failed to refresh account {account.DisplayName} ({account.Id}).");
                return;
            }

            if (MicrosoftAuthService.Account == null)
            {
                _logger.Error($"Failed to refresh account {account.DisplayName} ({account.Id}) after successful api call.");
                return;
            }
            
            var updatedAccount = MicrosoftAuthService.Account;
            updatedAccount.Id = account.Id; // Ensure the ID remains the same
            _logger.Info($"Successfully refreshed account {account.DisplayName} ({account.Id}).");
            
            AccountData accountData = await LauncherHelper.GetAccountDataAsync();
            var index = accountData.Accounts.FindIndex(x => x.Id == account.Id);
            accountData.Accounts[index] = updatedAccount;
            
            await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherAccountsPath, accountData);
            App.InvokeAccountsChanged();
            MicrosoftAuthService.Reset();
        });
    }

    /// <summary>
    /// Handles the click event for removing an account. Removes the account from the ViewModel and updates the selected account ID if necessary.
    /// </summary>
    /// <param name="sender">The source of the event, expected to be a Button with an Account as its DataContext.</param>
    /// <param name="e">The event data.</param>
    private void RemoveAccount_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
            return;

        if (sender is not Button { DataContext: Account account })
            return;

        viewModel.AccountData.Accounts.Remove(account);
        if (account.Id != viewModel.AccountData.SelectedAccountId) 
            return;
        
        viewModel.AccountData.SelectedAccountId = viewModel.AccountData.HasAccounts ? viewModel.AccountData.Accounts.FirstOrDefault()?.Id : null;
    }
    
    #endregion

    #region Settings Handlers
    /// <summary>
    /// Handles the selection of a language from a ComboBox and updates the application's language setting.
    /// </summary>
    /// <param name="sender">The ComboBox that triggered the event.</param>
    /// <param name="e">The event data associated with the selection change.</param>
    private void Language_OnSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
            return;
        
        if (sender is not ComboBox { SelectedItem: Language selectedLanguage })
            return;
        
        viewModel.CoreConfig.Launcher.Language = selectedLanguage.TwoLetterCode;
    }

    /// <summary>
    /// Opens a folder picker dialog to select the assets directory and updates the configuration with the selected path.
    /// </summary>
    /// <param name="sender">The button that triggered the event.</param>
    /// <param name="e">The event data associated with the button click.</param>
    private void AssetsDirSelect_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
            return;
        
        var directoryResult = OpenFolderPickerAsync();
        directoryResult.ContinueWith(task =>
        {
            if (!task.IsCompletedSuccessfully)
                return;

            if (task.Result is not { } resultPath)
                return;
            
            viewModel.CoreConfig.Launcher.AssetsDirectoryPath = resultPath;
        });
    }

    /// <summary>
    /// Opens a folder picker dialog to select the cache directory and updates the configuration with the selected path.
    /// </summary>
    /// <param name="sender">The button that triggered the event.</param>
    /// <param name="e">The event data associated with the button click.</param>
    private void CacheDirSelect_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
            return;
        
        var directoryResult = OpenFolderPickerAsync();
        directoryResult.ContinueWith(task =>
        {
            if (!task.IsCompletedSuccessfully)
                return;

            if (task.Result is not { } resultPath)
                return;
            
            viewModel.CoreConfig.Launcher.CacheDirectoryPath = resultPath;
        });
    }

    /// <summary>
    /// Opens a folder picker dialog to select the instances directory and updates the configuration with the selected path.
    /// </summary>
    /// <param name="sender">The button that triggered the event.</param>
    /// <param name="e">The event data associated with the button click.</param>
    private void InstancesDirSelect_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
            return;
        
        var directoryResult = OpenFolderPickerAsync();
        directoryResult.ContinueWith(task =>
        {
            if (!task.IsCompletedSuccessfully)
                return;

            if (task.Result is not { } resultPath)
                return;
            
            viewModel.CoreConfig.Launcher.InstancesDirectoryPath = resultPath;
        });
    }

    /// <summary>
    /// Opens a folder picker dialog to select the icons directory and updates the configuration with the selected path.
    /// </summary>
    /// <param name="sender">The button that triggered the event.</param>
    /// <param name="e">The event data associated with the button click.</param>
    private void IconsDirSelect_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
            return;
        
        var directoryResult = OpenFolderPickerAsync();
        directoryResult.ContinueWith(task =>
        {
            if (!task.IsCompletedSuccessfully)
                return;

            if (task.Result is not { } resultPath)
                return;
            
            viewModel.CoreConfig.Launcher.IconsDirectoryPath = resultPath;
        });
    }
    
    /// <summary>
    /// Handles the click event for selecting the Java directory.
    /// Opens a folder picker dialog to allow the user to select the Java directory
    /// and updates the configuration with the selected path.
    /// </summary>
    /// <param name="sender">The source of the event, typically a button.</param>
    /// <param name="e">The event data associated with the button click.</param>
    private void JavaDirSelect_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
            return;
        
        var directoryResult = OpenFolderPickerAsync();
        directoryResult.ContinueWith(task =>
        {
            if (!task.IsCompletedSuccessfully)
                return;

            if (task.Result is not { } resultPath)
                return;
            
            viewModel.CoreConfig.Launcher.JavaDirectoryPath = resultPath;
        });
    }

    /// <summary>
    /// Opens a folder picker dialog to select the libraries directory and updates the configuration with the selected path.
    /// </summary>
    /// <param name="sender">The button that triggered the event.</param>
    /// <param name="e">The event data associated with the button click.</param>
    private void LibrariesDirSelect_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
            return;
        
        var directoryResult = OpenFolderPickerAsync();
        directoryResult.ContinueWith(task =>
        {
            if (!task.IsCompletedSuccessfully)
                return;

            if (task.Result is not { } resultPath)
                return;
            
            viewModel.CoreConfig.Launcher.LibrariesDirectoryPath = resultPath;
        });
    }

    /// <summary>
    /// Opens a folder picker dialog to select the manifests directory and updates the configuration with the selected path.
    /// </summary>
    /// <param name="sender">The button that triggered the event.</param>
    /// <param name="e">The event data associated with the button click.</param>
    private void ManifestsDirSelect_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
            return;
        
        var directoryResult = OpenFolderPickerAsync();
        directoryResult.ContinueWith(task =>
        {
            if (!task.IsCompletedSuccessfully)
                return;

            if (task.Result is not { } resultPath)
                return;
            
            viewModel.CoreConfig.Launcher.ManifestsDirectoryPath = resultPath;
        });
    }

    /// <summary>
    /// Opens a folder picker dialog to select the translations directory and updates the configuration with the selected path.
    /// </summary>
    /// <param name="sender">The button that triggered the event.</param>
    /// <param name="e">The event data associated with the button click.</param>
    private void TranslationsDirSelect_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
            return;
        
        var directoryResult = OpenFolderPickerAsync();
        directoryResult.ContinueWith(task =>
        {
            if (!task.IsCompletedSuccessfully)
                return;

            if (task.Result is not { } resultPath)
                return;
            
            viewModel.CoreConfig.Launcher.TranslationsDirectoryPath = resultPath;
        });
    }

    /// <summary>
    /// Opens a folder picker dialog to select the versions directory and updates the configuration with the selected path.
    /// </summary>
    /// <param name="sender">The button that triggered the event.</param>
    /// <param name="e">The event data associated with the button click.</param>
    private void VersionsDirSelect_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
            return;
        
        var directoryResult = OpenFolderPickerAsync();
        directoryResult.ContinueWith(task =>
        {
            if (!task.IsCompletedSuccessfully)
                return;

            if (task.Result is not { } resultPath)
                return;
            
            viewModel.CoreConfig.Launcher.VersionsDirectoryPath = resultPath;
        });
    }
    
    /// <summary>
    /// Opens a folder picker dialog to select the Java executable path and updates the configuration with the selected path.
    /// </summary>
    /// <param name="sender">The button that triggered the event.</param>
    /// <param name="e">The event data associated with the button click.</param>
    private void JavaPathSelect_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
            return;
        
        var directoryResult = OpenFolderPickerAsync();
        directoryResult.ContinueWith(task =>
        {
            if (!task.IsCompletedSuccessfully)
                return;

            if (task.Result is not { } resultPath)
                return;
            
            viewModel.CoreConfig.Java.DefaultJavaPath = resultPath;
        });
    }
    
    /// <summary>
    /// Handles the click event for opening the Java path selector.
    /// Opens a dialog to allow the user to select a Java version and updates the configuration with the selected path.
    /// </summary>
    /// <param name="sender">The source of the event, typically a button.</param>
    /// <param name="e">The event data associated with the button click.</param>
    private async void JavaOpenPathSelector_OnClick(object? sender, RoutedEventArgs e)
    {
        // TODO: Replace async void with async Task
        var window = new JavaSelectorWindow();
        var javaVersion = await window.ShowDialog<JavaVersionModel>(this);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (javaVersion == null)
            return;
        
        if (DataContext is not MainViewModel viewModel)
            return;

        viewModel.CoreConfig.Java.DefaultJavaPath = javaVersion.Path;
    }
    #endregion
    #endregion
}