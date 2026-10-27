using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Tavstal.BeaconSDK.Helpers.IO;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Services;
using Tavstal.KonkordLauncher.Desktop.Services.Instances;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.MainView;

/// <summary>
///     View-model responsible for managing the "Instances" section of the main view.
/// </summary>
public partial class MainViewModel_Instances : KonkordObservableObject
{
    private readonly IEventService _eventService = null!;
    private readonly IInstanceModelFactory _instanceModelFactory = null!;
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IBeaconLogger _logger = null!;
    private readonly ITranslationService _translationService = null!;
    private readonly IFilePickerService _filePickerService = null!;
    private readonly IDialogService _dialogService = null!;

    public MainViewModel_Instances()
    {

    }

    public MainViewModel_Instances(ILauncherStore launcherStore, ITranslationService translationService,
        IEventService eventService, IInstanceModelFactory instanceModelFactory,
        IFilePickerService filePickerService, IDialogService dialogService)
    {
        if (Design.IsDesignMode)
            return;

        _logger = Program.Services.GetRequiredService<IBeaconLogger<MainViewModel_Instances>>();
        _launcherStore = launcherStore;
        _translationService = translationService;
        _eventService = eventService;
        _instanceModelFactory = instanceModelFactory;
        _filePickerService = filePickerService;
        _dialogService = dialogService;

        _eventService.OnInstanceAdded += OnInstanceAdded;
        _eventService.OnInstanceUpdated += OnInstanceUpdated;
        _eventService.OnInstanceRemoved += OnInstanceRemoved;
    }

    public ObservableCollection<InstanceGroup> InstanceGroups { get; } = [];

