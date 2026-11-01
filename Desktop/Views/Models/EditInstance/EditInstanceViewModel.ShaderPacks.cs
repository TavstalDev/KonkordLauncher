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

/// <summary>
///     Handles the shader packs tab of the instance editor.
/// </summary>
public partial class EditInstanceViewModel_ShaderPacks : KonkordObservableObject
{
    private readonly IBitmapService _bitmapService = null!;
    private readonly IDialogService _dialogService = null!;
    private readonly Func<string?> _getGameDirectory = () => null;
    private readonly InstanceModel _instance = null!;
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IBeaconLogger _logger = null!;

    private readonly SourceCache<ResourceBaseModel, string> _shaderPackCache = new(x => x.Name);

    /// <summary>
    ///     Creates the view-model and sets up the shader pack filter.
    /// </summary>
    public EditInstanceViewModel_ShaderPacks()
    {
        FilteredShaderPacks =
            new ReadOnlyObservableCollection<ResourceBaseModel>([]);

        if (Design.IsDesignMode)
            return;

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
        var subscription = _shaderPackCache.Connect()
            .Filter(filter)
            .ObserveOnMainThread()
            .Bind(out var shaders)
            .Subscribe();

        Disposables.Add(subscription);
        FilteredShaderPacks = shaders;
    }

    /// <summary>
    ///     Creates the view-model for one instance.
    /// </summary>
    /// <param name="instance">The instance being edited.</param>
    /// <param name="getGameDirectory">Returns the game directory of the instance.</param>
    /// <param name="launcherStore">The store used to save instance data.</param>
    /// <param name="bitmapService">The service used to load and release icons.</param>
    /// <param name="dialogService">The service used to show dialogs.</param>
    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public EditInstanceViewModel_ShaderPacks(InstanceModel instance, Func<string?> getGameDirectory,
        ILauncherStore launcherStore,
        IBitmapService bitmapService, IDialogService dialogService) : this()
    {
        if (Design.IsDesignMode)
            return;

        _instance = instance;
        _getGameDirectory = getGameDirectory;
        _logger = Program.Services.GetRequiredService<IBeaconLogger<EditInstanceViewModel_ShaderPacks>>();
        _launcherStore = launcherStore;
        _bitmapService = bitmapService;
        _dialogService = dialogService;
    }

    /// <summary>
    ///     Gets the shader packs that match the current search text.
    /// </summary>
    public ReadOnlyObservableCollection<ResourceBaseModel> FilteredShaderPacks { get; private set; }

    /// <summary>
    ///     Gets or sets the selected shader pack.
    /// </summary>
    [ObservableProperty] public partial ResourceBaseModel? SelectedShaderPack { get; set; }

    /// <summary>
    ///     Gets or sets the text used to filter the shader packs.
    /// </summary>
    [ObservableProperty] public partial string SearchQuery { get; set; } = string.Empty;

    /// <summary>
    ///     Raised after the shader pack list changes, with the resource type that changed.
    /// </summary>
    public event EventHandler<EResourceType>? OnResourcesUpdated;

