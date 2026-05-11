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
using Tavstal.KonkordLauncher.Common.Models;
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
    }
    
    /// <summary>
    /// Performs cleanup when the view-model is disposed.
    /// Unsubscribes from global instance change events to avoid memory leaks.
    /// </summary>
    /// <param name="disposing"><c>true</c> when called from Dispose; <c>false</c> when called from a finalizer.</param>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        GlobalEvents.OnInstancesChanged -= OnInstancesChanged;
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
        
        GlobalEvents.OnInstancesChanged += OnInstancesChanged;
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
        await _parent.ShowInstanceEditDialogInteraction.Handle(instance.Id);
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
        GlobalEvents.InvokeInstancesChanged();
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
        GlobalEvents.InvokeInstancesChanged();
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
        GlobalEvents.InvokeInstancesChanged();
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

        var directoryResult = await _parent.OpenFolderPickerInteraction.Handle(Unit.Default);
        if (string.IsNullOrEmpty(directoryResult))
            return;

        string exportPath = Path.Combine(directoryResult, instance.Name + "-modrinth.mrpack");

        // TODO: Add export window
        await InstanceHelper.ExportAsync(instance.getInstance(), exportPath, EInstanceProvider.Modrinth, "1.0.0", "", cancellationToken);
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

        var directoryResult = await _parent.OpenFolderPickerInteraction.Handle(Unit.Default);
        if (string.IsNullOrEmpty(directoryResult))
            return;

        string exportPath = Path.Combine(directoryResult, instance.Name + "-curseforge.zip");

        // TODO: Add export window
        await InstanceHelper.ExportAsync(instance.getInstance(), exportPath, EInstanceProvider.CurseForge, "1.0.0", "", cancellationToken);
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
        GlobalEvents.InvokeInstancesChanged();
    }

    #endregion
    
    /// <summary>
    /// Event handler invoked when the global instances collection has changed.
    /// This method logs the event and triggers a background refresh of the view-model's
    /// instance groups by calling <see cref="HandleInstancesChangedAsync(CancellationToken)"/>.
    /// </summary>
    private void OnInstancesChanged()
    {
        _logger.Debug("Instances data changed. Updating instances collection.");
        _ = HandleInstancesChangedAsync();
    }

    /// <summary>
    /// Reloads the list of instances from disk (via <see cref="LauncherHelper.GetInstancesAsync(CancellationToken)"/>),
    /// groups them by their configured group name (falling back to an "uncategorized" translation),
    /// rebuilds the in-memory <see cref="InstanceGroups"/> collection used by the UI, and updates
    /// the <see cref="HasInstances"/> flag.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation of the refresh operation.</param>
    /// <returns>A <see cref="Task"/> that completes when the instance grouping and UI collection update finish.</returns>
    private async Task HandleInstancesChangedAsync(CancellationToken cancellationToken = default)
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

        InstanceGroups.Clear();
        foreach (var group in instanceGroups.Values)
            InstanceGroups.Add(group);

        HasInstances = InstanceGroups.Count > 0;
    }
}