using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Serialization;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.MainView;

/// <summary>
/// View-model responsible for managing the "Instances" section of the main view.
/// </summary>
public partial class MainViewModel_Instances : KonkordObservableObject
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(MainViewModel_Instances));
    private readonly MainViewModel _parent;
    
    public ObservableCollection<InstanceGroup> InstanceGroups { get; } = new();
    [ObservableProperty] private bool _hasInstances;
    
    /// <summary>
    /// Creates a new instance of the <see cref="MainViewModel_Instances"/> sub-view-model.
    /// </summary>
    /// <param name="parent">The parent <see cref="MainViewModel"/> used for interactions.</param>
    public MainViewModel_Instances(MainViewModel parent)
    {
        _parent = parent;
        GlobalEvents.OnInstanceAdded += OnInstanceAdded;
        GlobalEvents.OnInstanceUpdated += OnInstanceUpdated;
        GlobalEvents.OnInstanceRemoved += OnInstanceRemoved;
    }
    
    /// <summary>
    /// Performs cleanup when the view-model is disposed.
    /// Unsubscribes from global instance change events to avoid memory leaks.
    /// </summary>
    /// <param name="disposing"><c>true</c> when called from Dispose; <c>false</c> when called from a finalizer.</param>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        GlobalEvents.OnInstanceAdded -= OnInstanceAdded;
        GlobalEvents.OnInstanceUpdated -= OnInstanceUpdated;
        GlobalEvents.OnInstanceRemoved -= OnInstanceRemoved;
    }
    
    /// <summary>
    /// Handles the global "instance added" event by kicking off asynchronous creation handling.
    /// </summary>
    /// <param name="instanceId">The ID of the instance that was added.</param>
    private void OnInstanceAdded(string instanceId)
    {
        _ =  HandleInstanceCreatedAsync(instanceId);
    }

    /// <summary>
    /// Handles the global "instance updated" event by kicking off asynchronous update handling.
    /// </summary>
    /// <param name="instanceId">The ID of the instance that was updated.</param>
    private void OnInstanceUpdated(string instanceId)
    {
        _ =  HandleInstanceUpdatedAsync(instanceId);
    }

    /// <summary>
    /// Handles the global "instance removed" event by kicking off asynchronous removal handling.
    /// </summary>
    /// <param name="instanceId">The ID of the instance that was removed.</param>
    private void OnInstanceRemoved(string instanceId)
    {
        _ = HandleInstanceRemovedAsync(instanceId);
    }

    /// <summary>
    /// Initializes this sub-view-model by loading instances from disk and grouping them.
    /// Populates <see cref="InstanceGroups"/>, sets <see cref="HasInstances"/>, and subscribes
    /// to global instance change events.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the initial load operation.</param>
    /// <returns>A task that completes when initialization has finished.</returns>
    public async Task InitAsync(CancellationToken cancellationToken = default)
    {
        var instances = await LauncherHelper.GetInstancesAsync(cancellationToken);
        var instanceGroups = new Dictionary<string, InstanceGroup>();
        string uncategorized = TranslationManager.Translate("main.page.play.uncategorized");
        foreach (var instance in instances)
        {
            string key = instance.Group ?? string.Empty;
            if (instanceGroups.ContainsKey(key))
            {
                instanceGroups[key].Instances.Add(new InstanceModel(instance));
            }
            else
            {
                var groupName = instance.Group ?? uncategorized;
                var newGroup = new InstanceGroup(groupName);
                newGroup.Instances.Add(new InstanceModel(instance));
                instanceGroups.Add(key, newGroup);
            }
        }

        foreach (var group in instanceGroups.Values)
            InstanceGroups.Add(group);

        HasInstances = InstanceGroups.Count > 0;
    }
    
    #region Commands

    /// <summary>
    /// Opens the "Create Instance" window to allow the user to add a new Minecraft instance asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [RelayCommand]
    private async Task AddInstanceBtnAsync() => await _parent.ShowInstanceCreationDialogInteraction.Handle(Unit.Default);

    /// <summary>
    /// Launches the specified Minecraft instance asynchronously.
    /// </summary>
    /// <param name="instance">The instance model representing the Minecraft instance to launch.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [RelayCommand]
    private async Task LaunchInstance(InstanceModel? instance, CancellationToken cancellationToken = default)
    {
        if (instance == null)
            return;
        await instance.LaunchAsync(_parent.ShowLogsWindowInteraction, _parent.CloseLogsWindowInteraction, _parent.CloseWindowInteraction, _parent.ShowAlertDialogInteraction);
    }

    /// <summary>
    /// Stops the specified Minecraft instance if it is currently running.
    /// </summary>
    /// <param name="instance">The instance model representing the Minecraft instance to stop.</param>
    [RelayCommand]
    private void StopInstance(InstanceModel? instance)
    {
        if (instance == null)
            return;

        if (!instance.IsGameRunning || instance.GameProcess == null)
        {
            _logger.Warn($"Instance {instance.Name} is not running or has no associated process.");
            return;
        }

        instance.GameProcess.Kill();
    }

    /// <summary>
    /// Opens an edit window for the specified Minecraft instance and updates the instance in the collection if changes are made.
    /// </summary>
    /// <param name="instance">The instance model representing the Minecraft instance to edit.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [RelayCommand]
    private async Task EditInstance(InstanceModel? instance)
    {
        if (instance == null)
            return;
        await _parent.ShowInstanceEditDialogInteraction.Handle(instance);
    }

    /// <summary>
    /// Displays the logs of the specified Minecraft instance in a separate window asynchronously.
    /// </summary>
    /// <param name="instance">The instance model representing the Minecraft instance whose logs are to be viewed.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [RelayCommand]
    private async Task ViewInstanceLogs(InstanceModel? instance)
    {
        if (instance == null)
            return;
        await _parent.ShowLogsWindowInteraction.Handle(instance.Id);
    }

    /// <summary>
    /// Renames the specified Minecraft instance asynchronously.
    /// Prompts the user for a new name, validates it, and updates the instance if valid.
    /// </summary>
    /// <param name="instance">The instance model representing the Minecraft instance to rename.</param>
    ///  <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    [RelayCommand]
    private async Task RenameInstance(InstanceModel? instance, CancellationToken cancellationToken = default)
    {
        if (instance == null)
            return;

        var instances = await LauncherHelper.GetInstancesAsync(cancellationToken);
        var targetInstance = instances.FirstOrDefault(i => i.Id == instance.Id);
        if (targetInstance == null)
            return;

        int index = instances.IndexOf(targetInstance);
        var result = await _parent.ShowTextInputDialogInteraction.Handle(TranslationManager.Translate("instance.rename.title"));
        if (string.IsNullOrEmpty(result))
            return;

        if (instances.Any(x => x.Name.Equals(result, StringComparison.OrdinalIgnoreCase)))
        {
            await _parent.ShowAlertDialogInteraction.Handle(new Alert(TranslationManager.Translate("common.error"),
                TranslationManager.Translate("instance.rename.duplicate"), EAlertType.Error));
            return;
        }

        targetInstance.Name = result;
        instances[index] = targetInstance;
        await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherInstancesPath, instances, cancellationToken);
        GlobalEvents.InvokeInstanceUpdated(targetInstance.Id);
    }

    /// <summary>
    /// Changes the icon of the specified Minecraft instance asynchronously.
    /// Opens an icon selector dialog, validates the selection, and updates the instance if valid.
    /// </summary>
    /// <param name="instance">The instance model representing the Minecraft instance to update the icon for.</param>
    ///  <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    [RelayCommand]
    private async Task ChangeInstanceIcon(InstanceModel? instance, CancellationToken cancellationToken = default)
    {
        if (instance == null)
            return;

        var instances = await LauncherHelper.GetInstancesAsync(cancellationToken);
        var targetInstance = instances.FirstOrDefault(i => i.Id == instance.Id);
        if (targetInstance == null)
            return;

        int index = instances.IndexOf(targetInstance);
        var result = await _parent.ShowIconSelectorDialogInteraction.Handle(Unit.Default);
        if (string.IsNullOrEmpty(result))
            return;

        targetInstance.IconPath = result;
        instances[index] = targetInstance;
        await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherInstancesPath, instances, cancellationToken);
        GlobalEvents.InvokeInstanceUpdated(targetInstance.Id);
    }

    /// <summary>
    /// Changes the group of the specified Minecraft instance asynchronously.
    /// Prompts the user for a new group name, validates it, and updates the instance if valid.
    /// </summary>
    /// <param name="instance">The instance model representing the Minecraft instance to update the group for.</param>
    ///  <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    [RelayCommand]
    private async Task ChangeInstanceGroup(InstanceModel? instance, CancellationToken cancellationToken = default)
    {
        if (instance == null)
            return;

        var instances = await LauncherHelper.GetInstancesAsync(cancellationToken);
        var targetInstance = instances.FirstOrDefault(i => i.Id == instance.Id);
        if (targetInstance == null)
            return;

        int index = instances.IndexOf(targetInstance);
        var result = await _parent.ShowTextInputDialogInteraction.Handle(TranslationManager.Translate("instance.change.group.title"));
        if (string.IsNullOrEmpty(result))
            return;

        targetInstance.Group = result;
        instances[index] = targetInstance;
        await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherInstancesPath, instances, cancellationToken);
        GlobalEvents.InvokeInstanceUpdated(targetInstance.Id);
    }

    /// <summary>
    /// Opens the directory of the specified Minecraft instance in the file explorer.
    /// </summary>
    /// <param name="instance">The instance model representing the Minecraft instance whose directory is to be opened.</param>
    [RelayCommand]
    private void OpenInstanceDir(InstanceModel? instance)
    {
        if (instance == null)
            return;

        if (string.IsNullOrEmpty(instance.GameDirectory))
            return;

        FileSystemHelper.OpenFolderInFileExplorer(instance.GameDirectory);
    }
    
    /// <summary>
    /// Exports the specified Minecraft instance in the Modrinth format asynchronously.
    /// </summary>
    /// <param name="instance">The instance model representing the Minecraft instance to export.</param>
    ///  <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    [RelayCommand]
    private async Task ExportModrinthInstance(InstanceModel? instance, CancellationToken cancellationToken = default)
    {
        if (instance == null)
            return;
        
        await _parent.ExportModrinthInstanceInteraction.Handle(instance.getInstance());
    }

    /// <summary>
    /// Exports the specified Minecraft instance in the CurseForge format asynchronously.
    /// </summary>
    /// <param name="instance">The instance model representing the Minecraft instance to export.</param>
    ///  <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    [RelayCommand]
    private async Task ExportCurseForgeInstance(InstanceModel? instance, CancellationToken cancellationToken = default)
    {
        if (instance == null)
            return;
        
        await _parent.ExportCurseForgeInstanceInteraction.Handle(instance.getInstance());
    }

    /// <summary>
    /// Deletes the specified Minecraft instance asynchronously.
    /// Prompts the user for confirmation before proceeding with the deletion.
    /// If confirmed, removes the instance from the list and deletes its associated directory.
    /// </summary>
    /// <param name="instance">The instance model representing the Minecraft instance to delete.</param>
    ///  <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [RelayCommand]
    private async Task DeleteInstance(InstanceModel? instance, CancellationToken cancellationToken = default)
    {
        if (instance == null)
            return;

        var result = await _parent.ShowConfirmDialogInteraction.Handle(new Alert(TranslationManager.Translate("instance.delete.title"),
            TranslationManager.Translate("instance.delete.message", instance.Name), EAlertType.Confirm));
        if (!result)
            return;

        var instances = await LauncherHelper.GetInstancesAsync(cancellationToken);
        var targetInstance = instances.FirstOrDefault(i => i.Id == instance.Id);
        if (targetInstance == null)
            return;

        if (string.IsNullOrEmpty(targetInstance.GameDirectory))
            return;

        if (Directory.Exists(targetInstance.GameDirectory))
            FileSystemHelper.DeleteDirectory(targetInstance.GameDirectory);
        instances.Remove(targetInstance);
        await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherInstancesPath, instances, cancellationToken);
        GlobalEvents.InvokeInstanceRemoved(targetInstance.Id);
    }

    #endregion

    /// <summary>
    /// Handles an instance creation event by loading the newly created instance from disk
    /// and inserting it into the correct <see cref="InstanceGroup"/> in the UI collection.
    /// </summary>
    /// <param name="instanceId">The ID of the newly created instance.</param>
    /// <param name="cancellationToken">Token used to cancel the disk read operation.</param>
    private async Task HandleInstanceCreatedAsync(string instanceId, CancellationToken cancellationToken = default)
    {
        var instances = await LauncherHelper.GetInstancesAsync(cancellationToken);
        var targetInstance = instances.FirstOrDefault(i => i.Id == instanceId);
        if (targetInstance == null)
            return;

        string uncategorized = TranslationManager.Translate("main.page.play.uncategorized");
        var groupName = targetInstance.Group ?? uncategorized;
        
        var existingGroup = InstanceGroups.FirstOrDefault(x => x.GroupName == groupName);
        if (existingGroup == null)
        {
            var newGroup = new InstanceGroup(groupName);
            newGroup.Instances.Add(new InstanceModel(targetInstance));
            InstanceGroups.Add(newGroup);
            HasInstances = InstanceGroups.Count > 0;
            return;
        }

        existingGroup.Instances.Add(new InstanceModel(targetInstance));
    }
    
    /// <summary>
    /// Handles an instance update event by refreshing the existing item in the UI collection
    /// and moving it to a different group when needed.
    /// </summary>
    /// <param name="instanceId">The ID of the updated instance.</param>
    /// <param name="cancellationToken">Token used to cancel the disk read operation.</param>
    private async Task HandleInstanceUpdatedAsync(string instanceId, CancellationToken cancellationToken = default)
    {
        var instances = await LauncherHelper.GetInstancesAsync(cancellationToken);
        var targetInstance = instances.FirstOrDefault(i => i.Id == instanceId);
        if (targetInstance == null)
            return;
        
        bool found = false;
        InstanceGroup? oldGroup = null;
        InstanceModel? instanceToUpdate = null;
        foreach (var group in InstanceGroups)
        {
            foreach (var instance in group.Instances.ToList())
            {
                if (instance.Id != instanceId)
                    continue;

                bool shouldUpdateGroup = instance.Group != targetInstance.Group;
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
        
        string uncategorized = TranslationManager.Translate("main.page.play.uncategorized");
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
    
    /// <summary>
    /// Handles an instance removal event by removing the corresponding item from the
    /// appropriate group and removing empty groups from the UI collection.
    /// </summary>
    /// <param name="instanceId">The ID of the removed instance.</param>
    /// <param name="cancellationToken">Unused cancellation token (present for signature consistency).</param>
    /// <returns>A completed task.</returns>
    private Task HandleInstanceRemovedAsync(string instanceId, CancellationToken cancellationToken = default)
    {
        bool found = false;
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
}