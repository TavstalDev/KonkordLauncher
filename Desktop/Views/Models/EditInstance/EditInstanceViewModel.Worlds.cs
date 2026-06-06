using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NbtLib;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.EditInstance;

public partial class EditInstanceViewModel_Worlds  : KonkordObservableObject
{
    private readonly ICustomLogger _logger;
    private readonly IBitmapService _bitmapService;
    private readonly EditInstanceViewModel _parent;

    public ObservableCollection<WorldModel> Worlds { get; set; } = [];
    [ObservableProperty]
    public partial WorldModel? SelectedWorld { get; set; }

    public EditInstanceViewModel_Worlds(EditInstanceViewModel parent)
    {
        _parent = parent;
        if (Design.IsDesignMode)
            return;
        
        var services = Program.ServiceProvider;
        _logger = services.GetRequiredService<ICustomLogger<EditInstanceViewModel_Worlds>>();
        _bitmapService = services.GetRequiredService<IBitmapService>();
    }
    
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Worlds.CollectionChanged -= WorldsOnCollectionChanged;
        foreach (var world in Worlds)
            world.Icon.Dispose(_bitmapService);
        Worlds.Clear();
        SelectedWorld?.Icon.Dispose(_bitmapService);
        SelectedWorld = null;
    }
    
    public async Task InitAsync(CancellationToken cancellationToken = default)
    {
        RefreshWorlds();
    }
    
    #region Commands

    /// <summary>
    /// Duplicates the specified Minecraft world by creating a copy of its directory
    /// and refreshing the list of worlds.
    /// </summary>
    /// <param name="world">The world to duplicate.</param>
    [RelayCommand]
    private async Task Duplicate(WorldModel world)
    {
        await DuplicateWorldAsync(world);
        RefreshWorlds();
    }

    /// <summary>
    /// Initiates the renaming process for the specified Minecraft world by enabling
    /// edit mode in the worlds table.
    /// </summary>
    /// <param name="world">The world to rename.</param>
    [RelayCommand]
    private async Task Rename(WorldModel world) => await _parent.BeginWorldRename.Handle(Unit.Default);

    /// <summary>
    /// Deletes the specified Minecraft world by removing its directory from the file system
    /// and refreshing the list of worlds.
    /// </summary>
    /// <param name="world">The world to delete.</param>
    [RelayCommand]
    private void Delete(WorldModel world)
    {
        FileSystemHelper.DeleteDirectory(world.Path);
        RefreshWorlds();
    }

    /// <summary>
    /// Copies the seed of the specified Minecraft world to the system clipboard.
    /// </summary>
    /// <param name="world">The world whose seed is to be copied.</param>
    [RelayCommand]
    private async Task CopySeed(WorldModel world) => await _parent.SetClipboardText.Handle(world.Seed.ToString());

    /// <summary>
    /// Opens the directory of the specified Minecraft world in the file explorer.
    /// </summary>
    /// <param name="world">The world whose directory is to be opened.</param>
    [RelayCommand]
    private void OpenDir(WorldModel world)
    {
        if (!Directory.Exists(world.Path))
            return;
    
        FileSystemHelper.OpenFolderInFileExplorer(world.Path);
    }
    
    #endregion
    
    /// <summary>
    /// Duplicates a Minecraft world by creating a copy of its directory and updating its metadata.
    /// Generates a unique name for the duplicated world and updates the "LevelName" tag in the `level.dat` file.
    /// </summary>
    /// <param name="world">The world to duplicate.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task DuplicateWorldAsync(WorldModel world, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Saving worlds...");
        if (_parent.GameDirectory == null)
            return;
        
        string worldsDir = Path.Combine(_parent.GameDirectory, "saves");
        if (!Directory.Exists(worldsDir))
            return;
        
        if (!Directory.Exists(world.Path))
            return;
            
        string? newPath = null;
        string? newName = null;
        for (int i = 0; i < 100; i++)
        {
            // Generate a new path with a unique name
            newName = $"{world.Name}({i})";
            newPath = Path.Combine(worldsDir, newName);
            if (!Directory.Exists(newPath))
                break;
        }
        if (newPath == null)
        {
            _logger.LogError("Failed to generate a unique path for the duplicated world.");
            return;
        }

        try
        {
            FileSystemHelper.MoveDirectory(world.Path, newPath, true, false);
            
            string levelDatPath = Path.Combine(newPath, "level.dat");
            await using var inputStream = File.OpenRead(levelDatPath);
            await using var gzip = new GZipStream(inputStream, CompressionMode.Decompress);
            using var mem = new MemoryStream();
            await gzip.CopyToAsync(mem, cancellationToken);
            mem.Seek(0, SeekOrigin.Begin);
            var worldData = NbtConvert.ParseNbtStream(mem);
            inputStream.Close(); // Close the input stream to avoid file lock issues
            if (worldData == null)
            {
                _logger.LogError("Failed to parse level.dat for world: " + world.Name);
                return;
            }

            // TryGet should not be used here, since it can't convert the INbtTag to NbtCompoundTag
            // ReSharper disable once CanSimplifyDictionaryLookupWithTryGetValue
            if (!worldData.ContainsKey("Data"))
            {
                _logger.LogError("No 'Data' tag found in level.dat for world: " + world.Name);
                return;
            }

            var dataTag = worldData["Data"] as NbtCompoundTag;
            if (dataTag == null)
            {
                _logger.LogError("Data tag is not a compound tag in level.dat for world: " + world.Name);
                return;
            }

            // TryGet should not be used here, as we want to ensure the tag exists
            // and it will be remowed and replaced with the new name.
            // ReSharper disable once CanSimplifyDictionaryRemovingWithSingleCall
            if (!dataTag.ContainsKey("LevelName"))
            {
                _logger.LogError("No 'LevelName' tag found in level.dat for world: " + world.Name);
                return;
            }

            dataTag.Remove("LevelName"); // dataTag[] uses .Add, so the old one should be removed
            dataTag["LevelName"] = new NbtStringTag(newName);

            await using var outputStream = new NbtWriter().CreateNbtStream(worldData);
            await using var fileOutputStream = File.Create(levelDatPath);
            outputStream.Seek(0, SeekOrigin.Begin);
            await outputStream.CopyToAsync(fileOutputStream, cancellationToken);
            fileOutputStream.Close();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to duplicate world.");
        }
    }
    
    /// <summary>
    /// Saves the current list of Minecraft worlds by updating their metadata and renaming their directories
    /// if necessary. This method ensures that the world's name in the `level.dat` file matches the directory name.
    /// </summary>
    public void SaveWorlds()
    {
        _logger.LogDebug("Saving worlds...");
        if (_parent.GameDirectory == null)
            return;
        
        try
        {
            string worldsDir = Path.Combine(_parent.GameDirectory, "saves");
            if (!Directory.Exists(worldsDir))
                return;
            
            foreach (var world in Worlds)
            {
                if (!Directory.Exists(world.Path))
                    continue;
                
                string? oldName = Path.GetDirectoryName(world.Path);
                // This should be changed if other properties will be changed in the future
                if (oldName == world.Name)
                    continue;
                
                string newPath = Path.Combine(worldsDir, world.Name);
                if (Directory.Exists(newPath))
                {
                    _logger.LogWarning("World with the same name already exists, skipping save.");
                    continue;
                }
                
                string levelDatPath = Path.Combine(world.Path, "level.dat");
                using var inputStream = File.OpenRead(levelDatPath);
                using var gzip = new GZipStream(inputStream, CompressionMode.Decompress);
                using var mem = new MemoryStream();
                gzip.CopyTo(mem);
                mem.Seek(0, SeekOrigin.Begin);
                var worldData = NbtConvert.ParseNbtStream(mem);
                inputStream.Close(); // Close the input stream to avoid file lock issues
                if (worldData == null)
                {
                    _logger.LogError("Failed to parse level.dat for world: " + world.Name);
                    continue;
                }
                
                // TryGet should not be used here, since it can't convert the INbtTag to NbtCompoundTag
                // ReSharper disable once CanSimplifyDictionaryLookupWithTryGetValue
                if (!worldData.ContainsKey("Data"))
                {
                    _logger.LogError("No 'Data' tag found in level.dat for world: " + world.Name);
                    continue;
                }
                
                var dataTag = worldData["Data"] as NbtCompoundTag;
                if (dataTag == null)
                {
                    _logger.LogError("Data tag is not a compound tag in level.dat for world: " + world.Name);
                    continue;
                }
                
                // TryGet should not be used here, as we want to ensure the tag exists
                // and it will be remowed and replaced with the new name.
                // ReSharper disable once CanSimplifyDictionaryRemovingWithSingleCall
                if (!dataTag.ContainsKey("LevelName"))
                {
                    _logger.LogError("No 'LevelName' tag found in level.dat for world: " + world.Name);
                    continue;
                }
                
                dataTag.Remove("LevelName"); // dataTag[] uses .Add, so the old one should be removed
                dataTag["LevelName"] = new NbtStringTag(world.Name);
                
                using var outputStream = new NbtWriter().CreateNbtStream(worldData);
                using var fileOutputStream = File.Create(levelDatPath);
                outputStream.Seek(0, SeekOrigin.Begin);
                outputStream.CopyTo(fileOutputStream);
                fileOutputStream.Close();
                FileSystemHelper.MoveDirectory(world.Path, newPath, true);
                world.Path = newPath;
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to save worlds.");
        }
    }
    
    /// <summary>
    /// Handles changes to the `Worlds` collection by saving the updated list of worlds.
    /// This method is triggered whenever the `Worlds` collection is modified.
    /// </summary>
    /// <param name="sender">The source of the event, typically the `Worlds` collection.</param>
    /// <param name="e">The event data containing details about the collection change.</param>
    public void WorldsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => SaveWorlds();
    
    /// <summary>
    /// Refreshes the list of Minecraft worlds by scanning the game directory for saved worlds.
    /// Updates the Worlds collection with metadata such as name, game mode, last played timestamp, 
    /// seed, size, and icon for each world.
    /// </summary>
    public void RefreshWorlds()
    {
        if (_parent.GameDirectory == null)
            return;
        
        string worldsDir = Path.Combine(_parent.GameDirectory, "saves");
        if (!Directory.Exists(worldsDir))
            return;

        Worlds.CollectionChanged -= WorldsOnCollectionChanged;
        
        foreach (var world in Worlds)
        {
            // Dispose of the image to free memory
            world.Icon.Dispose(_bitmapService);
        }
        Worlds.Clear();
        var worldDirs = Directory.GetDirectories(worldsDir);
        foreach (var worldDir in worldDirs)
        {
            var worldName = "unknown";
            string levelDatPath = Path.Combine(worldDir, "level.dat");
            string gamemode = "unknown";
            long lastPlayed = 0;
            long seed = 0;
            BitmapEntry icon = new BitmapEntry(null ,null);

            if (File.Exists(levelDatPath))
            {
                using var inputStream = File.OpenRead(levelDatPath);
                using var gzip = new GZipStream(inputStream, CompressionMode.Decompress);
                using var mem = new MemoryStream();
                gzip.CopyTo(mem);
                mem.Seek(0, SeekOrigin.Begin);
                var data = NbtConvert.DeserializeObject<Level>(mem);
                inputStream.Close(); // Close the input stream to avoid file lock issues

                if (data != null)
                {
                    lastPlayed = data.Data.LastPlayed;

                    if (!string.IsNullOrEmpty(data.Data.LevelName))
                        worldName = data.Data.LevelName;

                    if (data.Data.RandomSeed != null)
                        seed = data.Data.RandomSeed.Value;
                    
                    // GameMode (int) — try "GameType" first, then "GameMode"
                    if (data.Data.GameMode != null)
                        gamemode = data.Data.GameMode switch
                        {
                            0 => "Survival",
                            1 => "Creative",
                            2 => "Adventure",
                            3 => "Spectator",
                            _ => "Unknown"
                        };
                    else if (data.Data.GameType != null)
                        gamemode = data.Data.GameType switch
                        {
                            0 => "Survival",
                            1 => "Creative",
                            2 => "Adventure",
                            3 => "Spectator",
                            _ => "Unknown"
                        };
                }
            }
            
            var files = Directory.EnumerateFiles(worldDir, "*", SearchOption.AllDirectories);
            long size = files.Sum(file => new FileInfo(file).Length);
            string iconPath = Path.Combine(worldDir, "icon.png");
            if (File.Exists(iconPath))
            {
                try
                {
                    icon = _bitmapService.GetBitmap(iconPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to load world icon for {worldName}: {ex.Message}");
                }
            }
            if (icon.Key == null)
                icon = _bitmapService.GetBitmap("avares://KonkordLauncher/Assets/Images/default_world.png");
            
            Worlds.Add(new WorldModel(worldName, worldDir, gamemode,  seed, lastPlayed, size, icon));
        }
        Worlds.CollectionChanged += WorldsOnCollectionChanged;
    }
}