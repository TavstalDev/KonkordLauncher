using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.Extensions.Hosting;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;

namespace Tavstal.KonkordLauncher.Desktop.Services.Presentation;

/// <inheritdoc cref="IBitmapService" />
public class BitmapService : IHostedService, IBitmapService
{
    private readonly IBeaconLogger _logger;
    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<string, Bitmap> _cache = [];
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = [];
    private readonly ConcurrentDictionary<string, int> _refCounts = [];

    public BitmapService(IBeaconLogger<BitmapService> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("default");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("The service has started successfully.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Stopping the service...");
        _httpClient.Dispose();
        foreach (var bitmap in _cache.Values)
        {
            try
            {
                bitmap.Dispose();
            }
            catch
            {
                // Ignore exceptions during disposal
            }
        }
        _cache.Clear();
        foreach (var loc in _locks.Values)
        {
            try
            {
                loc.Dispose();
            }
            catch
            {
                // Ignore exceptions during disposal
            }
        }
        _locks.Clear();
        _refCounts.Clear();
        _logger.LogDebug("The service has stopped successfully.");
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public BitmapEntry GetBitmap(string path, int decodeWidth = 256)
    {
        if (_cache.TryGetValue(path, out var cached))
        {
            _refCounts.AddOrUpdate(path, 1, (_, count) => count + 1);
            return new BitmapEntry(path, cached);
        }

        var image = LoadFromResource(path, decodeWidth);
        _cache[path] = image;
        _refCounts.AddOrUpdate(path, 1, (_, count) => count + 1);
        return new BitmapEntry(path, image);
    }

    /// <inheritdoc />
    public BitmapEntry GetBitmapBase64(string cacheKey, string base64Image, int decodeWidth = 256)
    {
        if (_cache.TryGetValue(cacheKey, out var cached))
        {
            _refCounts.AddOrUpdate(cacheKey, 1, (_, count) => count + 1);
            return new BitmapEntry(cacheKey, cached);
        }

        var bytes = Convert.FromBase64String(base64Image);
        using var stream = new MemoryStream(bytes);

        using var originalBitmap = new Bitmap(stream);
        int originalWidth = originalBitmap.PixelSize.Width;
        stream.Position = 0;

        var finalBitmap = originalWidth > decodeWidth
            ?
            Bitmap.DecodeToWidth(stream, decodeWidth)
            : originalBitmap;

        _cache[cacheKey] = finalBitmap;
        _refCounts.AddOrUpdate(cacheKey, 1, (_, count) => count + 1);
        return new BitmapEntry(cacheKey, finalBitmap);
    }

    /// <inheritdoc />
    public async Task<BitmapEntry> GetBitmapAsync(string uri, int decodeWidth = 256)
    {
        if (_cache.TryGetValue(uri, out var cached))
        {
            _refCounts.AddOrUpdate(uri, 1, (_, count) => count + 1);
            return new BitmapEntry(uri, cached);
        }

        var sem = _locks.GetOrAdd(uri, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            if (_cache.TryGetValue(uri, out cached))
            {
                _refCounts.AddOrUpdate(uri, 1, (_, count) => count + 1);
                return new BitmapEntry(uri, cached);
            }

            Bitmap image;
            if (uri.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                uri.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                image = await LoadFromWebAsync(uri, decodeWidth);
            else
                image = LoadFromResource(uri, decodeWidth);
            _cache[uri] = image;
            _refCounts.AddOrUpdate(uri, 1, (_, count) => count + 1);
            return new BitmapEntry(uri, image);
        }
        finally
        {
            sem.Release();
            _locks.TryRemove(uri, out _);
        }
    }

    /// <inheritdoc />
    public void Release(string key)
    {
        if (_cache.TryGetValue(key, out var bitmap))
            if (_refCounts.AddOrUpdate(key, 0, (_, count) => count > 0 ? count - 1 : 0) == 0)
            {
                bitmap.Dispose();
                _cache.TryRemove(key, out _);
                _refCounts.TryRemove(key, out _);
            }
    }

    private async Task<Bitmap> LoadFromWebAsync(string uri, int decodeWidth)
    {
        using var response = await _httpClient.GetAsync(uri);
        if (response == null)
            throw new Exception($"Failed to load image from web: {uri}");
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync();

        using var originalBitmap = new Bitmap(stream);
        int originalWidth = originalBitmap.PixelSize.Width;
        stream.Position = 0;

        var finalBitmap = originalWidth > decodeWidth
            ?
            Bitmap.DecodeToWidth(stream, decodeWidth)
            : originalBitmap;

        return finalBitmap;
    }

    private Bitmap LoadFromResource(string filePath, int decodeWidth)
    {
        if (filePath.StartsWith("avares://", StringComparison.OrdinalIgnoreCase))
        {
            using var stream = AssetLoader.Open(new Uri(filePath));
            using var originalResBitmap = new Bitmap(stream);
            int originalResWidth = originalResBitmap.PixelSize.Width;
            stream.Position = 0;
            var finalResBitmap = originalResWidth > decodeWidth
                ?
                Bitmap.DecodeToWidth(stream, decodeWidth)
                : originalResBitmap;
            return finalResBitmap;
        }

        using var fileStream = File.OpenRead(filePath);
        using var originalBitmap = new Bitmap(fileStream);
        int originalWidth = originalBitmap.PixelSize.Width;
        fileStream.Position = 0;
        var finalBitmap = originalWidth > decodeWidth
            ?
            Bitmap.DecodeToWidth(fileStream, decodeWidth)
            : originalBitmap;
        return finalBitmap;
    }
}
