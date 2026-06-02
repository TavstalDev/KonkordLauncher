using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Common.Models.Translation;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Serialization;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Config.Launcher;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.MainView;

/// <summary>
/// View-model responsible for exposing and editing launcher configuration from the main view.
/// </summary>
public partial class MainViewModel_Config : KonkordObservableObject
{
    private readonly ICustomLogger _logger;
    private readonly ILauncherStore _launcherStore;
    private readonly MainViewModel _parent;

    [ObservableProperty]
    public partial CoreConfigModel CoreConfig { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="MainViewModel_Config"/>.
    /// </summary>
    /// <param name="parent">The parent <see cref="MainViewModel"/> that owns this sub view-model.</param>
    public MainViewModel_Config(MainViewModel parent)
    {
        _parent = parent;
        if (Design.IsDesignMode)
            return;
        
        var services = Program.ServiceProvider;
        _logger = services.GetRequiredService<ICustomLogger<MainViewModel_Config>>();
        _launcherStore = services.GetRequiredService<ILauncherStore>();
    }
    
    /// <summary>
    /// Initializes the configuration sub-view-model with the provided settings.
    /// This will create the observable model wrapper and subscribe to child property changes
    /// so that modifications made through bindings are saved automatically.
    /// </summary>
    /// <param name="settings">
    /// The <see cref="CoreConfig"/> instance read from disk or created by default which will
    /// be wrapped by a <see cref="CoreConfigModel"/> and exposed to the UI.
    /// </param>
    public Task InitAsync(CoreConfig settings)
    {
        try
        {
            CoreConfig = new CoreConfigModel(settings);
            SubscribeToCoreConfigChildren(CoreConfig);
            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }
    
    
    /// <summary>
    /// Command handler that forwards a request to switch the settings tab to the parent view-model.
    /// This method is exposed as a RelayCommand so it can be bound to buttons in the UI.
    /// </summary>
    /// <param name="tabType">The settings tab to switch to.</param>
    [RelayCommand]
    public async Task HandleSettingsBtn(ESettingsTab tabType) => await _parent.UpdateSettingsTabButtonInteraction.Handle(tabType);
    
    /// <summary>
    /// Opens a folder picker dialog to select a directory and updates the corresponding configuration path
    /// based on the provided index.
    /// </summary>
    /// <param name="rawIndex">
    /// The index representing the configuration path to update:
    /// <br/>0 - AssetsDirectoryPath,
    /// <br/>1 - CacheDirectoryPath,
    /// <br/>2 - InstancesDirectoryPath,
    /// <br/>3 - IconsDirectoryPath,
    /// <br/>4 - JavaDirectoryPath,
    /// <br/>5 - LibrariesDirectoryPath,
    /// <br/>6 - ManifestsDirectoryPath,
    /// <br/>7 - TranslationsDirectoryPath,
    /// <br/>8 - VersionsDirectoryPath,
    /// <br/>9 - DefaultJavaPath.
    /// </param>
    [RelayCommand]
    public async Task ConfigDirSelectAsync(string rawIndex)
    {
        if (!int.TryParse(rawIndex, out var index))
            return;

        var directoryResult = await _parent.OpenFolderPickerInteraction.Handle(Unit.Default);
        if (string.IsNullOrEmpty(directoryResult))
            return;
        switch (index)
        {
            case 0:
            {
                CoreConfig.Launcher.AssetsDirectoryPath = directoryResult;
                break;
            }
            case 1:
            {
                CoreConfig.Launcher.CacheDirectoryPath = directoryResult;
                break;
            }
            case 2:
            {
                CoreConfig.Launcher.InstancesDirectoryPath = directoryResult;
                break;
            }
            case 3:
            {
                CoreConfig.Launcher.IconsDirectoryPath = directoryResult;
                break;
            }
            case 4:
            {
                CoreConfig.Launcher.JavaDirectoryPath = directoryResult;
                break;
            }
            case 5:
            {
                CoreConfig.Launcher.LibrariesDirectoryPath = directoryResult;
                break;
            }
            case 6:
            {
                CoreConfig.Launcher.ManifestsDirectoryPath = directoryResult;
                break;
            }
            case 7:
            {
                CoreConfig.Launcher.TranslationsDirectoryPath = directoryResult;
                break;
            }
            case 8:
            {
                CoreConfig.Launcher.VersionsDirectoryPath = directoryResult;
                break;
            }
            case 9:
            {
                CoreConfig.Java.DefaultJavaPath = directoryResult;
                break;
            }
        }
    }

    /// <summary>
    /// Opens a Java selector window to choose a Java version and updates the default Java path
    /// in the configuration with the selected version's path.
    /// </summary>
    [RelayCommand]
    private async Task JavaPathSelectorAsync()
    {
        var javaVersion = await _parent.ShowJavaSelectorDialogInteraction.Handle(Unit.Default);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (javaVersion == null)
            return;
        CoreConfig.Java.DefaultJavaPath = javaVersion.Path;
    }
    
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
        if (_parent.IsLoading || !_parent.Initialization.IsCompletedSuccessfully)
            return; 
        
        _logger.LogDebug("CoreConfig changed with old and new value. Unsubscribing from old, subscribing to new.");

        if (oldValue != null)
            UnsubscribeFromCoreConfigChildren(oldValue);

        SubscribeToCoreConfigChildren(newValue);
        
        SaveCoreConfigToFile(newValue);
    }

    /// <summary>
    /// Handles property change events for child configuration objects and saves the updated configuration to a file.
    /// </summary>
    /// <param name="sender">The object that triggered the property change event.</param>
    /// <param name="e">The event data associated with the property change.</param>
    private void OnChildConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_parent.IsLoading || !_parent.Initialization.IsCompletedSuccessfully)
            return;
        