    /// <summary>
    ///     Releases the shader pack icons and clears the cache.
    /// </summary>
    /// <param name="disposing">True when the object is disposed by code.</param>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        foreach (var shaderPack in _shaderPackCache.Items)
            shaderPack.Icon.Dispose(_bitmapService);
        _shaderPackCache.Clear();
        _shaderPackCache.Dispose();
        SelectedShaderPack = null;
    }

    /// <summary>
    ///     Loads the shader packs of the instance.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that completes when the shader packs are loaded.</returns>
    public async Task InitAsync(CancellationToken cancellationToken = default)
    {
        if (!_instance.IsModded)
            return;
        await Dispatcher.UIThread.Invoke(async () => await RefreshShaderPacksAsync());
    }

    /// <summary>
    ///     Renames shader pack files on disk to match their enabled state.
    /// </summary>
    public void SaveShaderPacks()
    {
        _logger.LogDebug("Saving shader packs...");
        var gameDir = _getGameDirectory.Invoke();
        if (string.IsNullOrEmpty(gameDir))
            return;

        var shaderPacksDir = Path.Combine(gameDir, "shaderpacks");
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
    ///     Reads the shaderpacks folder again and rebuilds the list.
    /// </summary>
    /// <returns>A task that completes when the list is rebuilt.</returns>
    public async Task RefreshShaderPacksAsync()
    {
        var gameDir = _getGameDirectory.Invoke();
        if (string.IsNullOrEmpty(gameDir))
            return;

        var shaderPacksDir = Path.Combine(gameDir, "shaderpacks");
        Directory.CreateDirectory(shaderPacksDir);

        var instanceResources = await _instance.getInstance().GetInstanceResourcesAsync() ?? [];

        _shaderPackCache.Edit(innerCache =>
        {
            foreach (var shader in innerCache.Items)
                shader.Icon.Dispose(_bitmapService); // Dispose of the image to free memory

            innerCache.Clear();
            var packs = Directory.GetFiles(shaderPacksDir, "*")
                .Where(x => x.EndsWith(".zip") || x.EndsWith(".zip.dis"));

            foreach (var pack in packs)
                try
                {
                    var fileName = Path.GetFileName(pack);
                    var resourceName = fileName
                        .Replace(".zip.dis", "")
                        .Replace(".zip", "");
                    var size = new FileInfo(pack).Length;

                    var instanceResource = instanceResources.FirstOrDefault(x => x.Type == EResourceType.SHADER_PACK &&
                        x.Path.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

                    var icon = new BitmapEntry(null, null);
                    try
                    {
                        if (instanceResource is { IconPath: not null })
                            icon = _bitmapService.GetBitmap(instanceResource.IconPath, BitmapSizes.ShaderPackIcon);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to read icon from {pack}:");
                    }

                    if (icon.Key == null)
                        icon = _bitmapService.GetBitmap(
                            "avares://KonkordLauncher/Assets/Images/default_world.png", BitmapSizes.DefaultWorldIcon);

                    var newResourcePack = new ResourceBaseModel
                    {
                        IsEnabled = !fileName.EndsWith(".dis"),
                        Name = instanceResource?.Name ?? resourceName,
                        Icon = icon,
                        FileSize = size,
                        FilePath = pack,
                        IsInstalled = true,
                        Type = instanceResource?.Type ?? EResourceType.SHADER_PACK,
                        Platform = instanceResource?.Platform,
                        ProjectId = instanceResource?.ProjectId,
                        SelectedVersionId = instanceResource?.VersionId
                    };
                    innerCache.AddOrUpdate(newResourcePack);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, $"Failed to load shader pack {pack}:");
                }
        });
    }

    #region Commands

    [RelayCommand]
    private void Toggle(ResourceBaseModel shader)
    {
        shader.IsEnabled = !shader.IsEnabled;
        SaveShaderPacks();
    }

    [RelayCommand]
    private async Task Remove(ResourceBaseModel shader)
    {
        if (!File.Exists(shader.FilePath))
            return;

        if (!FileSystemHelper.DeleteFile(shader.FilePath))
        {
            _logger.LogError("Failed to delete resource pack file: " + shader.FilePath);
            return;
        }

        var instance = _instance.getInstance();
        var resources = await instance.GetInstanceResourcesAsync();
        var targetResource =
            resources?.FirstOrDefault(x => x.Name == shader.Name && x.Type == EResourceType.SHADER_PACK);
        if (targetResource != null && resources != null)
        {
            resources.Remove(targetResource);
            await _launcherStore.SaveInstanceResourcesAsync(instance, resources);
        }

        await RefreshShaderPacksAsync();
    }

    [RelayCommand]
    private async Task Download()
    {
        var result =
            await _dialogService.ShowResourceDownloaderAsync(_instance.getInstance(), EResourceType.SHADER_PACK);
        if (result == null)
            return;
        if (result.Value)
            OnResourcesUpdated?.Invoke(this, EResourceType.SHADER_PACK);
    }

    [RelayCommand]
    private void OpenDir()
    {
        var gameDir = _getGameDirectory.Invoke();
        if (string.IsNullOrEmpty(gameDir))
            return;

        var shaderPacksDir = Path.Combine(gameDir, "shaderpacks");
        if (!Directory.Exists(shaderPacksDir))
            return;

        FileSystemHelper.OpenFolderInFileExplorer(shaderPacksDir);
    }

    #endregion
}
