using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Tavstal.BeaconSDK.Enums;
using Tavstal.BeaconSDK.Helpers.Platform;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Config.Instance;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs.Config;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs.InstanceConfig;
using Tavstal.KonkordLauncher.Desktop.Services;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.EditInstance;

public partial class EditInstanceViewModel_Settings : KonkordObservableObject
{
    private readonly IDialogService _dialogService = null!;
    private readonly IEventService _eventService = null!;
    private readonly IFilePickerService _filePickerService = null!;
    private readonly string _instanceId = string.Empty;
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IBeaconLogger _logger = null!;
    private readonly ITranslationService _translationService = null!;

    public EditInstanceViewModel_Settings()
    {
        if (Design.IsDesignMode)
            InstanceConfig = new InstanceConfigModel();
    }

    public EditInstanceViewModel_Settings(string instanceId, ILauncherStore launcherStore, ITranslationService
            translationService, IEventService eventService, IFilePickerService filePickerService,
        IDialogService dialogService) : this()
    {
        if (Design.IsDesignMode)
            return;

        _instanceId = instanceId;
        _logger = Program.Services.GetRequiredService<IBeaconLogger<EditInstanceViewModel_Settings>>();
        _launcherStore = launcherStore;
        _translationService = translationService;
        _eventService = eventService;
        _filePickerService = filePickerService;
        _dialogService = dialogService;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRemoveEnvironmentVariable))]
    public partial int? SelectedEnvironmentVariableIndex { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRemoveEnvironmentVariable))]
    public partial InstanceConfigModel InstanceConfig { get; set; } = null!;

    [ObservableProperty] public partial int? OverridenAccountIndex { get; set; } = 0;

    public bool CanRemoveEnvironmentVariable =>
        SelectedEnvironmentVariableIndex is >= 0 && InstanceConfig.EnableEnvironment;

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        UnsubscribeFromConfigChildren(InstanceConfig);
        InstanceConfig = new InstanceConfigModel();
        SelectedEnvironmentVariableIndex = null;
    }

    public async Task InitAsync(InstanceConfig config, CancellationToken cancellationToken = default)
    {
        InstanceConfig = new InstanceConfigModel(config);

        //SubscribeToConfigChildren(InstanceConfig);
        var accounts = await _launcherStore.GetAccountDataAsync(cancellationToken);
        if (!string.IsNullOrEmpty(InstanceConfig.Misc.AccountId))
            OverridenAccountIndex = accounts.Accounts.FindIndex(x => x.Id == InstanceConfig.Misc.AccountId);
    }

    private void SubscribeToConfigChildren(InstanceConfigModel config)
    {
        config.Game.PropertyChanged += OnChildConfigPropertyChanged;
        config.Java.PropertyChanged += OnChildConfigPropertyChanged;
        config.Commands.PropertyChanged += OnChildConfigPropertyChanged;
        config.Environment.CollectionChanged += OnChildConfigCollectionChanged;
        config.Misc.PropertyChanged += OnChildConfigPropertyChanged;
    }

    private void UnsubscribeFromConfigChildren(InstanceConfigModel config)
    {
        config.Game.PropertyChanged -= OnChildConfigPropertyChanged;
        config.Java.PropertyChanged -= OnChildConfigPropertyChanged;
        config.Commands.PropertyChanged -= OnChildConfigPropertyChanged;
        config.Environment.CollectionChanged -= OnChildConfigCollectionChanged;
        config.Misc.PropertyChanged -= OnChildConfigPropertyChanged;
    }

    partial void OnInstanceConfigChanged(InstanceConfigModel? oldValue, InstanceConfigModel newValue)
    {
        _logger.LogDebug("InstanceConfig changed with old and new value. Unsubscribing from old, subscribing to new.");

        if (oldValue != null)
            UnsubscribeFromConfigChildren(oldValue);

        SubscribeToConfigChildren(newValue);

        Task.Run(async () => await SaveCoreConfigToFileAsync(InstanceConfig));
    }

    private void OnChildConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        _logger.LogDebug($"Inner property '{e.PropertyName}' changed on {sender?.GetType().Name}. Saving to file...");
        Task.Run(async () => await SaveCoreConfigToFileAsync(InstanceConfig));
    }

    private void OnChildConfigCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _logger.LogDebug($"Inner collection changed on {sender?.GetType().Name}. Saving to file...");
        Task.Run(async () => await SaveCoreConfigToFileAsync(InstanceConfig));
    }

    private async Task SaveCoreConfigToFileAsync(InstanceConfigModel newValue)
    {
        if (newValue.Java.MinMemory > newValue.Java.MaxMemory)
            newValue.Java.MinMemory = newValue.Java.MaxMemory;

        var instances = await _launcherStore.GetInstancesAsync();
        var index = 0;
        Instance? instanceToSave = null;
        foreach (var instance in instances)
        {
            if (instance.Id == _instanceId)
            {
                instanceToSave = instance;
                break;
            }

            index++;
        }

        if (instanceToSave == null)
            return;

        var environmentVariables = newValue.Environment
            .Select(x => new EnvironmentVariable(x.Key, x.Value))
            .ToList();

        instanceToSave.Config = new InstanceConfig
        {
            Java =
                new JavaConfig
                {
                    MinMemory = newValue.Java.MinMemory,
                    MaxMemory = newValue.Java.MaxMemory,
                    PermaGen = newValue.Java.PermaGen,
                    JavaPath = newValue.Java.DefaultJavaPath,
                    JvmArguments = newValue.Java.JvmArguments
                },
            Game =
                new InstanceGameConfig
                {
                    StartMaximized = newValue.Game.StartMaximized,
                    WindowHeight = newValue.Game.WindowHeight,
                    WindowWidth = newValue.Game.WindowWidth,
                    ShowConsoleWhileGameRunning = newValue.Game.ShowConsoleWhileGameRunning,
                    ShowConsoleWhenGameCrashes = newValue.Game.ShowConsoleWhenGameCrashes,
                    CloseConsoleOnGameExit = newValue.Game.CloseConsoleOnGameExit,
                    EnableFeralGameMode = newValue.Game.EnableFeralGameMode,
                    EnableMangoHud = newValue.Game.EnableMangoHud,
                    UseDedicatedGpu = newValue.Game.UseDedicatedGpu
                },
            Commands =
                new InstanceCommandsConfig
                {
                    PreLaunchCommand = newValue.Commands.PreLaunchCommand,
                    WrapperCommand = newValue.Commands.WrapperCommand,
                    PostExitCommand = newValue.Commands.PostExitCommand
                },
            EnableEnvironment = newValue.EnableEnvironment,
            Environment = environmentVariables,
            Misc = new InstanceMiscConfig
            {
                UseCustomGlfw = newValue.Misc.UseCustomGlfw,
                CustomGlfwPath = newValue.Misc.CustomGlfwPath,
                UseCustomOpenAL = newValue.Misc.UseCustomOpenAL,
                CustomOpenALPath = newValue.Misc.CustomOpenALPath,
                AccountId = newValue.Misc.AccountId,
                OverrideAccount = newValue.Misc.OverrideAccount,
                JoinServerOnLaunch = newValue.Misc.JoinServerOnLaunch,
                ServerAddress = newValue.Misc.ServerAddress
            }
        };
        instances[index] = instanceToSave;

        await _launcherStore.SaveInstancesAsync(instances);
        _eventService.RaiseInstanceUpdated(_instanceId);
        _logger.LogDebug("Saved instance config to file.");
    }

    #region Commands

    [RelayCommand]
    private async Task JavaDirSelect()
    {
        var result =
            await _filePickerService.OpenFolderPickerAsync(_translationService.Translate("common.select.directory"));
        if (string.IsNullOrEmpty(result) || !Directory.Exists(result))
            return;

        InstanceConfig.Java.DefaultJavaPath = Path.Combine(result,
            OSHelper.GetOperatingSystem() == EOperatingSystem.WINDOWS ? "javaw.exe" : "java");
    }

    [RelayCommand]
    private async Task JavaPathSelector()
    {
        var javaVersion = await _dialogService.ShowJavaSelectorAsync();
        if (javaVersion == null)
            return;
        InstanceConfig.Java.DefaultJavaPath = javaVersion.Path;
    }

    #endregion
}
