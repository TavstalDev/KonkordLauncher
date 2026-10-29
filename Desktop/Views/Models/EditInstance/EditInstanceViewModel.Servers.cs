using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NbtLib;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Constants;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.EditInstance;

public partial class EditInstanceViewModel_Servers : KonkordObservableObject
{
    private readonly IBitmapService _bitmapService = null!;
    private readonly Func<string?> _getGameDirectory = () => null;
    private readonly IBeaconLogger _logger = null!;

    public EditInstanceViewModel_Servers() { }

    public EditInstanceViewModel_Servers(Func<string?> getGameDirectory, IBitmapService bitmapService) : this()
    {
        _getGameDirectory = getGameDirectory;
        _logger = Program.Services.GetRequiredService<IBeaconLogger<EditInstanceViewModel_Servers>>();
        _bitmapService = bitmapService;
    }

    [ObservableProperty] public partial string ServerName { get; set; } = string.Empty;

    [ObservableProperty] public partial string ServerIp { get; set; } = string.Empty;

    public ObservableCollection<ServerModel> Servers { get; set; } = [];

    [ObservableProperty] public partial ServerModel? SelectedServer { get; set; }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Servers.CollectionChanged -= ServersOnCollectionChanged;
        foreach (var server in Servers)
            server.Image.Dispose(_bitmapService);
        Servers.Clear();
        SelectedServer?.Image.Dispose(_bitmapService);
        SelectedServer = null;
    }

    public Task InitAsync(CancellationToken cancellationToken = default)
    {
        RefreshServers();
        return Task.CompletedTask;
    }

    public void SaveServers()
    {
        _logger.LogDebug("Saving servers to servers.dat file...");
        var gameDir = _getGameDirectory.Invoke();
        if (string.IsNullOrEmpty(gameDir))
            return;
        try
        {
            var filePath = Path.Combine(gameDir, "servers.dat");

            var root = new NbtCompoundTag();
            var serversList = new NbtListTag(NbtTagType.Compound);

            foreach (var s in Servers)
            {
                var serverTag = new NbtCompoundTag
                {
                    { "name", new NbtStringTag(s.Name) },
                    { "ip", new NbtStringTag(s.Ip) },
                    { "acceptTextures", new NbtIntTag(s.AcceptTextures) }
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
            _logger.LogCritical(ex, "Failed to save servers to servers.dat file.");
        }
    }

    public void RefreshServers()
    {
        var gameDir = _getGameDirectory.Invoke();
        if (string.IsNullOrEmpty(gameDir))
            return;

        // Construct the file path for the servers.dat file
        var filePath = Path.Combine(gameDir, "servers.dat");
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
            // Dispose of the image to free memory
            server.Image.Dispose(_bitmapService);
        Servers.Clear();
        foreach (var server in serversDat.Servers)
        {
            var model = new ServerModel(server.Name, server.Ip, server.AcceptTextures, server.HideAddress,
                server.Icon);
            if (server.Icon != null)
                _bitmapService.GetBitmapAsync(server.Icon, BitmapSizes.ServerIcon).ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully) model.Image = t.Result;
                }, TaskScheduler.FromCurrentSynchronizationContext());
            else
                model.Image = _bitmapService.GetBitmap(
                    "avares://KonkordLauncher/Assets/Images/default_world.png", BitmapSizes.DefaultWorldIcon);
            Servers.Add(model);
        }

        Servers.CollectionChanged += ServersOnCollectionChanged;
    }

    #region Events

    private void ServersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SaveServers();
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void Add()
    {
        if (string.IsNullOrEmpty(ServerName) || string.IsNullOrEmpty(ServerIp))
            return;

        var model = new ServerModel(ServerName, ServerIp, 0, 0, null)
        {
            Image = _bitmapService.GetBitmap(
                "avares://KonkordLauncher/Assets/Images/default_world.png", BitmapSizes.DefaultWorldIcon)
        };

        Servers.Add(model);
    }

    [RelayCommand]
    private void Remove(ServerModel server)
    {
        if (Servers.Contains(server))
            Servers.Remove(server);
    }

    #endregion
}
