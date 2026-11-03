using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Services;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

public partial class InstanceLogsViewModel : KonkordObservableObject
{
    private readonly IEventService _eventService = null!;
    private readonly IFilePickerService _filePickerService = null!;
    private readonly string _instanceId;
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IBeaconLogger _logger = null!;

    public InstanceLogsViewModel()
    {
        _instanceId = string.Empty;
        InstanceName = "Design Instance";
    }

    public InstanceLogsViewModel(string instanceId, IBeaconLogger<InstanceLogsViewModel> logger,
        ILauncherStore launcherStore, IEventService eventService, IFilePickerService filePickerService)
    {
        _instanceId = instanceId;

        if (Design.IsDesignMode)
            return;

        _logger = logger;
        _launcherStore = launcherStore;
        _eventService = eventService;
        _filePickerService = filePickerService;

        _ = InitAsync();
    }

    private async Task InitAsync(CancellationToken cancellationToken = default)
    {
        var instances = await _launcherStore.GetInstancesAsync(cancellationToken);
        var currentInstance = instances.FirstOrDefault(x => x.Id == _instanceId);
        if (currentInstance == null)
        {
            _logger.LogError($"Instance with ID '{_instanceId}' not found.");
            throw new KeyNotFoundException($"Instance with ID '{_instanceId}' not found.");
        }

        InstanceName = currentInstance.Name;
        GameDirectory = currentInstance.GameDirectory;

        // Logging setup
        _eventService.OnInstanceLogged += OnInstanceLogged;
        Logs.Clear();
        Logs.AddRange(_eventService.GetInstanceLogs(_instanceId).Split("\n"));
        if (Logs.Count > 0)
            await Dispatcher.UIThread.Invoke(async () => await LogsScrollToEnd.Handle(Unit.Default));
    }

    private void OnInstanceLogged(string instanceId, string logMessage)
    {
        if (instanceId != _instanceId)
            return;

        string msg = logMessage.Trim();
        if (msg.EndsWith("\n \n"))
            msg = msg.Substring(0, msg.Length - 3);
        else if (msg.EndsWith("\n"))
            msg = msg.Substring(0, msg.Length - 1);

        Logs.Add(msg);
        Dispatcher.UIThread.Invoke(async () => await LogsScrollToEnd.Handle(Unit.Default));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _logger.LogDebug("Freeing memory in EditInstanceViewModel...");
        _eventService.OnInstanceLogged -= OnInstanceLogged;

        InstanceName = string.Empty;
        GameDirectory = null;
        Logs.Clear();
    }

    [RelayCommand]
    private async Task ScrollLogsToEnd()
    {
        await LogsScrollToEnd.Handle(Unit.Default);
    }

    [RelayCommand]
    private async Task CopyLogs()
    {
        await _filePickerService.SetClipboardTextAsync(string.Join("\n", Logs));
    }

    [RelayCommand]
    private void ClearLogs()
    {
        Logs.Clear();
        _eventService.CleareInstanceLogs(_instanceId);
    }

    [RelayCommand]
    private async Task CloseWindow()
    {
        await CloseWindowInteraction.Handle(Unit.Default);
    }

    #region Interactions

    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> LogsScrollToEnd { get; } = new();

    #endregion

    #region Observable Properties

    [ObservableProperty] public partial string InstanceName { get; set; } = null!;

    [ObservableProperty] public partial string? GameDirectory { get; set; }

    public ObservableCollection<string> Logs { get; set; } = [];

    #endregion
}
