using System.Collections.Concurrent;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Common.Services.Implementations;

/// <inheritdoc/>
public class BitmapService : IBitmapService
{
    private readonly IHttpService _httpService;
    private readonly ConcurrentDictionary<string, Bitmap> _cache = new();
    private readonly ConcurrentDictionary<string, int> _refCounts = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
    
    public BitmapService(IHttpService httpService)
    {
        _httpService = httpService;
    }
    
    /// <inheritdoc/>
    public BitmapEntry GetBitmap(string path)
    {
        if (_cache.TryGetValue(path, out var cached))
        {
            _refCounts.AddOrUpdate(path, 1, (_, count) => count + 1);
            return new BitmapEntry(path, cached);
        }

        var image = LoadFromResource(path);
        _cache[path] = image;
        _refCounts.AddOrUpdate(path, 1, (_, count) => count + 1);
        return new BitmapEntry(path, image);
    }

    /// <inheritdoc/>
    public BitmapEntry GetBitmapBase64(string cacheKey, string base64Image)
    {
        if (_cache.TryGetValue(cacheKey, out var cached))
        {
            _refCounts.AddOrUpdate(cacheKey, 1, (_, count) => count + 1);
            return new BitmapEntry(cacheKey,cached);
        }
        
        var bytes = Convert.FromBase64String(base64Image);
        using var stream = new MemoryStream(bytes);
        var image = new Bitmap(stream);
        _cache[cacheKey] = image;
        _refCounts.AddOrUpdate(cacheKey, 1, (_, count) => count + 1);
        return new BitmapEntry(cacheKey, image);
    }

    /// <inheritdoc/>
    public async Task<BitmapEntry> GetBitmapAsync(string uri)
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
            if (uri.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || uri.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                image = await LoadFromWebAsync(uri);
            else 
                image = LoadFromResource(uri);
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

    /// <inheritdoc/>
    public void Release(string key)
    {
        if (_cache.TryGetValue(key, out var bitmap))
        {
            if (_refCounts.AddOrUpdate(key, 0, (_, count) => count > 0 ? count - 1 : 0) == 0)
            {
                bitmap.Dispose();
                _cache.TryRemove(key, out _);
                _refCounts.TryRemove(key, out _);
            }
        }
    }

    private async Task<Bitmap> LoadFromWebAsync(string uri) 
    {
        var response = await _httpService.GetAsync(uri);
        if (response == null)
            throw new Exception($"Failed to load image from web: {uri}");
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync();
        return new Bitmap(stream);
    }
    
    private Bitmap LoadFromResource(string filePath) 
    {
        if (filePath.StartsWith("avares://", StringComparison.OrdinalIgnoreCase))
        {
            using var stream = AssetLoader.Open(new Uri(filePath));
            return new Bitmap(stream);
        }

        using var fileStream = File.OpenRead(filePath);
        return new Bitmap(fileStream);
    }
}