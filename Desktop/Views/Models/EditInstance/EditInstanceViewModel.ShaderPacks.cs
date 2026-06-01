using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Serialization;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Desktop.Helpers;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.EditInstance;

public partial class EditInstanceViewModel_ShaderPacks  : KonkordObservableObject
{
    private readonly ICustomLogger _logger;
    private readonly EditInstanceViewModel _parent;
    
    private readonly SourceCache<ResourceBaseModel, string> _shaderPackCache = new(x => x.Name);
    public ReadOnlyObservableCollection<ResourceBaseModel> FilteredShaderPacks { get; private set; }
    [ObservableProperty]
    public partial ResourceBaseModel? SelectedShaderPack { get; set; }
    [ObservableProperty] 
    public partial string SearchQuery { get; set; } = string.Empty;
    
    public EditInstanceViewModel_ShaderPacks(EditInstanceViewModel parent)
    {
        _parent = parent;
        if (Design.IsDesignMode)
            return;
        
        var services = Program.ServiceProvider;
        _logger = services.GetRequiredService<ICustomLogger<EditInstanceViewModel_ShaderPacks>>();
        
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
        var subscription =  _shaderPackCache.Connect()
            .Filter(filter)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Bind(out var shaders)
            .Subscribe();

        Disposables.Add(subscription);
        FilteredShaderPacks = shaders;
    }
    
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        foreach (var shaderPack in _shaderPackCache.Items)
            shaderPack.Icon?.Dispose();
        _shaderPackCache.Clear();
        _shaderPackCache.Dispose();
        SelectedShaderPack = null;
    }

    public async Task InitAsync(CancellationToken cancellationToken = default)
    {
        if (_parent.IsVanilla)
            return;
        
        await Dispatcher.UIThread.Invoke(async () => await RefreshShaderPacksAsync());
    }

    #region Commands

    /// <summary>
    /// Toggles the enabled state of a shader pack and saves the updated state.
    /// </summary>
    /// <param name="shader">The shader pack to toggle.</param>
    [RelayCommand]
    private void Toggle(ResourceBaseModel shader)
    {
        shader.IsEnabled = !shader.IsEnabled;
        SaveShaderPacks();
    }
    
    /// <summary>
    /// Removes a shader pack file from the file system and refreshes the shader pack list.
    /// </summary>
    /// <param name="shader">The shader pack to remove.</param>
    [RelayCommand]
    private async Task Remove(ResourceBaseModel shader)
    {
        if (!File.Exists(shader.FilePath))
            return;

        File.Delete(shader.FilePath);
        await RefreshShaderPacksAsync();
    }

    [RelayCommand]
    private async Task Download() => await _parent.OpenResourceDownloadDialog.Handle(EResourceType.SHADER_PACK);

    /// <summary>
    /// Opens the directory containing the shader packs in the file explorer.
    /// </summary>
    [RelayCommand]
    private void OpenDir()
    {
        if (_parent.GameDirectory == null)
            return;
        
        string shaderPacksDir = Path.Combine(_parent.GameDirectory, "shaderpacks");
        if (!Directory.Exists(shaderPacksDir))
            return;

        FileSystemHelper.OpenFolderInFileExplorer(shaderPacksDir);
    }

    #endregion
    
    /// <summary>
    /// Saves the current state of shader packs by renaming their file extensions
    /// based on their enabled or disabled status. Enabled shader packs have the
    /// `.zip` extension, while disabled ones have `.zip.dis`.
    /// </summary>
    public void SaveShaderPacks()
    {
        _logger.LogDebug("Saving shader packs...");
        if (_parent.GameDirectory == null)
            return;

        string shaderPacksDir = Path.Combine(_parent.GameDirectory, "shaderpacks");
        if (!Directory.Exists(shaderPacksDir))
            return;

        foreach (var shaderPack in _shaderPackCache.Items)
        {
            string? newPath = null;
            if (shaderPack.FilePath == null)
                continue;
            
            if (shaderPack.IsEnabled && shaderPack.FilePath.EndsWith(".zip.dis"))
                newPath = shaderPack.FilePath.Replace(".dis", "");
            else if (!shaderPack.IsEnabled && shaderPack.FilePath.EndsWith(".zip"))
                newPath = shaderPack.FilePath.Replace(".zip", ".zip.dis");

            if (newPath == null)
                continue;

            if (File.Exists(newPath))
            {
                _logger.LogWarning("Skipping save... Shader pack file already exists: " + newPath);
                continue;
            }

            File.Move(shaderPack.FilePath, newPath);
            shaderPack.FilePath = newPath;
        }
    }
    
    /// <summary>
    /// Refreshes the list of shader packs by scanning the game directory for shader pack files.
    /// Updates the `_shaderPackCache` with metadata such as name, size, and enabled status for each shader pack.
    /// </summary>
    public async Task RefreshShaderPacksAsync()
    {
        if (_parent.GameDirectory == null)
            return;

        string shaderPacksDir = Path.Combine(_parent.GameDirectory, "shaderpacks");
        Directory.CreateDirectory(shaderPacksDir);
        
        var configPath = _parent.Instance.getInstance().GetResourceConfigPath();
        List<InstanceResource> instanceResources = [];
        if (File.Exists(configPath))
        {
            var localResources = await JsonHelper.ReadJsonFileAsync<List<InstanceResource>>(configPath);
            if (localResources != null)
                instanceResources = localResources;
        }

        _shaderPackCache.Edit(innerCache =>
        {
            foreach (var shader in innerCache.Items)
                shader.Icon?.Dispose(); // Dispose of the image to free memory
            
            innerCache.Clear();
            var packs = Directory.GetFiles(shaderPacksDir, "*")
                .Where(x => x.EndsWith(".zip") || x.EndsWith(".zip.dis"));
            
            foreach (var pack in packs)
            {
                try
                {
                    string fileName = Path.GetFileName(pack);
                    string resourceName = fileName
                        .Replace(".zip.dis", "")
                        .Replace(".zip", "");
                    var size = new FileInfo(pack).Length;
                    
                    var instanceResource = instanceResources.FirstOrDefault(x => x.Type == EResourceType.SHADER_PACK &&
                        x.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase));
                    
                    Bitmap? icon = null;
                    try
                    {
                        if (instanceResource is { IconPath: not null })
                            icon = ImageHelper.LoadFromResource(new Uri(instanceResource.IconPath));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to read icon from {pack}:");
                    }

                    icon ??= ImageHelper.LoadFromResource(new Uri("avares://Desktop/Assets/Images/default_world.png"));
                    
                    var newResourcePack = new ResourceBaseModel
                    {
                        IsEnabled = !fileName.EndsWith(".dis"),
                        Name = resourceName,
                        Icon = icon,
                        FileSize = size,
                        FilePath = pack,
                        IsInstalled = true,
                        Platform = instanceResource?.Platform,
                        ProjectId = instanceResource?.ProjectId,
                    };
                    innerCache.AddOrUpdate(newResourcePack);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, $"Failed to load shader pack {pack}:");
                }
            }
        });
    }
}