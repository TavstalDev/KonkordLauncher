using System;
using System.Collections.Generic;
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
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Json;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Serialization;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.EditInstance;

public partial class EditInstanceViewModel_Mods  : KonkordObservableObject
{
    private readonly ICustomLogger _logger = null!;
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IBitmapService _bitmapService = null!;
    private readonly EditInstanceViewModel _parent;
    
    private readonly SourceCache<ResourceBaseModel, string> _modsCache = new(x => x.Name);
    public ReadOnlyObservableCollection<ResourceBaseModel> FilteredMods { get; private set; } = null!;

    [ObservableProperty]
    public partial ResourceBaseModel? SelectedMod { get; set; }
    [ObservableProperty] 
    public partial string? SearchQuery { get; set; } = string.Empty;
    
    [RequiresUnreferencedCode( "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public EditInstanceViewModel_Mods(EditInstanceViewModel parent)
    {
        _parent = parent;
        if (Design.IsDesignMode)
            return;
        
        var services = Program.ServiceProvider;
        _logger = services.GetRequiredService<ICustomLogger<EditInstanceViewModel_Mods>>();
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
        var subscription = _modsCache.Connect()
            .Filter(filter)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Bind(out var mods)
            .Subscribe();

        Disposables.Add(subscription);
        FilteredMods = mods;
    }
    
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
        if (_parent.IsVanilla)
            return;
        
        await Dispatcher.UIThread.Invoke(async () => await RefreshModsAsync());
    }
    
    #region Commands
    
    /// <summary>
    /// Toggles the enabled state of a mod and saves the updated state.
    /// </summary>
    /// <param name="mod">The mod to toggle.</param>
    [RelayCommand]
    public void Toggle(ResourceBaseModel mod)
    {
        mod.IsEnabled = !mod.IsEnabled;
        SaveMods();
    }

    [RelayCommand]
    public void CheckUpdate(ResourceBaseModel mod)
    {
        // TODO: Implement mod update check logic
    }

    [RelayCommand]
    public void ChangeVersion(ResourceBaseModel mod)
    {
        // TODO: Implement mod version change logic
    }

    /// <summary>
    /// Removes the specified mod file from the file system and refreshes the mod list.
    /// </summary>
    /// <param name="mod">The mod to remove.</param>
    [RelayCommand]
    public async Task Remove(ResourceBaseModel mod)
    {
        if (!File.Exists(mod.FilePath))
            return;

        if (!FileSystemHelper.DeleteFile(mod.FilePath))
        {
            _logger.LogError("Failed to delete resource pack file: " + mod.FilePath);
            return;
        }

        var instance = _parent.Instance.getInstance();
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
    private async Task Download() => await _parent.OpenResourceDownloadDialog.Handle(EResourceType.MOD);

    /// <summary>
    /// Opens the directory containing the mods in the file explorer.
    /// </summary>
    [RelayCommand]
    public void ModOpenDirectory()
    {
        if (_parent.GameDirectory == null)
            return;
        
        string modsDir = Path.Combine(_parent.GameDirectory, "mods");
        if (!Directory.Exists(modsDir))
            return;

        FileSystemHelper.OpenFolderInFileExplorer(modsDir);
    }
    #endregion
    
    /// <summary>
    /// Saves the current state of mods by renaming their file extensions
    /// based on their enabled or disabled status. Enabled mods have the
    /// `.jar` extension, while disabled ones have `.jar.dis`.
    /// </summary>
    public void SaveMods()
    {
        _logger.LogDebug("Saving mods...");
        if (_parent.GameDirectory == null)
            return;

        string modsDir = Path.Combine(_parent.GameDirectory, "mods");
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
    
    /// <summary>
    /// Refreshes the list of mods by scanning the game directory for mod files.
    /// Updates the `_modsCache` with metadata such as name, size, and enabled status for each mod.
    /// </summary>
    public async Task RefreshModsAsync()
    {
        if (_parent.GameDirectory == null)
            return;

        string modsDir = Path.Combine(_parent.GameDirectory, "mods");
        Directory.CreateDirectory(modsDir);

        List<InstanceResource> instanceResources = await _parent.Instance.getInstance().GetInstanceResourcesAsync() ?? [];
        
        _modsCache.Edit(innerCache =>
        {
            foreach (var mod in innerCache.Items)
                mod.Icon.Dispose(_bitmapService); // Dispose of the image to free memory
            
            innerCache.Clear();
            var mods = Directory.GetFiles(modsDir, "*")
                .Where(x => x.EndsWith(".jar") || x.EndsWith(".jar.dis"));
            
            foreach (var mod in mods)
            {
                try
                {
                    string fileName = Path.GetFileName(mod);
                    string resourceName = fileName
                        .Replace(".jar.dis", "")
                        .Replace(".jar", "");
                    var size = new FileInfo(mod).Length;

                    var instanceResource = instanceResources.FirstOrDefault(x => x.Type == EResourceType.MOD &&
                        x.Path.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

                    BitmapEntry icon = new BitmapEntry(null, null);
                    try
                    {
                        if (instanceResource is { IconPath: not null })
                            icon = _bitmapService.GetBitmap(instanceResource.IconPath);
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
            }
        });
    }
}