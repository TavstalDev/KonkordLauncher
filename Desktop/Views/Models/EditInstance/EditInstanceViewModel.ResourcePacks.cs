using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
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

public partial class EditInstanceViewModel_ResourcePacks : KonkordObservableObject
{
    private readonly IBitmapService _bitmapService = null!;
    private readonly IDialogService _dialogService = null!;
    private readonly Func<string?> _getGameDirectory = () => null;
    private readonly InstanceModel _instance = null!;
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IBeaconLogger _logger = null!;

    private readonly SourceCache<ResourceBaseModel, string> _resourcePackCache = new(x => x.Name);

    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public EditInstanceViewModel_ResourcePacks()
    {
        if (Design.IsDesignMode)
        {
            FilteredResourcePacks =
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
        var subscription = _resourcePackCache.Connect()
            .Filter(filter)
            .ObserveOnMainThread()
            .Bind(out var packs)
            .Subscribe();

        Disposables.Add(subscription);
        FilteredResourcePacks = packs;
    }

    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public EditInstanceViewModel_ResourcePacks(InstanceModel instance, Func<string?> getGameDirectory,
        ILauncherStore launcherStore,
        IBitmapService bitmapService, IDialogService dialogService) : this()
    {
        if (Design.IsDesignMode)
            return;

        _instance = instance;
        _getGameDirectory = getGameDirectory;
        _logger = Program.Services.GetRequiredService<IBeaconLogger<EditInstanceViewModel_ResourcePacks>>();
        _launcherStore = launcherStore;
        _bitmapService = bitmapService;
        _dialogService = dialogService;
    }

    public ReadOnlyObservableCollection<ResourceBaseModel> FilteredResourcePacks { get; } = null!;

    [ObservableProperty] public partial ResourceBaseModel? SelectedResourcePack { get; set; }

    [ObservableProperty] public partial string SearchQuery { get; set; } = string.Empty;

    public event EventHandler<EResourceType>? OnResourcesUpdated;

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        foreach (var resourcePack in FilteredResourcePacks)
            resourcePack.Icon.Dispose(_bitmapService);
        _resourcePackCache.Clear();
        _resourcePackCache.Dispose();
        SelectedResourcePack?.Icon.Dispose(_bitmapService);
        SelectedResourcePack = null;
    }

    public async Task InitAsync(CancellationToken cancellationToken = default)
    {
        await Dispatcher.UIThread.Invoke(async () => await RefreshResourcePacksAsync());
    }

    public void SaveResourcePacks()
    {
        _logger.LogDebug("Saving resource packs...");
        var gameDir = _getGameDirectory.Invoke();
        if (string.IsNullOrEmpty(gameDir))
            return;

        var resourcePacksDir = Path.Combine(gameDir, "resourcepacks");
        if (!Directory.Exists(resourcePacksDir))
            return;

        foreach (var resourcePack in _resourcePackCache.Items)
        {
            string? newPath = null;
            if (resourcePack.FilePath == null)
                continue;

            if (resourcePack.IsEnabled && resourcePack.FilePath.EndsWith(".zip.dis"))
                newPath = resourcePack.FilePath.Replace(".dis", "");
            else if (!resourcePack.IsEnabled && resourcePack.FilePath.EndsWith(".zip"))
                newPath = resourcePack.FilePath.Replace(".zip", ".zip.dis");

            if (newPath == null)
                continue;

            if (File.Exists(newPath))
            {
                _logger.LogWarning("Skipping save... Resource pack file already exists: " + newPath);
                continue;
            }

            File.Move(resourcePack.FilePath, newPath);
            resourcePack.FilePath = newPath;
        }
    }

