using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using ReactiveUI;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Helpers;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.EditInstance;

public partial class EditInstanceViewModel_ResourcePacks  : KonkordObservableObject
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(EditInstanceViewModel_ResourcePacks));
    private EditInstanceViewModel _parent;
    
    private readonly SourceCache<ResourcePackModel, Guid> _resourcePackCache = new(x => x.Id);
    public ReadOnlyObservableCollection<ResourcePackModel> FilteredResourcePacks { get; private set; }
    [ObservableProperty] private ResourcePackModel? _selectedResourcePack;
    [ObservableProperty] private string? _resourcePackSearchQuery = string.Empty;
    
    public EditInstanceViewModel_ResourcePacks(EditInstanceViewModel parent)
    {
        _parent = parent;
    }
    
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        foreach (var resourcePack in _resourcePackCache.Items)
            resourcePack.Icon?.Dispose();
        _resourcePackCache.Clear();
        _resourcePackCache.Dispose();
        SelectedResourcePack?.Icon?.Dispose();
        SelectedResourcePack = null;
    }
    
    public async Task InitAsync(CancellationToken cancellationToken = default)
    {
        // Set up a reactive filter for the ResourcePackSearchQuery property.
        // The filter updates dynamically based on the search query, matching resource packs whose names contain the query string (case-insensitive).
        var resourcePack = this.WhenAnyValue(x => x.ResourcePackSearchQuery)
            .Select(query =>
            {
                if (string.IsNullOrWhiteSpace(query))
                    return (Func<ResourcePackModel, bool>)(_ => true); // No filter
                return (Func<ResourcePackModel, bool>)(pack =>
                    pack.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
            });

        // Connect the resource pack cache to the reactive filter.
        // Apply the filter and bind the resulting filtered collection to the FilteredResourcePacks property.
        // Subscribe to changes in the cache to keep the filtered collection up-to-date.
        var resourcePackSubscription = _resourcePackCache.Connect()
            .Filter(resourcePack)
            .Bind(out var filteredResourcePacks)
            .Subscribe();

        Disposables.Add(resourcePackSubscription);

        FilteredResourcePacks = filteredResourcePacks;
        RefreshResourcePacks();
    }
    
    #region Commands

    /// <summary>
    /// Toggles the enabled state of a resource pack and saves the updated state.
    /// </summary>
    /// <param name="resourcePack">The resource pack to toggle.</param>
    [RelayCommand]
    public void Toggle(ResourcePackModel resourcePack)
    {
        resourcePack.IsEnabled = !resourcePack.IsEnabled;
        SaveResourcePacks();
    }

    /// <summary>
    /// Removes a resource pack file from the file system and refreshes the resource pack list.
    /// </summary>
    /// <param name="resourcePack">The resource pack to remove.</param>
    [RelayCommand]
    public void Remove(ResourcePackModel resourcePack)
    {
        if (!File.Exists(resourcePack.Path))
            return;

        File.Delete(resourcePack.Path);
        RefreshResourcePacks();
    }

    [RelayCommand]
    public void Download(ResourcePackModel resourcePack)
    {
        // TODO: Implement resource pack download logic
    }

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
        _logger.Debug("Saving resource packs...");
        if (_parent.GameDirectory == null)
            return;

        string resourcePacksDir = Path.Combine(_parent.GameDirectory, "resourcepacks");
        if (!Directory.Exists(resourcePacksDir))
            return;

        foreach (var resourcePack in _resourcePackCache.Items)
        {
            string? newPath = null;
            if (resourcePack.IsEnabled && resourcePack.Path.EndsWith(".zip.dis"))
                newPath = resourcePack.Path.Replace(".dis", "");
            else if (!resourcePack.IsEnabled && resourcePack.Path.EndsWith(".zip"))
                newPath = resourcePack.Path.Replace(".zip", ".zip.dis");

            if (newPath == null)
                continue;

            if (File.Exists(newPath))
            {
                _logger.Warn("Skipping save... Resource pack file already exists: " + newPath);
                continue;
            }

            File.Move(resourcePack.Path, newPath);
            resourcePack.Path = newPath;
        }
    }

    /// <summary>
    /// Refreshes the list of resource packs by scanning the game directory for resource pack files.
    /// Updates the `ResourcePacks` collection with metadata such as name, size, and icon for each resource pack.
    /// </summary>
    public void RefreshResourcePacks()
    {
        if (_parent.GameDirectory == null)
            return;

        string resourcePacksDir = Path.Combine(_parent.GameDirectory, "resourcepacks");
        if (!Directory.Exists(resourcePacksDir))
            return;

        _resourcePackCache.Edit(innerCache =>
        {
            foreach (var resourcePack in innerCache.Items)
            {
                // Dispose of the image to free memory
                resourcePack.Icon?.Dispose();
            }

            innerCache.Clear();
            var resources = Directory.GetFiles(resourcePacksDir, "*")
                .Where(x => x.EndsWith(".zip") || x.EndsWith(".zip.dis"));
            foreach (var resource in resources)
            {
                // Make sure the name does not include the extension
                var resourceName = Path.GetFileNameWithoutExtension(resource.Replace(".zip.dis", ".zip"));
                var size = File.ReadAllBytes(resource).LongLength;
                using var zipFile = ZipFile.OpenRead(resource);
                Bitmap? icon = null;

                var iconEntry = zipFile.Entries.FirstOrDefault(x =>
                    x.FullName.EndsWith("pack.png", StringComparison.OrdinalIgnoreCase));
                if (iconEntry != null)
                {
                    using var iconStream = iconEntry.Open();
                    using var memoryStream = new MemoryStream();
                    iconStream.CopyTo(memoryStream);
                    iconStream.Close();
                    icon = ImageHelper.Base64ToBitmap(Convert.ToBase64String(memoryStream.ToArray()));
                }

                icon ??= ImageHelper.LoadFromResource(new Uri("avares://Desktop/Assets/Images/default_world.png"));

                // TODO: Handle provider
                var newResourcePack = new ResourcePackModel(resource.EndsWith(".zip"), resourceName, resource, icon,
                    "unknown", size);
                innerCache.AddOrUpdate(newResourcePack);
            }
        });
    }
}