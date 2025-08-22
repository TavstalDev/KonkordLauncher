using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

public partial class InstanceLogsViewModel : KonkordObservableObject
{
    private readonly string _instanceId;
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(InstanceLogsViewModel));
    
    #region Interactions
    public Interaction<Unit, Unit> CloseWindow { get; } = new();
    public Interaction<string, Unit> SetClipboardText { get; } = new();
    public Interaction<Unit, Unit> LogsScrollToEnd { get; } = new();

    #endregion

    #region Observable Properties

    [ObservableProperty] private string _instanceName;
    [ObservableProperty] private string? _gameDirectory;
    [ObservableProperty] private string _logs;

    #endregion
    
    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceLogsViewModel"/> class with the specified instance ID.
    /// Retrieves the instance details and sets up logging for the instance.
    /// </summary>
    /// <param name="instanceId">The unique identifier of the instance to be managed by this view model.</param>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when an instance with the specified ID cannot be found.
    /// </exception>
    public InstanceLogsViewModel(string instanceId)
    {
        if (Design.IsDesignMode)
            return;

        _instanceId = instanceId;
        var instances = LauncherHelper.GetInstances();
        var currentInstance = instances.FirstOrDefault(x => x.Id == _instanceId);
        if (currentInstance == null)
        {
            _logger.Error($"Instance with ID '{_instanceId}' not found.");
            throw new KeyNotFoundException($"Instance with ID '{_instanceId}' not found.");
        }

        _instanceName = currentInstance.Name;
        _gameDirectory = currentInstance.GameDirectory;

        // Logging setup
        GlobalEvents.OnInstanceLogged += OnInstanceLogged;
        Logs = GlobalEvents.GetInstanceLogs(_instanceId);
        if (!string.IsNullOrEmpty(Logs))
            Dispatcher.UIThread.Invoke(async () => await LogsScrollToEnd.Handle(Unit.Default));
    }

    /// <summary>
    /// Handles log messages for a specific instance by updating the Logs property
    /// and triggering the LogsScrollToEnd interaction to scroll to the end of the logs.
    /// </summary>
    /// <param name="instanceId">The ID of the instance that generated the log message.</param>
    /// <param name="logMessage">The log message to be handled.</param>
    private void OnInstanceLogged(string instanceId, string logMessage)
    {
        if (instanceId != _instanceId)
            return;

        Logs += logMessage;
        Dispatcher.UIThread.Invoke(async () => await LogsScrollToEnd.Handle(Unit.Default));
    }
    
    /// <summary>
    /// Disposes of the resources used by the <see cref="InstanceLogsViewModel"/>.
    /// Unsubscribes from global events and clears instance-related data to free memory.
    /// </summary>
    /// <param name="disposing">
    /// A boolean value indicating whether the method is being called directly or by the garbage collector.
    /// If true, the method has been called directly and managed resources can be disposed.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _logger.Debug("Freeing memory in EditInstanceViewModel...");
        GlobalEvents.OnInstanceLogged -= OnInstanceLogged;
        
        InstanceName = string.Empty;
        GameDirectory = null;
        Logs = string.Empty;
    }
    
    /// <summary>
    /// Scrolls the logs to the end by triggering the LogsScrollToEnd interaction.
    /// </summary>
    [RelayCommand]
    private async Task ScrollLogsToEnd() => await LogsScrollToEnd.Handle(Unit.Default);

    /// <summary>
    /// Copies the current logs to the system clipboard by triggering the SetClipboardText interaction.
    /// </summary>
    [RelayCommand]
    private async Task CopyLogs() => await SetClipboardText.Handle(Logs);

    /// <summary>
    /// Clears the logs for the current instance and updates the global log storage.
    /// </summary>
    [RelayCommand]
    private void ClearLogs()
    {
        Logs = string.Empty;
        GlobalEvents.CleareInstanceLogs(_instanceId);
    }
}