    public async Task RefreshResourcePacksAsync()
    {
        var gameDir = _getGameDirectory.Invoke();
        if (string.IsNullOrEmpty(gameDir))
            return;

        var resourcePacksDir = Path.Combine(gameDir, "resourcepacks");
        Directory.CreateDirectory(resourcePacksDir);

        var instanceResources = await _instance.getInstance().GetInstanceResourcesAsync() ?? [];

        _resourcePackCache.Edit(innerCache =>
        {
            foreach (var resourcePack in innerCache.Items)
                resourcePack.Icon.Dispose(_bitmapService); // Dispose of the image to free memory

            innerCache.Clear();
            var resources = Directory.GetFiles(resourcePacksDir, "*")
                .Where(x => x.EndsWith(".zip") || x.EndsWith(".zip.dis")).ToList();

            foreach (var resource in resources)
                try
                {
                    var fileName = Path.GetFileName(resource);
                    var resourceName = fileName
                        .Replace(".zip.dis", "")
                        .Replace(".zip", "");
                    var size = new FileInfo(resource).Length;

                    var icon = new BitmapEntry(null, null);
                    try
                    {
                        using var zipFile = ZipFile.OpenRead(resource);
                        var iconEntry = zipFile.Entries.FirstOrDefault(x =>
                            x.FullName.Equals("pack.png", StringComparison.OrdinalIgnoreCase));

                        if (iconEntry != null)
                        {
                            using var iconStream = iconEntry.Open();
                            using var memoryStream = new MemoryStream();
                            iconStream.CopyTo(memoryStream);
                            icon.SetValue(resource,
                                ImageHelper.Base64ToBitmap(Convert.ToBase64String(memoryStream.ToArray())));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Failed to read icon from {resource}: {ex.Message}");
                    }

                    if (icon.Key == null)
                        icon = _bitmapService.GetBitmap(
                            "avares://KonkordLauncher/Assets/Images/default_world.png", BitmapSizes.DefaultWorldIcon);


                    var instanceResource = instanceResources.FirstOrDefault(x =>
                        x.Type == EResourceType.RESOURCE_PACK &&
                        x.Path.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));
                    var newResourcePack = new ResourceBaseModel
                    {
                        IsEnabled = !fileName.EndsWith(".dis"),
                        Name = instanceResource?.Name ?? resourceName,
                        Icon = icon,
                        FileSize = size,
                        FilePath = resource,
                        IsInstalled = true,
                        Type = instanceResource?.Type ?? EResourceType.RESOURCE_PACK,
                        Platform = instanceResource?.Platform,
                        ProjectId = instanceResource?.ProjectId,
                        SelectedVersionId = instanceResource?.VersionId
                    };
                    innerCache.AddOrUpdate(newResourcePack);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to load resource pack from {resource}:");
                }
        });

        _logger.LogDebug($"Cache now contains {_resourcePackCache.Count} items");
    }


    #region Commands

    [RelayCommand]
    private void Toggle(ResourceBaseModel resourcePack)
    {
        resourcePack.IsEnabled = !resourcePack.IsEnabled;
        SaveResourcePacks();
    }

    [RelayCommand]
    private async Task Remove(ResourceBaseModel resourcePack)
    {
        if (!File.Exists(resourcePack.FilePath))
            return;

        if (!FileSystemHelper.DeleteFile(resourcePack.FilePath))
        {
            _logger.LogError("Failed to delete resource pack file: " + resourcePack.FilePath);
            return;
        }

        var instance = _instance.getInstance();
        var resources = await instance.GetInstanceResourcesAsync();
        var targetResource =
            resources?.FirstOrDefault(x => x.Name == resourcePack.Name && x.Type == EResourceType.RESOURCE_PACK);
        if (targetResource != null && resources != null)
        {
            resources.Remove(targetResource);
            await _launcherStore.SaveInstanceResourcesAsync(instance, resources);
        }

        await RefreshResourcePacksAsync();
    }

    [RelayCommand]
    private async Task Download()
    {
        var result =
            await _dialogService.ShowResourceDownloaderAsync(_instance.getInstance(), EResourceType.RESOURCE_PACK);
        if (result == null)
            return;
        if (result.Value)
            OnResourcesUpdated?.Invoke(this, EResourceType.RESOURCE_PACK);
    }

    [RelayCommand]
    private void OpenDir()
    {
        var gameDir = _getGameDirectory.Invoke();
        if (string.IsNullOrEmpty(gameDir))
            return;

        var resourcePacksDir = Path.Combine(gameDir, "resourcepacks");
        Directory.CreateDirectory(resourcePacksDir);
        FileSystemHelper.OpenFolderInFileExplorer(resourcePacksDir);
    }

    #endregion
}
