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
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using MainViewModel = Tavstal.KonkordLauncher.Desktop.Views.Models.MainViewModel;

namespace Tavstal.KonkordLauncher.Desktop.Views;

// ReSharper disable once PartialTypeWithSinglePart
public partial class MainWindow : Window
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(MainWindow));
    private PixelSize _screenSize;
    private Button _selectedButton;
    
    public MainWindow()
    {
        InitializeComponent();
        
#if DEBUG
        this.AttachDevTools(); // Attaches Avalonia Dev Tools for debugging
#endif
        
        var screen = Screens.Primary;
        if (screen == null)
            throw new InvalidOperationException("No primary screen found."); // Ensure there is a primary screen
        _screenSize = screen.Bounds.Size;
        App.SetScreenSize(_screenSize);
       
        
        // Instantiate your ViewModel and assign it to the DataContext
        this.DataContext = new MainViewModel();
        _selectedButton = PlaySideBtn;

        var settings = LauncherHelper.GetLauncherSettings();
        HandleLanguageChange(settings.Launcher.Language);
        App.OnLanguageChanged += HandleLanguageChange;
    }
    
    private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        UpdateInstancesOnPlayPage();
        UpdateNewsCards();
    }

    #region Methods

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

    private void UpdateInstancesOnPlayPage()
    {
        if (DataContext is not MainViewModel viewModel)
            return;

        // TODO: Here you would typically fetch or update the instances on the play page.
        // For demonstration, let's assume we are adding a new instance.
        viewModel.Instances.Add(new PlayCardModel { Title = "New Instance" });
        
        // After Updating:
        bool hasInstances = viewModel.Instances.Count > 0;
        PlayPageInstancesEmptyTb.IsVisible = !hasInstances;
    }

    private void UpdateNewsCards()
    {
        if (DataContext is not MainViewModel viewModel)
            return;
        
        // TODO: Fetch or update the news cards.
        
        bool hasNews = viewModel.News.Count > 0;
        NoNewsTextBlock.IsVisible = !hasNews;
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
            Title = "Select a Folder",
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

    private void HandleLanguageChange(string language)
    {
        #region Sidebar
        WelcomeLabel.Content = TranslationManager.Translate("main.subtitle");
        
        // Side Buttons
        PlaySideBtn.Content = TranslationManager.Translate("main.sidebar.play");
        NewsSideBtn.Content = TranslationManager.Translate("main.sidebar.patch");
        AccountsSideBtn.Content = TranslationManager.Translate("main.sidebar.accounts");
        SettingsSideBtn.Content = TranslationManager.Translate("main.sidebar.settings");
        AboutSideBtn.Content = TranslationManager.Translate("main.sidebar.about");
        
        VersionLabel.Content = TranslationManager.Translate("main.sidebar.version.update.none");
        #endregion

        #region Play Page

        PlayPageTitleTextBlock.Text = TranslationManager.Translate("main.page.play.title");
        PlayPageInstancesEmptyTb.Text = TranslationManager.Translate("main.page.play.empty");

        #endregion

        #region News Page

        NewsPageTitleTextBlock.Text = TranslationManager.Translate("main.page.patch.title");
        NoNewsTextBlock.Text = TranslationManager.Translate("main.page.patch.empty");

        #endregion

        #region Accounts Page

        AccountsPageTitleTextBlock.Text = TranslationManager.Translate("main.page.accounts.title");
        NoAccountsTextBlock.Text = TranslationManager.Translate("main.page.accounts.empty");

        #endregion

        #region Settings Page

        SettingsPageTitleTb.Text = TranslationManager.Translate("main.page.settings.title");
        
        SettingsLauncherTab.Header = TranslationManager.Translate("settings.tab.launcher");
        SettingsJavaTab.Header = TranslationManager.Translate("settings.tab.java");
        SettingsMinecraftTab.Header = TranslationManager.Translate("settings.tab.minecraft");
        SettingsMiscTab.Header = TranslationManager.Translate("settings.tab.misc");

        #region Launcher Settings
        StEnableUpdatesTb.Text = TranslationManager.Translate("settings.launcher.autoUpdate");
        StUpdateIntervalTb.Text = TranslationManager.Translate("settings.launcher.updateInterval");
        StLanguageTb.Text = TranslationManager.Translate("settings.launcher.language");
        StThemeTb.Text = TranslationManager.Translate("settings.launcher.theme");
        StDirectoriesSubTitle.Text = TranslationManager.Translate("settings.launcher.subtitle.directories");
        
        StAssetsTb.Text = TranslationManager.Translate("settings.launcher.dir.assets");
        StCacheTb.Text = TranslationManager.Translate("settings.launcher.dir.cache");
        StInstancesTb.Text = TranslationManager.Translate("settings.launcher.dir.instances");
        StIconsTb.Text = TranslationManager.Translate("settings.launcher.dir.icons");
        StLibrariesTb.Text = TranslationManager.Translate("settings.launcher.dir.libraries");
        StManifestsTb.Text = TranslationManager.Translate("settings.launcher.dir.manifests");
        StTranslationsTb.Text = TranslationManager.Translate("settings.launcher.dir.translations");
        StVersionsTb.Text = TranslationManager.Translate("settings.launcher.dir.versions");
        #endregion
        
        #region Minecraft Settings

        StMinecraftMaximizedTb.Text = TranslationManager.Translate("settings.minecraft.startMaximized");
        StWindowWidthTb.Text = TranslationManager.Translate("settings.minecraft.windowWidth");
        StWindowHeightTb.Text = TranslationManager.Translate("settings.minecraft.windowHeight");
        StCloseAfterOpenTb.Text = TranslationManager.Translate("settings.minecraft.closeOpen");
        StCloseAfterExitTb.Text = TranslationManager.Translate("settings.minecraft.closeExit");
        
        #endregion
        
        #region Java Settings
        
        StMinMemoryTb.Text = TranslationManager.Translate("settings.java.minMemory");
        StMaxMemoryTb.Text = TranslationManager.Translate("settings.java.maxMemory");
        StPermaGenTb.Text = TranslationManager.Translate("settings.java.permaGen");
        StJavaPathTb.Text = TranslationManager.Translate("settings.java.path");
        StJavaPathInput.Watermark = TranslationManager.Translate("common.select.directory");
        StJvmArgsTb.Text = TranslationManager.Translate("settings.java.jvmArgs");
        
        #endregion

        #region Misc Settings

        StCustomCommandsTb.Text = TranslationManager.Translate("settings.misc.subtitle.customCommands");
        StPreLaunchTb.Text = TranslationManager.Translate("settings.misc.prelaunchCommand");
        StWrapperTb.Text = TranslationManager.Translate("settings.misc.wrapperCommand");
        StPostExitTb.Text = TranslationManager.Translate("settings.misc.postExitCommand");
        StNativeLibrariesTb.Text = TranslationManager.Translate("settings.misc.subtitle.nativeLibraries");
        StUseCustomGlfwTb.Text = TranslationManager.Translate("settings.misc.customGLFW");
        StGlfwPathTb.Text = TranslationManager.Translate("settings.misc.pathGLFW");
        StUseCustomOpenAlTb.Text = TranslationManager.Translate("settings.misc.customOpenAL");
        StOpenAlPathTb.Text = TranslationManager.Translate("settings.misc.pathOpenAL");
        StPerformanceTb.Text = TranslationManager.Translate("settings.misc.subtitle.performance");
        StEnableMangoHudTb.Text = TranslationManager.Translate("settings.misc.enableMangoHud");
        StFeralGameModeTb.Text = TranslationManager.Translate("settings.misc.enableGameMode");
        StUseDedicatedGpuTb.Text = TranslationManager.Translate("settings.misc.useDedicatedGpu");

        #endregion

        #endregion
    }

    #endregion

    #region Sidebar Button Click Handlers
    public void OnPlaySideButtonClick(object? sender, RoutedEventArgs e)
    {
        HandleSidebarChange(ESidebarType.Play);
    }
    
    public void OnNewsSideButtonClick(object? sender, RoutedEventArgs e)
    {
        HandleSidebarChange(ESidebarType.Patch);
    }
    
    public void OnAccountsSideButtonClick(object? sender, RoutedEventArgs e)
    {
        HandleSidebarChange(ESidebarType.Accounts);
    }
    
    public void OnSettingsSideButtonClick(object? sender, RoutedEventArgs e)
    {
        HandleSidebarChange(ESidebarType.Settings);
    }
    
    private void OnAboutSideButtonClick(object? sender, RoutedEventArgs e)
    {
        HandleSidebarChange(ESidebarType.About);
    }
    #endregion

    #region Instance Button Click Handlers

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
        
        viewModel.AccountData.SelectedAccountId = account.Id;
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
        
        // TODO
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
    #endregion
    #endregion
}