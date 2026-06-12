using System;
using System.Collections.Generic;
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
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Json;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Serialization;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Desktop.Helpers;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.EditInstance;

public partial class EditInstanceViewModel_ResourcePacks  : KonkordObservableObject
{
    private readonly ICustomLogger _logger = null!;
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IBitmapService _bitmapService = null!;
    private readonly EditInstanceViewModel _parent;
    
    private readonly SourceCache<ResourceBaseModel, string> _resourcePackCache = new(x => x.Name);
    public ReadOnlyObservableCollection<ResourceBaseModel> FilteredResourcePacks { get; } = null!;

    [ObservableProperty]
    public partial ResourceBaseModel? SelectedResourcePack { get; set; }
    [ObservableProperty] 
    public partial string SearchQuery { get; set; } = string.Empty;
    
    [RequiresUnreferencedCode( "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public EditInstanceViewModel_ResourcePacks(EditInstanceViewModel parent)
    {
        _parent = parent;
        if (Design.IsDesignMode)
            return;
        
        var services = Program.ServiceProvider;
        _logger = services.GetRequiredService<ICustomLogger<EditInstanceViewModel_ResourcePacks>>();
        _launcherStore = services.GetRequiredService<ILauncherStore>();
        _bitmapService = services.GetRequiredService<IBitmapService>();
        
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
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Bind(out var packs)
            .Subscribe();

        Disposables.Add(subscription);
        FilteredResourcePacks = packs;
    }
    
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        foreach (var resourcePack in  FilteredResourcePacks)
            resourcePack.Icon.Dispose(_bitmapService);
        _resourcePackCache.Clear();
        _resourcePackCache.Dispose();
        SelectedResourcePack?.Icon.Dispose(_bitmapService);
        SelectedResourcePack = null;
    }
    
    public async Task InitAsync(CancellationToken cancellationToken = default) => await Dispatcher.UIThread.Invoke(async () => await RefreshResourcePacksAsync());
    
    
    #region Commands

    /// <summary>
    /// Toggles the enabled state of a resource pack and saves the updated state.
    /// </summary>
    /// <param name="resourcePack">The resource pack to toggle.</param>
    [RelayCommand]
    public void Toggle(ResourceBaseModel resourcePack)
    {
        resourcePack.IsEnabled = !resourcePack.IsEnabled;
        SaveResourcePacks();
    }

    /// <summary>
    /// Removes a resource pack file from the file system and refreshes the resource pack list.
    /// </summary>
    /// <param name="resourcePack">The resource pack to remove.</param>
    [RelayCommand]
    public async Task Remove(ResourceBaseModel resourcePack)
    {
        if (!File.Exists(resourcePack.FilePath))
            return;

        if (!FileSystemHelper.DeleteFile(resourcePack.FilePath))
        {
            _logger.LogError("Failed to delete resource pack file: " + resourcePack.FilePath);
            return;
        }

        var instance = _parent.Instance.getInstance();
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
    private async Task Download() => await _parent.OpenResourceDownloadDialog.Handle(EResourceType.RESOURCE_PACK);

    /// <summary>
    /// Opens the directory containing the resource packs in the file explorer.
    /// </summary>
    [RelayCommand]
    public void OpenDir()
    {
        if (_parent.GameDirectory == null)
            return;
        
        string resourcePacksDir = Path.Combine(_parent.GameDirectory, "resourcepacks");
        Directory.CreateDirectory(resourcePacksDir);
        FileSystemHelper.OpenFolderInFileExplorer(resourcePacksDir);
    }

    #endregion

    /// <summary>
    /// Saves the current state of resource packs by renaming their file extensions
    /// based on their enabled or disabled status. Enabled resource packs have the
    /// `.zip` extension, while disabled ones have `.zip.dis`.
    /// </summary>
    public void SaveResourcePacks()
    {
        _logger.LogDebug("Saving resource packs...");
        if (_parent.GameDirectory == null)
            return;

        string resourcePacksDir = Path.Combine(_parent.GameDirectory, "resourcepacks");
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

    /// <summary>
    /// Refreshes the list of resource packs by scanning the game directory for resource pack files.
    /// Updates the `ResourcePacks` collection with metadata such as name, size, and icon for each resource pack.
    /// </summary>
    public async Task RefreshResourcePacksAsync()
    {
        if (_parent.GameDirectory == null)
            return;

        string resourcePacksDir = Path.Combine(_parent.GameDirectory, "resourcepacks");
        Directory.CreateDirectory(resourcePacksDir);
        
        List<InstanceResource> instanceResources = await _parent.Instance.getInstance().GetInstanceResourcesAsync() ?? [];

        _resourcePackCache.Edit(innerCache =>
        {
            foreach (var resourcePack in innerCache.Items)
                resourcePack.Icon.Dispose(_bitmapService); // Dispose of the image to free memory

            innerCache.Clear();
            var resources = Directory.GetFiles(resourcePacksDir, "*")
                .Where(x => x.EndsWith(".zip") || x.EndsWith(".zip.dis")).ToList();
            
            foreach (var resource in resources)
            {
                try
                {
                    string fileName = Path.GetFileName(resource);
                    string resourceName = fileName
                        .Replace(".zip.dis", "")
                        .Replace(".zip", "");
                    var size = new FileInfo(resource).Length;

                    BitmapEntry icon = new BitmapEntry(null, null);
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
                            icon.SetValue(resource, ImageHelper.Base64ToBitmap(Convert.ToBase64String(memoryStream.ToArray())));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Failed to read icon from {resource}: {ex.Message}");
                    }

                    if (icon.Key == null)
                        icon = _bitmapService.GetBitmap("avares://KonkordLauncher/Assets/Images/default_world.png");
                    

                    var instanceResource = instanceResources.FirstOrDefault(x => x.Type == EResourceType.RESOURCE_PACK &&
                        x.Path.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));
                    var newResourcePack = new ResourceBaseModel
                    {
                        IsEnabled = !fileName.EndsWith(".dis"),
                        Name = instanceResource?.Name ?? resourceName,
                        Icon = icon,
                        FileSize = size,
                        FilePath = resource,
                        IsInstalled = true,
                        Platform = instanceResource?.Platform,
                        ProjectId = instanceResource?.ProjectId,
                    };
                    innerCache.AddOrUpdate(newResourcePack);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to load resource pack from {resource}:");
                }
            }
        });
        
        _logger.LogDebug($"Cache now contains {_resourcePackCache.Count} items");
    }
}