using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NbtLib;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.EditInstance;

public partial class EditInstanceViewModel_Servers : KonkordObservableObject
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(EditInstanceViewModel_Servers));
    private readonly EditInstanceViewModel _parent;

    [ObservableProperty]
    public partial string ServerName { get; set; }

    [ObservableProperty]
    public partial string ServerIp { get; set; }
    public ObservableCollection<ServerModel> Servers { get; set; } = [];
    [ObservableProperty]
    public partial ServerModel? SelectedServer { get; set; }

    public EditInstanceViewModel_Servers(EditInstanceViewModel parent)
    {
        _parent = parent;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Servers.CollectionChanged -= ServersOnCollectionChanged;
        foreach (var server in Servers)
            server.Image?.Dispose();
        Servers.Clear();
        SelectedServer?.Image?.Dispose();
        SelectedServer = null;
    }

    public async Task InitAsync(CancellationToken cancellationToken = default)
    {
        RefreshServers();
    }
    
    #region Commands

    /// <summary>
    /// Adds a new server to the list of servers if both the server name and IP address are provided.
    /// </summary>
    [RelayCommand]
    private void Add()
    {
        if (string.IsNullOrEmpty(ServerName) || string.IsNullOrEmpty(ServerIp))
            return;

        Servers.Add(new ServerModel(ServerName, ServerIp, 0, 0, null));
    }

    /// <summary>
    /// Removes the specified server from the list of servers if it exists in the collection.
    /// </summary>
    /// <param name="server">The server to remove.</param>
    [RelayCommand]
    private void Remove(ServerModel server)
    {
        if (Servers.Contains(server))
            Servers.Remove(server);
    }
    
    #endregion
    
    /// <summary>
    /// Saves the current list of Minecraft servers to the `servers.dat` file
    /// in the game directory. The method serializes the server data into NBT format
    /// and writes it to the file.
    /// </summary>
    public void SaveServers()
    {
        _logger.Debug("Saving servers to servers.dat file...");
        if (_parent.GameDirectory == null)
            return;
        try
        {
            string filePath = Path.Combine(_parent.GameDirectory, "servers.dat");
            
            var root = new NbtCompoundTag();
            var serversList = new NbtListTag(NbtTagType.Compound);

            foreach (var s in Servers)
            {
                var serverTag = new NbtCompoundTag
                {
                    { "name", new NbtStringTag(s.Name) },
                    { "ip", new NbtStringTag(s.Ip) },
                    { "acceptTextures", new NbtIntTag(s.AcceptTextures) },
                };
                
                if (s.HideAddress.HasValue)
                    serverTag.Add("hideAddress", new NbtIntTag(s.HideAddress.Value));
                
                if (!string.IsNullOrEmpty(s.Icon))
                    serverTag.Add("icon", new NbtStringTag(s.Icon));

                serversList.Add(serverTag);
            }

            root.Add("servers", serversList);
            
            using var outputStream = new NbtWriter().CreateUncompressedNbtStream(root, "");
            using var fileStream = File.Create(filePath);
            outputStream.Seek(0, SeekOrigin.Begin);
            outputStream.CopyTo(fileStream);
        }
        catch (Exception ex)
        {
            _logger.Exc("Failed to save servers to servers.dat file.");
            _logger.Error(ex);
        }
    }

    /// <summary>
    /// Handles changes to the `Servers` collection by saving the updated list
    /// of servers to the `servers.dat` file.
    /// </summary>
    /// <param name="sender">The source of the event, typically the `Servers` collection.</param>
    /// <param name="e">The event data containing details about the collection change.</param>
    private void ServersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => SaveServers();
    
    /// <summary>
    /// Refreshes the list of Minecraft servers by reading the `servers.dat` file
    /// from the game directory and updating the Servers collection with the data.
    /// </summary>
    public void RefreshServers()
    {
        if (_parent.GameDirectory == null)
            return;
    
        // Construct the file path for the servers.dat file
        string filePath = Path.Combine(_parent.GameDirectory, "servers.dat");
        if (!File.Exists(filePath))
            return;

        // Open the servers.dat file and deserialize its content
        using var inputStream = File.OpenRead(filePath);
        var serversDat = NbtConvert.DeserializeObject<ServersDat>(inputStream);
        if (serversDat == null)
            return;
    
        Servers.CollectionChanged -= ServersOnCollectionChanged;
        
        // Clear the existing Servers collection and populate it with new data
        foreach (var server in Servers)
        {
            // Dispose of the image to free memory
            server.Image?.Dispose();
        }
        Servers.Clear();
        foreach (var server in serversDat.Servers)
            Servers.Add(new ServerModel(server.Name, server.Ip, server.AcceptTextures, server.HideAddress, server.Icon));
        
        Servers.CollectionChanged += ServersOnCollectionChanged;
    }
}