using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Views;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

namespace Tavstal.KonkordLauncher.Desktop.Services.Presentation;

/// <inheritdoc />
public class DialogService : IDialogService
{
    private readonly Dictionary<string, InstanceLogsWindow> _logWindows = [];
    private readonly Dictionary<string, EditInstanceWindow> _openEditWindows = [];
    private readonly IWindowFactory _windowFactory;
    private readonly IBitmapService _bitmapService;
    private Window? _mainWindow;

    public DialogService(IWindowFactory windowFactory, IBitmapService bitmapService)
    {
        _windowFactory = windowFactory;
        _bitmapService = bitmapService;
    }

    /// <inheritdoc />
    public void SetMainWindow(Window mainWindow)
    {
        _mainWindow = mainWindow;
    }

    /// <inheritdoc />
    public Task CloseAsync()
    {
        try
        {
            _mainWindow?.Close();
            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }

    /// <inheritdoc />
    public async Task<bool> ShowAlertAsync(string title, string message, EAlertType type)
    {
        if (_mainWindow == null)
            return false;

        AlertWindow alertWindow = new(title, message, type, _bitmapService);
        return await alertWindow.ShowDialog<bool>(_mainWindow);
    }

    /// <inheritdoc />
    public async Task<bool> ShowConfirmAsync(string title, string message, EAlertType type)
    {
        return await ShowAlertAsync(title, message, type);
    }

    /// <inheritdoc />
    public async Task ShowAccountsAsync()
    {
        if (_mainWindow == null)
            return;

        var dialog = _windowFactory.CreateAccountsWindow();
        await dialog.ShowDialog(_mainWindow);
    }

    /// <inheritdoc />
    public async Task<JavaVersionModel?> ShowJavaSelectorAsync()
    {
        if (_mainWindow == null)
            return null;
        var window = _windowFactory.CreateJavaSelectorWindow();
        return await window.ShowDialog<JavaVersionModel>(_mainWindow);
    }

    /// <inheritdoc />
    public async Task<string?> ShowInputAsync(string title)
    {
        if (_mainWindow == null)
            return null;

        var dialog = new InputWindow(title, _bitmapService);
        return await dialog.ShowDialog<string?>(_mainWindow);
    }

    /// <inheritdoc />
    public async Task<string?> ShowIconSelectorAsync()
    {
        if (_mainWindow == null)
            return null;

        var dialog = _windowFactory.CreateIconSelectorWindow();
        return await dialog.ShowDialog<string?>(_mainWindow);
    }

    /// <inheritdoc />
    public async Task ShowInstanceCreationAsync()
    {
        if (_mainWindow == null)
            return;
        await _windowFactory.CreateCreateInstanceWindow().ShowDialog(_mainWindow);
    }

    /// <inheritdoc />
    public Task ShowInstanceEditorAsync(InstanceModel instance)
    {
        try
        {
            if (_mainWindow == null)
                return Task.CompletedTask;

            if (_openEditWindows.TryGetValue(instance.Id, out var window))
            {
                window.Activate();
                if (window.WindowState == WindowState.Minimized)
                    window.WindowState = WindowState.Normal;
                return Task.CompletedTask;
            }

            var editInstanceWindow = _windowFactory.CreateEditInstanceWindow(instance);
            editInstanceWindow.Show(_mainWindow);
            _openEditWindows.Add(instance.Id, editInstanceWindow);
            editInstanceWindow.Closed += (_, _) => _openEditWindows.Remove(instance.Id);
            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }

    /// <inheritdoc />
    public Task ShowInstanceLogsAsync(string instanceId)
    {
        try
        {
            if (_mainWindow == null)
                return Task.CompletedTask;

            var window = _windowFactory.CreateInstanceLogsWindow(instanceId);
            window.Show();
            _logWindows[instanceId] = window;
            window.Closed += (_, _) => _logWindows.Remove(instanceId);
            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }

    /// <inheritdoc />
    public Task CloseInstanceLogsAsync(string instanceId)
    {
        try
        {
            if (_mainWindow == null)
                return Task.CompletedTask;

            var window = _logWindows.GetValueOrDefault(instanceId);
            window?.Close();
            if (window != null)
                _logWindows.Remove(instanceId);
            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }

    /// <inheritdoc />
    public async Task ShowInstanceExportAsync(Instance instance, EInstanceProvider provider)
    {
        if (_mainWindow == null)
            return;

        var exportWindow = _windowFactory.CreateExportWindow(instance, provider);
        await exportWindow.ShowDialog(_mainWindow);
    }

    /// <inheritdoc />
    public async Task<bool> ShowInstanceVersionSelectorAsync(Instance instance)
    {
        if (_mainWindow == null)
            return false;
        var selectorWindow = _windowFactory.CreateInstanceVersionSelectorWindow(instance);
        return await selectorWindow.ShowDialog<bool>(_mainWindow);
    }

    /// <inheritdoc />
    public async Task<bool?> ShowResourceDownloaderAsync(Instance instance, EResourceType type)
    {
        if (_mainWindow == null)
            return false;
        var dialog =
            _windowFactory.CreateResourceDownloadWindow(instance, type);
        return await dialog.ShowDialog<bool?>(_mainWindow);
    }

    /// <inheritdoc />
    public async Task<bool> ShowResourceReviewAsync(Instance instance, EResourceType type,
        List<ResourceDownloadModel> resources)
    {
        if (_mainWindow == null)
            return false;
        var dialog =
            _windowFactory.CreateResourceReviewWindow(instance, type, resources);
        return await dialog.ShowDialog<bool>(_mainWindow);
    }
}
