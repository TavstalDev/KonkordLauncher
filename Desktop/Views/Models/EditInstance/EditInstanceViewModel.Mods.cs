using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using Tavstal.BeaconSDK.Helpers.IO;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Constants;
using Tavstal.KonkordLauncher.Desktop.Helpers;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.EditInstance;

public partial class EditInstanceViewModel_Mods : KonkordObservableObject
{
    private readonly IBitmapService _bitmapService = null!;
    private readonly IDialogService _dialogService = null!;
    private readonly Func<string?> _getGameDirectory = () => null;
    private readonly InstanceModel _instance = null!;
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IBeaconLogger _logger = null!;

    private readonly SourceCache<ResourceBaseModel, string> _modsCache = new(x => x.Name);

    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public EditInstanceViewModel_Mods()
    {
        if (Design.IsDesignMode)
        {
            FilteredMods =
                new ReadOnlyObservableCollection<ResourceBaseModel>([]);
            return;
        }

        // Set up a reactive filter for the SearchQuery property.
        var filter = this.WhenAnyValue(x => x.SearchQuery)
            .Select(query =>
            {
                if (string.IsNullOrWhiteSpace(query))
                    return (Func<ResourceBaseModel, bool>)(_ => true); // No filter
                return (Func<ResourceBaseModel, bool>)(pack =>
                    pack.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
            });

        // Connect the cache to the reactive filter and bind results
        var subscription = _modsCache.Connect()
            .Filter(filter)
            .ObserveOnMainThread()
            .Bind(out var mods)
            .Subscribe();

        Disposables.Add(subscription);
        FilteredMods = mods;
    }

    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public EditInstanceViewModel_Mods(InstanceModel instance, Func<string?> getGameDirectory,
        ILauncherStore launcherStore,
        IBitmapService bitmapService, IDialogService dialogService) : this()
    {
        _instance = instance;
        _getGameDirectory = getGameDirectory;
        _logger = Program.Services.GetRequiredService<IBeaconLogger<EditInstanceViewModel_Mods>>();
        _launcherStore = launcherStore;
        _bitmapService = bitmapService;
        _dialogService = dialogService;
    }

    public ReadOnlyObservableCollection<ResourceBaseModel> FilteredMods { get; private set; }

    [ObservableProperty] public partial ResourceBaseModel? SelectedMod { get; set; }

    [ObservableProperty] public partial string? SearchQuery { get; set; } = string.Empty;

    public event EventHandler<EResourceType>? OnResourcesUpdated;

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        foreach (var mod in _modsCache.Items)
            mod.Icon.Dispose(_bitmapService);
        _modsCache.Clear();
        _modsCache.Dispose();
        SelectedMod = null;
    }

    public async Task InitAsync(CancellationToken cancellationToken = default)
    {
        if (!_instance.IsModded)
            return;
        await Dispatcher.UIThread.Invoke(async () => await RefreshModsAsync());
    }

    public void SaveMods()
    {
        _logger.LogDebug("Saving mods...");
        var gameDir = _getGameDirectory.Invoke();
        if (string.IsNullOrEmpty(gameDir))
            return;

        var modsDir = Path.Combine(gameDir, "mods");
        if (!Directory.Exists(modsDir))
            return;

        foreach (var mod in _modsCache.Items)
        {
            string? newPath = null;
            if (mod.FilePath == null)
                continue;

            if (mod.IsEnabled && mod.FilePath.EndsWith(".jar.dis"))
                newPath = mod.FilePath.Replace(".dis", "");
            else if (!mod.IsEnabled && mod.FilePath.EndsWith(".jar"))
                newPath = mod.FilePath.Replace(".jar", ".jar.dis");

            if (newPath == null)
                continue;

            if (File.Exists(newPath))
            {
                _logger.LogWarning("Skipping save... Mod file already exists: " + newPath);
                continue;
            }

            File.Move(mod.FilePath, newPath);
            mod.FilePath = newPath;
        }
    }

    public async Task RefreshModsAsync()
    {
        var gameDir = _getGameDirectory.Invoke();
        if (string.IsNullOrEmpty(gameDir))
            return;

        var modsDir = Path.Combine(gameDir, "mods");
        Directory.CreateDirectory(modsDir);

        var instanceResources = await _instance.getInstance().GetInstanceResourcesAsync() ?? [];

        _modsCache.Edit(innerCache =>
        {
            foreach (var mod in innerCache.Items)
                mod.Icon.Dispose(_bitmapService); // Dispose of the image to free memory

            innerCache.Clear();
            var mods = Directory.GetFiles(modsDir, "*")
                .Where(x => x.EndsWith(".jar") || x.EndsWith(".jar.dis"));

            foreach (var mod in mods)
                try
                {
                    var fileName = Path.GetFileName(mod);
                    var resourceName = fileName
                        .Replace(".jar.dis", "")
                        .Replace(".jar", "");
                    var size = new FileInfo(mod).Length;

                    var instanceResource = instanceResources.FirstOrDefault(x => x.Type == EResourceType.MOD &&
                        x.Path.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

                    var icon = new BitmapEntry(null, null);
                    try
                    {
                        if (instanceResource is { IconPath: not null })
                            icon = _bitmapService.GetBitmap(instanceResource.IconPath, BitmapSizes.ModIcon);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to read icon from {mod}:");
                    }

                    var newMod = new ResourceBaseModel
                    {
                        IsEnabled = !fileName.EndsWith(".dis"),
                        Name = instanceResource?.Name ?? resourceName,
                        Icon = icon,
                        FileSize = size,
                        FilePath = mod,
                        IsInstalled = true,
                        Type = instanceResource?.Type ?? EResourceType.MOD,
                        Platform = instanceResource?.Platform,
                        ProjectId = instanceResource?.ProjectId,
                        SelectedVersionId = instanceResource?.VersionId
                    };
                    innerCache.AddOrUpdate(newMod);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to load mod from {mod}:");
                }
        });
    }

    #region Commands

    [RelayCommand]
    private void Toggle(ResourceBaseModel mod)
    {
        mod.IsEnabled = !mod.IsEnabled;
        SaveMods();
    }

    [RelayCommand]
    private async Task Remove(ResourceBaseModel mod)
    {
        if (!File.Exists(mod.FilePath))
            return;

        if (!FileSystemHelper.DeleteFile(mod.FilePath))
        {
            _logger.LogError("Failed to delete resource pack file: " + mod.FilePath);
            return;
        }

        var instance = _instance.getInstance();
        var resources = await instance.GetInstanceResourcesAsync();
        var targetResource =
            resources?.FirstOrDefault(x => x.Name == mod.Name && x.Type == EResourceType.MOD);
        if (targetResource != null && resources != null)
        {
            resources.Remove(targetResource);
            await _launcherStore.SaveInstanceResourcesAsync(instance, resources);
        }

        await RefreshModsAsync();
    }

    [RelayCommand]
    private async Task Download()
    {
        var result = await _dialogService.ShowResourceDownloaderAsync(_instance.getInstance(), EResourceType.MOD);
        if (result == null)
            return;
        if (result.Value)
            OnResourcesUpdated?.Invoke(this, EResourceType.MOD);
    }

    [RelayCommand]
    private void ModOpenDirectory()
    {
        var gameDir = _getGameDirectory.Invoke();
        if (string.IsNullOrEmpty(gameDir))
            return;

        var modsDir = Path.Combine(gameDir, "mods");
        if (!Directory.Exists(modsDir))
            return;

        FileSystemHelper.OpenFolderInFileExplorer(modsDir);
    }

    #endregion
}