        _logger.LogDebug($"Inner property '{e.PropertyName}' changed on {sender?.GetType().Name}. Saving to file...");
        SaveCoreConfigToFile(CoreConfig);

        // Handle theme change
        if (e.PropertyName == nameof(CoreConfig.Launcher.Theme))
        {
            GlobalEvents.InvokeThemeChanged(CoreConfig.Launcher.Theme);
            return;
        }

        // Handle language change
        if (e.PropertyName == nameof(CoreConfig.Launcher.Language))
        {
            TranslationBindingSource.Instance.RaiseLanguageChanged();
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
        if (newValue.Java.MinMemory > newValue.Java.MaxMemory)
        {
            _logger.LogWarning("Min memory cannot be greater than max memory. Adjusting values.");
            newValue.Java.MinMemory = newValue.Java.MaxMemory;
        }

        var settings = new CoreConfig
        {
            Launcher = new LauncherConfig
            {
                EnableAutomaticUpdates = newValue.Launcher.EnableAutomaticUpdates,
                UpdateInterval = newValue.Launcher.UpdateInterval,
                NextUpdateCheck = _parent.NextUpdate,
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
            Java = new JavaConfig
            {
                MinMemory = newValue.Java.MinMemory,
                MaxMemory = newValue.Java.MaxMemory,
                PermaGen = newValue.Java.PermaGen,
                JavaPath = newValue.Java.DefaultJavaPath,
                JvmArguments = newValue.Java.JvmArguments,
            },
            Minecraft = new MinecraftConfig
            {
                StartMaximized = newValue.Minecraft.StartMaximized,
                WindowHeight = newValue.Minecraft.WindowHeight,
                WindowWidth = newValue.Minecraft.WindowWidth,
                CloseLauncherOnGameStart = newValue.Minecraft.CloseLauncherOnGameStart,
                CloseLauncherOnGameExit = newValue.Minecraft.CloseLauncherOnGameExit,
            },
            Misc = new MiscConfig
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
            },
            CacheRefreshDate = _parent.NextCacheRefresh
        };

        _launcherStore.SaveSettings(settings);
    }
}