using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.EditInstance;

public partial class EditInstanceViewModel_ShaderPacks  : KonkordObservableObject
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(EditInstanceViewModel_ShaderPacks));
    private EditInstanceViewModel _parent;
    
    private readonly SourceCache<ShaderPackModel, Guid> _shaderPackCache = new(x => x.Id);
    public ReadOnlyObservableCollection<ShaderPackModel> FilteredShaderPacks { get; private set; }
    [ObservableProperty] private ShaderPackModel? _selectedShaderPack;
    [ObservableProperty] private string? _shaderPackSearchQuery = string.Empty;
    
    public EditInstanceViewModel_ShaderPacks(EditInstanceViewModel parent)
    {
        _parent = parent;
    }
    
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        /*foreach (var shaderPack in _shaderPackCache.Items)
            shaderPack.Icon?.Dispose();*/
        _shaderPackCache.Clear();
        _shaderPackCache.Dispose();
        SelectedShaderPack = null;
    }
    
    public async Task InitAsync(CancellationToken cancellationToken = default)
    {
        if (!_parent.IsVanilla)
        {
            var shaderPacks = this.WhenAnyValue(x => x.ShaderPackSearchQuery)
                .Select(query =>
                {
                    if (string.IsNullOrWhiteSpace(query))
                        return (Func<ShaderPackModel, bool>)(_ => true); // No filter
                    return (Func<ShaderPackModel, bool>)(pack =>
                        pack.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
                });

            var shaderPackSubscription = _shaderPackCache.Connect()
                .Filter(shaderPacks)
                .Bind(out var filteredShaderPacks)
                .Subscribe();

            Disposables.Add(shaderPackSubscription);
            FilteredShaderPacks = filteredShaderPacks;

            RefreshShaderPacks();
        }
    }
    
    #region Commands

    /// <summary>
    /// Toggles the enabled state of a shader pack and saves the updated state.
    /// </summary>
    /// <param name="shader">The shader pack to toggle.</param>
    [RelayCommand]
    private void Toggle(ShaderPackModel shader)
    {
        shader.IsEnabled = !shader.IsEnabled;
        SaveShaderPacks();
    }
    
    /// <summary>
    /// Removes a shader pack file from the file system and refreshes the shader pack list.
    /// </summary>
    /// <param name="shader">The shader pack to remove.</param>
    [RelayCommand]
    private void Remove(ShaderPackModel shader)
    {
        if (!File.Exists(shader.Path))
            return;

        File.Delete(shader.Path);
        RefreshShaderPacks();
    }

    [RelayCommand]
    private async Task Download() => await _parent.OpenResourceDownloadDialog.Handle((EPlatformType.Modrinth, EResourceType.SHADER_PACK));

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
        _logger.Debug("Saving shader packs...");
        if (_parent.GameDirectory == null)
            return;

        string shaderPacksDir = Path.Combine(_parent.GameDirectory, "shaderpacks");
        if (!Directory.Exists(shaderPacksDir))
            return;

        foreach (var shaderPack in _shaderPackCache.Items)
        {
            string? newPath = null;
            if (shaderPack.IsEnabled && shaderPack.Path.EndsWith(".zip.dis"))
                newPath = shaderPack.Path.Replace(".dis", "");
            else if (!shaderPack.IsEnabled && shaderPack.Path.EndsWith(".zip"))
                newPath = shaderPack.Path.Replace(".zip", ".zip.dis");

            if (newPath == null)
                continue;

            if (File.Exists(newPath))
            {
                _logger.Warn("Skipping save... Shader pack file already exists: " + newPath);
                continue;
            }

            File.Move(shaderPack.Path, newPath);
            shaderPack.Path = newPath;
        }
    }
    
    /// <summary>
    /// Refreshes the list of shader packs by scanning the game directory for shader pack files.
    /// Updates the `_shaderPackCache` with metadata such as name, size, and enabled status for each shader pack.
    /// </summary>
    public void RefreshShaderPacks()
    {
        if (_parent.GameDirectory == null)
            return;

        string shaderPacksDir = Path.Combine(_parent.GameDirectory, "shaderpacks");
        if (!Directory.Exists(shaderPacksDir))
            return;

        _shaderPackCache.Edit(innerCache =>
        {
            innerCache.Clear();
            var packs = Directory.GetFiles(shaderPacksDir, "*")
                .Where(x => x.EndsWith(".zip") || x.EndsWith(".zip.dis"));
            foreach (var pack in packs)
            {
                // Make sure the name does not include the extension
                var packName = Path.GetFileNameWithoutExtension(pack.Replace(".zip.dis", ".zip"));
                var size = File.ReadAllBytes(pack).LongLength;
                
                // TODO: Handle provider
                var newPack = new ShaderPackModel(pack.EndsWith(".zip"), packName, pack, "unknown", size);
                innerCache.AddOrUpdate(newPack);
            }
        });
    }
}