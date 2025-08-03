using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Config.Launcher;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

/// <summary>
/// Represents the main view model for the application, managing the state and behavior of the UI.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly bool _isInitialized;
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(MainViewModel));
    
    [ObservableProperty] private ESidebarType _currentPageIndex;
    public ObservableCollection<PlayCardModel> Instances { get; } = [];
    public ObservableCollection<NewsCardModel> News { get; } = [];
    public ObservableCollection<AccountCardModel> Accounts { get; } = [];
    [ObservableProperty] private CoreConfigModel _coreConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    public MainViewModel()
    {
        _currentPageIndex = ESidebarType.Play;
        // TODO: Load instances
        // TODO: Fetch news
        // TODO: Load accounts

        _coreConfig = new CoreConfigModel(LauncherHelper.GetLauncherSettings());
        _isInitialized = true;
        
        SubscribeToCoreConfigChildren(_coreConfig);
    }

    #region Config Management
    /// <summary>
    /// Subscribes to property change events for the child configuration objects.
    /// </summary>
    /// <param name="config">The core configuration model to subscribe to.</param>
    private void SubscribeToCoreConfigChildren(CoreConfigModel config)
    {
        config.Launcher.PropertyChanged += OnChildConfigPropertyChanged;
        config.Java.PropertyChanged += OnChildConfigPropertyChanged;
        config.Minecraft.PropertyChanged += OnChildConfigPropertyChanged;
        config.Misc.PropertyChanged += OnChildConfigPropertyChanged;
    }
    
    /// <summary>
    /// Unsubscribes from property change events for the child configuration objects.
    /// </summary>
    /// <param name="config">The core configuration model to unsubscribe from.</param>
    private void UnsubscribeFromCoreConfigChildren(CoreConfigModel config)
    {
        config.Launcher.PropertyChanged -= OnChildConfigPropertyChanged;
        config.Java.PropertyChanged -= OnChildConfigPropertyChanged;
        config.Minecraft.PropertyChanged -= OnChildConfigPropertyChanged;
        config.Misc.PropertyChanged -= OnChildConfigPropertyChanged;
    }
    
    /// <summary>
    /// Handles changes to the core configuration model, unsubscribing from the old value and subscribing to the new value.
    /// </summary>
    /// <param name="oldValue">The previous core configuration model.</param>
    /// <param name="newValue">The new core configuration model.</param>
    partial void OnCoreConfigChanged(CoreConfigModel? oldValue, CoreConfigModel newValue)
    {
        _logger.Debug("CoreConfig changed with old and new value. Unsubscribing from old, subscribing to new.");

        if (oldValue != null)
            UnsubscribeFromCoreConfigChildren(oldValue);

        SubscribeToCoreConfigChildren(newValue);
        
        if (!_isInitialized)
            return;
        SaveCoreConfigToFile(newValue);
    }
    
    /// <summary>
    /// Handles property change events for child configuration objects and saves the updated configuration to a file.
    /// </summary>
    /// <param name="sender">The object that triggered the property change event.</param>
    /// <param name="e">The event data associated with the property change.</param>
    private void OnChildConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!_isInitialized)
            return;
        _logger.Debug($"Inner property '{e.PropertyName}' changed on {sender?.GetType().Name}. Saving to file...");
        SaveCoreConfigToFile(CoreConfig);
        
        // Handle theme change
        if (e.PropertyName == nameof(CoreConfig.Launcher.Theme))
        {
            App.InvokeThemeChanged(CoreConfig.Launcher.Theme);
            return;
        }
        
        // Handle language change
        if (e.PropertyName == nameof(CoreConfig.Launcher.Language))
        {
            App.InvokeLanguageChanged(CoreConfig.Launcher.Language);
            // ReSharper disable once RedundantJumpStatement, there might be additional logic in the future
            return;
        }
    }
    
    /// <summary>
    /// Saves the core configuration model to a file, preserving non-observable properties.
    /// </summary>
    /// <param name="newValue">The updated core configuration model to save.</param>
    private void SaveCoreConfigToFile(CoreConfigModel newValue)
    {
        var oldSettings = LauncherHelper.GetLauncherSettings(); // Fetch to preserve non-observable properties
        
        if (newValue.Java.MinMemory > newValue.Java.MaxMemory)
        {
            _logger.Warn("Min memory cannot be greater than max memory. Adjusting values.");
            newValue.Java.MinMemory = newValue.Java.MaxMemory;
        }
        
        var settings = new CoreConfig()
        {
            Launcher = new LauncherConfig()
            {
                EnableAutomaticUpdates = newValue.Launcher.EnableAutomaticUpdates,
                UpdateInterval = newValue.Launcher.UpdateInterval,
                NextUpdateCheck = oldSettings.Launcher.NextUpdateCheck, // Preserve
                Language = newValue.Launcher.Language,
                Theme = newValue.Launcher.Theme,
                AssetsDirectoryPath = newValue.Launcher.AssetsDirectoryPath,
                InstancesDirectoryPath = newValue.Launcher.InstancesDirectoryPath,
                CacheDirectoryPath = newValue.Launcher.CacheDirectoryPath,
                IconsDirectoryPath = newValue.Launcher.IconsDirectoryPath,
                LibrariesDirectoryPath = newValue.Launcher.LibrariesDirectoryPath,
                ManifestsDirectoryPath = newValue.Launcher.ManifestsDirectoryPath,
                TranslationsDirectoryPath = newValue.Launcher.TranslationsDirectoryPath,
                VersionsDirectoryPath = newValue.Launcher.VersionsDirectoryPath,
            },
            Java = new JavaConfig()
            {
                MinMemory = newValue.Java.MinMemory,
                MaxMemory = newValue.Java.MaxMemory,
                PermaGen = newValue.Java.PermaGen,
                DefaultJavaPath = newValue.Java.DefaultJavaPath,
                JvmArguments = newValue.Java.JvmArguments,
                JavaPaths = oldSettings.Java.JavaPaths // Preserve
            },
            Minecraft = new MinecraftConfig()
            {
                StartMaximized = newValue.Minecraft.StartMaximized,
                WindowHeight = newValue.Minecraft.WindowHeight,
                WindowWidth = newValue.Minecraft.WindowWidth,
                CloseLauncherOnGameStart = newValue.Minecraft.CloseLauncherOnGameStart,
                CloseLauncherOnGameExit = newValue.Minecraft.CloseLauncherOnGameExit,
            },
            Misc = new MiscConfig()
            {
                PreLaunchCommand = newValue.Misc.PreLaunchCommand,
                WrapperCommand = newValue.Misc.WrapperCommand,
                PostExitCommand = newValue.Misc.PostExitCommand,
                UseCustomGlfw = newValue.Misc.UseCustomGlfw,
                CustomGlfwPath = newValue.Misc.CustomGlfwPath,
                UseCustomOpenAl = newValue.Misc.UseCustomOpenAl,
                CustomOpenAlPath = newValue.Misc.CustomOpenAlPath,
                UseDedicatedGpu = newValue.Misc.UseDedicatedGpu,
                EnableMangoHud = newValue.Misc.EnableMangoHud,
                EnableFeralGameMode = newValue.Misc.EnableFeralGameMode,
            }
        };

        JsonHelper.WriteJsonFile(PathHelper.LauncherConfigPath, settings);
    }
    #endregion
}