    [ObservableProperty]
    public partial bool HasInstances { get; set; }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _eventService.OnInstanceAdded -= OnInstanceAdded;
        _eventService.OnInstanceUpdated -= OnInstanceUpdated;
        _eventService.OnInstanceRemoved -= OnInstanceRemoved;
    }

    private void OnInstanceAdded(string instanceId)
    {
        _ = HandleInstanceCreatedAsync(instanceId).ContinueWith(task =>
        {
            if (task.IsFaulted)
                _logger.LogError(task.Exception, "Instance added event handling error.");
        }, TaskScheduler.Default);
    }

    private void OnInstanceUpdated(string instanceId)
    {
        _ = HandleInstanceUpdatedAsync(instanceId).ContinueWith(task =>
        {
            if (task.IsFaulted)
                _logger.LogError(task.Exception, "Instance updated event handling error.");
        }, TaskScheduler.Default);
    }

    private void OnInstanceRemoved(string instanceId)
    {
        _ = HandleInstanceRemovedAsync(instanceId).ContinueWith(task =>
        {
            if (task.IsFaulted)
                _logger.LogError(task.Exception, "Instance removed event handling error.");
        }, TaskScheduler.Default);
    }

    public async Task InitAsync(CancellationToken cancellationToken = default)
    {
        var instances = await _launcherStore.GetInstancesAsync(cancellationToken);
        var instanceGroups = new Dictionary<string, InstanceGroup>();
        var uncategorized = _translationService.Translate("main.page.play.uncategorized");
        foreach (var instance in instances)
        {
            var key = instance.Group ?? string.Empty;
            if (instanceGroups.ContainsKey(key))
            {
                instanceGroups[key].Instances.Add(_instanceModelFactory.Create(instance));
            }
            else
            {
                var groupName = instance.Group ?? uncategorized;
                var newGroup = new InstanceGroup(groupName);
                newGroup.Instances.Add(_instanceModelFactory.Create(instance));
                instanceGroups.Add(key, newGroup);
            }
        }

        foreach (var group in instanceGroups.Values)
            InstanceGroups.Add(group);

        HasInstances = InstanceGroups.Count > 0;
    }

    private async Task HandleInstanceCreatedAsync(string instanceId, CancellationToken cancellationToken = default)
    {
        var instances = await _launcherStore.GetInstancesAsync(cancellationToken);
        var targetInstance = instances.FirstOrDefault(i => i.Id == instanceId);
        if (targetInstance == null)
            return;

        var uncategorized = _translationService.Translate("main.page.play.uncategorized");
        var groupName = targetInstance.Group ?? uncategorized;

        var existingGroup = InstanceGroups.FirstOrDefault(x => x.GroupName == groupName);
        if (existingGroup == null)
        {
            var newGroup = new InstanceGroup(groupName);
            newGroup.Instances.Add(_instanceModelFactory.Create(targetInstance));
            InstanceGroups.Add(newGroup);
            HasInstances = InstanceGroups.Count > 0;
            return;
        }

        existingGroup.Instances.Add(_instanceModelFactory.Create(targetInstance));
    }

    private async Task HandleInstanceUpdatedAsync(string instanceId, CancellationToken cancellationToken = default)
    {
        var instances = await _launcherStore.GetInstancesAsync(cancellationToken);
        var targetInstance = instances.FirstOrDefault(i => i.Id == instanceId);
        if (targetInstance == null)
            return;

        var found = false;
        InstanceGroup? oldGroup = null;
        InstanceModel? instanceToUpdate = null;
        foreach (var group in InstanceGroups)
        {
            foreach (var instance in group.Instances.ToList())
            {
                if (instance.Id != instanceId)
                    continue;

                var shouldUpdateGroup = instance.Group != targetInstance.Group;
                oldGroup = group;
                instance.UpdateDetails(targetInstance);
                if (shouldUpdateGroup)
                    instanceToUpdate = instance;

                found = true;
                break;
            }

            if (found)
                break;
        }

        if (instanceToUpdate == null || oldGroup == null)
            return;

        oldGroup.Instances.Remove(instanceToUpdate);
        if (oldGroup.Instances.Count == 0)
            InstanceGroups.Remove(oldGroup);

        var uncategorized = _translationService.Translate("main.page.play.uncategorized");
        var groupName = targetInstance.Group ?? uncategorized;
        var newGroup = InstanceGroups.FirstOrDefault(x => x.GroupName == groupName);
        if (newGroup == null)
        {
            newGroup = new InstanceGroup(groupName);
            newGroup.Instances.Add(instanceToUpdate);
            InstanceGroups.Add(newGroup);
            HasInstances = InstanceGroups.Count > 0;
            return;
        }

        newGroup.Instances.Add(instanceToUpdate);
    }

    private Task HandleInstanceRemovedAsync(string instanceId)
    {
        var found = false;
        InstanceModel? instanceToRemove = null;
        InstanceGroup? targetGroup = null;
        foreach (var group in InstanceGroups)
        {
            foreach (var instance in group.Instances)
            {
                if (instance.Id != instanceId)
                    continue;
                targetGroup = group;
                instanceToRemove = instance;
                found = true;
                break;
            }

            if (found)
                break;
        }

        if (instanceToRemove == null || targetGroup == null)
            return Task.CompletedTask;

        targetGroup.Instances.Remove(instanceToRemove);
        if (targetGroup.Instances.Count == 0)
            InstanceGroups.Remove(targetGroup);

        HasInstances = InstanceGroups.Count > 0;
        return Task.CompletedTask;
    }

    #region Commands

    [RelayCommand]
    private async Task AddInstanceBtnAsync()
    {
        await _dialogService.ShowInstanceCreationAsync();
    }

    [RelayCommand]
    private async Task LaunchInstance(InstanceModel? instance, CancellationToken cancellationToken = default)
    {
        if (instance == null)
            return;
        await instance.LaunchAsync();
    }

    [RelayCommand]
    private void StopInstance(InstanceModel? instance)
    {
        if (instance == null)
            return;

        if (!instance.IsGameRunning || instance.GameProcess == null)
        {
            _logger.LogWarning($"Instance {instance.Name} is not running or has no associated process.");
            return;
        }

        instance.GameProcess.Kill();
    }

    [RelayCommand]
    private async Task EditInstance(InstanceModel? instance)
    {
        if (instance == null)
            return;
        await _dialogService.ShowInstanceEditorAsync(instance);
    }

    [RelayCommand]
    private async Task ViewInstanceLogs(InstanceModel? instance)
    {
        if (instance == null)
            return;
        await _dialogService.ShowInstanceLogsAsync(instance.Id);
    }

    [RelayCommand]
    private async Task RenameInstance(InstanceModel? instance, CancellationToken cancellationToken = default)
    {
        if (instance == null)
            return;

        var instances = await _launcherStore.GetInstancesAsync(cancellationToken);
        var targetInstance = instances.FirstOrDefault(i => i.Id == instance.Id);
        if (targetInstance == null)
            return;

        var index = instances.IndexOf(targetInstance);
        var result =
            await _dialogService.ShowInputAsync(_translationService.Translate("instance.rename.title"));
        if (string.IsNullOrEmpty(result))
            return;

        if (instances.Any(x => x.Name.Equals(result, StringComparison.OrdinalIgnoreCase)))
        {
            await _dialogService.ShowAlertAsync(
                _translationService.Translate("common.error"),
                _translationService.Translate("instance.rename.duplicate"),
                EAlertType.Error);
            return;
        }

        targetInstance.Name = result;
        instances[index] = targetInstance;
        await _launcherStore.SaveInstancesAsync(instances, cancellationToken);
        _eventService.RaiseInstanceUpdated(targetInstance.Id);
    }

    [RelayCommand]
    private async Task ChangeInstanceIcon(InstanceModel? instance, CancellationToken cancellationToken = default)
    {
        if (instance == null)
            return;

        var instances = await _launcherStore.GetInstancesAsync(cancellationToken);
        var targetInstance = instances.FirstOrDefault(i => i.Id == instance.Id);
        if (targetInstance == null)
            return;

        var index = instances.IndexOf(targetInstance);
        var result = await _dialogService.ShowIconSelectorAsync();
        if (string.IsNullOrEmpty(result))
            return;

        targetInstance.IconPath = result;
        instances[index] = targetInstance;
        await _launcherStore.SaveInstancesAsync(instances, cancellationToken);
        _eventService.RaiseInstanceUpdated(targetInstance.Id);
    }

    [RelayCommand]
    private async Task ChangeInstanceVersion(InstanceModel? instance, CancellationToken cancellationToken = default)
    {
        if (instance == null)
            return;
        await _dialogService.ShowInstanceVersionSelectorAsync(instance.getInstance());
    }

    [RelayCommand]
    private async Task ChangeInstanceGroup(InstanceModel? instance, CancellationToken cancellationToken = default)
    {
        if (instance == null)
            return;

        var instances = await _launcherStore.GetInstancesAsync(cancellationToken);
        var targetInstance = instances.FirstOrDefault(i => i.Id == instance.Id);
        if (targetInstance == null)
            return;

        var index = instances.IndexOf(targetInstance);
        var result =
            await _dialogService.ShowInputAsync(_translationService.Translate("instance.change.group.title"));
        if (string.IsNullOrEmpty(result))
            return;

        targetInstance.Group = result;
        instances[index] = targetInstance;
        await _launcherStore.SaveInstancesAsync(instances, cancellationToken);
        _eventService.RaiseInstanceUpdated(targetInstance.Id);
    }

    [RelayCommand]
    private void OpenInstanceDir(InstanceModel? instance)
    {
        if (instance == null)
            return;

        if (string.IsNullOrEmpty(instance.GameDirectory))
            return;

        FileSystemHelper.OpenFolderInFileExplorer(instance.GameDirectory);
    }

    [RelayCommand]
    private async Task ExportModrinthInstance(InstanceModel? instance, CancellationToken cancellationToken = default)
    {
        if (instance == null)
            return;

        await _dialogService.ShowInstanceExportAsync(instance.getInstance(), EInstanceProvider.MODRINTH);
    }

    [RelayCommand]
    private async Task ExportCurseForgeInstance(InstanceModel? instance, CancellationToken cancellationToken = default)
    {
        if (instance == null)
            return;

        await _dialogService.ShowInstanceExportAsync(instance.getInstance(), EInstanceProvider.CURSE_FORGE);
    }

    [RelayCommand]
    private async Task DeleteInstance(InstanceModel? instance, CancellationToken cancellationToken = default)
    {
        if (instance == null)
            return;

        var result = await _dialogService.ShowAlertAsync(
            _translationService.Translate("instance.delete.title"),
            _translationService.Translate("instance.delete.message", instance.Name),
            EAlertType.Confirm);
        if (!result)
            return;

        var instances = await _launcherStore.GetInstancesAsync(cancellationToken);
        var targetInstance = instances.FirstOrDefault(i => i.Id == instance.Id);
        if (targetInstance == null)
            return;

        if (string.IsNullOrEmpty(targetInstance.GameDirectory))
            return;

        if (Directory.Exists(targetInstance.GameDirectory))
            FileSystemHelper.DeleteDirectory(targetInstance.GameDirectory);
        instances.Remove(targetInstance);
        await _launcherStore.SaveInstancesAsync(instances, cancellationToken);
        _eventService.RaiseInstanceRemoved(targetInstance.Id);
    }

    #endregion
}
