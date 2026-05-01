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
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Helpers;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.EditInstance;

public partial class EditInstanceViewModel_Mods  : KonkordObservableObject
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(EditInstanceViewModel_Mods));
    private EditInstanceViewModel _parent;
    
    private readonly SourceCache<ModModel, Guid> _modsCache = new(x => x.Id);
    public ReadOnlyObservableCollection<ModModel> FilteredMods { get; private set; }
    [ObservableProperty] private ModModel? _selectedMod;
    [ObservableProperty] private string? _modSearchQuery = string.Empty;
    
    public EditInstanceViewModel_Mods(EditInstanceViewModel parent)
    {
        _parent = parent;
    }
    
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        foreach (var mod in _modsCache.Items)
            mod.Icon?.Dispose();
        _modsCache.Clear();
        _modsCache.Dispose();
        SelectedMod = null;
    }
    
    public async Task InitAsync(CancellationToken cancellationToken = default)
    {
        if (!_parent.IsVanilla)
        {
            var mods = this.WhenAnyValue(x => x.ModSearchQuery)
                .Select(query =>
                {
                    if (string.IsNullOrWhiteSpace(query))
                        return (Func<ModModel, bool>)(_ => true); // No filter
                    return (Func<ModModel, bool>)(mod =>
                        mod.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
                });

            var modSubscription = _modsCache.Connect()
                .Filter(mods)
                .Bind(out var filteredMods)
                .Subscribe();

            Disposables.Add(modSubscription);
            FilteredMods = filteredMods;

            RefreshMods();
        }
    }
    
    #region Commands
    /// <summary>
    /// Toggles the enabled state of a mod and saves the updated state.
    /// </summary>
    /// <param name="mod">The mod to toggle.</param>
    [RelayCommand]
    public void Toggle(ModModel mod)
    {
        mod.IsEnabled = !mod.IsEnabled;
        SaveMods();
    }

    [RelayCommand]
    public void CheckUpdate(ModModel mod)
    {
        // TODO: Implement mod update check logic
    }

    [RelayCommand]
    public void ChangeVersion(ModModel mod)
    {
        // TODO: Implement mod version change logic
    }

    /// <summary>
    /// Removes the specified mod file from the file system and refreshes the mod list.
    /// </summary>
    /// <param name="mod">The mod to remove.</param>
    [RelayCommand]
    public void Remove(ModModel mod)
    {
        if (!File.Exists(mod.Path))
            return;

        File.Delete(mod.Path);
        RefreshMods();
    }

    [RelayCommand]
    public void Download()
    {
        // TODO: Implement mod download logic
    }

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
    /// Refreshes the list of mods by scanning the game directory for mod files.
    /// Updates the `_modsCache` with metadata such as name, size, and enabled status for each mod.
    /// </summary>
    public void RefreshMods()
    {
        if (_parent.GameDirectory == null)
            return;

        string modsDir = Path.Combine(_parent.GameDirectory, "mods");
        if (!Directory.Exists(modsDir))
            return;

        _modsCache.Edit(innerCache =>
        {
            innerCache.Clear();
            var mods = Directory.GetFiles(modsDir, "*")
                .Where(x => x.EndsWith(".jar") || x.EndsWith(".jar.dis"));
            foreach (var mod in mods)
            {
                // Make sure the name does not include the extension
                var modName = Path.GetFileNameWithoutExtension(mod.Replace(".jar.dis", ".jar"));
                var size = File.ReadAllBytes(mod).LongLength;
                
                // TODO: Handle icon based on provider
                var icon = ImageHelper.LoadFromResource(new Uri("avares://Desktop/Assets/Images/default_world.png"));
                
                // TODO: Handle provider & version
                var newMod = new ModModel(mod.EndsWith(".jar"), modName, mod, icon, "unknown", "unknown", size);
                innerCache.AddOrUpdate(newMod);
            }
        });
    }
    
    /// <summary>
    /// Saves the current state of mods by renaming their file extensions
    /// based on their enabled or disabled status. Enabled mods have the
    /// `.jar` extension, while disabled ones have `.jar.dis`.
    /// </summary>
    public void SaveMods()
    {
        _logger.Debug("Saving mods...");
        if (_parent.GameDirectory == null)
            return;

        string modsDir = Path.Combine(_parent.GameDirectory, "mods");
        if (!Directory.Exists(modsDir))
            return;

        foreach (var mod in _modsCache.Items)
        {
            string? newPath = null;
            if (mod.IsEnabled && mod.Path.EndsWith(".jar.dis"))
                newPath = mod.Path.Replace(".dis", "");
            else if (!mod.IsEnabled && mod.Path.EndsWith(".jar"))
                newPath = mod.Path.Replace(".jar", ".jar.dis");

            if (newPath == null)
                continue;

            if (File.Exists(newPath))
            {
                _logger.Warn("Skipping save... Mod file already exists: " + newPath);
                continue;
            }

            File.Move(mod.Path, newPath);
            mod.Path = newPath;
        }
    }
}