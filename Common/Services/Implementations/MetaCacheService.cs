using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Caching.Memory;
using Modrinth.Models;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Json;
using Tavstal.KonkordLauncher.Common.Models.MetaCache;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Serialization;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using File = System.IO.File;
using Version = Modrinth.Models.Version;

namespace Tavstal.KonkordLauncher.Common.Services.Implementations;

/// <inheritdoc cref="IMetaCacheService" />
public class MetaCacheService : IMetaCacheService, IAsyncInitializable
{
    private readonly ICustomLogger _logger;
    private readonly ILauncherStore _launcherStore;
    private readonly IBitmapService _bitmapService;
    private readonly IModrinthApiClient _modrinthApiClient;
    private static readonly HttpClient _httpClient = new();
    private static readonly ConcurrentDictionary<string, MetaCache> _cache = new();
    private static readonly TimeSpan _searchCacheDuration = TimeSpan.FromHours(1);
    private static readonly TimeSpan _projectCacheDuration = TimeSpan.FromHours(24);
    private static readonly MemoryCache _fileMemoryCache = new(new MemoryCacheOptions
    {
        SizeLimit = 1024
    });
    private static readonly MemoryCacheEntryOptions _fileCacheOptions = new()
    {
        SlidingExpiration = TimeSpan.FromMinutes(30),
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2),
        Size = 1
    };
    private record CachedFileEntry(object Value, DateTime LastWriteUtc);

    /// <summary>
    /// Initializes a new instance of the <see cref="MetaCacheService"/> class.
    /// </summary>
    /// <param name="logger">Logger used to record initialization failures and other diagnostics.</param>
    /// <param name="launcherStore">Launcher store used to resolve cache directory settings.</param>
    /// <param name="bitmapService">Bitmap service used to decode and cache images fetched from the web.</param>
    /// <param name="modrinthApiClient">API client used to fetch data from Modrinth when cache misses occur.</param>
    public MetaCacheService(ICustomLogger<MetaCacheService> logger, ILauncherStore launcherStore, IBitmapService bitmapService, IModrinthApiClient modrinthApiClient)
    {
        _logger = logger;
        _launcherStore = launcherStore;
        _bitmapService = bitmapService;
        _modrinthApiClient = modrinthApiClient;
    }
    
    /// <inheritdoc/>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(PathHelper.MetaCachePath))
                return;

            var result = await JsonHelper.ReadJsonFileAsync<List<MetaCache>>(PathHelper.MetaCachePath, CommonJsonContex.Default.ListMetaCache);
            if (result != null)
                foreach (var metaCache in result)
                {
                    if (!File.Exists(metaCache.Path) || !metaCache.IsValid())
                        continue;
                    
                    _cache.TryAdd(metaCache.Id, metaCache);
                    switch (metaCache.Type)
                    {
                        case EMetaCacheType.SEARCH_RESULT:
                        {
                            await ReadCachedFileAsync<SearchResponse>(metaCache.Path, ModrinthJsonContext.Default.SearchResponse);
                            break;
                        }
                        case EMetaCacheType.PROJECT:
                        {
                            await ReadCachedFileAsync<Project>(metaCache.Path, ModrinthJsonContext.Default.Project);
                            break;
                        }
                        case EMetaCacheType.VERSION:
                        {
                            await ReadCachedFileAsync<Version>(metaCache.Path, ModrinthJsonContext.Default.Version);
                            break;
                        }
                    }
                }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to initialize meta cache:");
        }
    }

    /// <inheritdoc/>
    public async Task<BitmapEntry?> GetImageAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_cache.TryGetValue(imageUrl, out var cached) && cached.IsValid())
                return await _bitmapService.GetBitmapAsync(cached.Path);
            
            var response = await _httpClient.GetAsync(imageUrl, cancellationToken);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            
            var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);
            var cacheDir = settings.Launcher.CacheDirectoryPath;
            Directory.CreateDirectory(cacheDir);
            string imagesDir = Path.Combine(cacheDir, "images");
            Directory.CreateDirectory(imagesDir);
            var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes(imageUrl));
            string hash = Convert.ToHexString(sha1);
            string cachedFilePath = Path.Combine(imagesDir, $"{hash}.png");
            await File.WriteAllBytesAsync(cachedFilePath, data, cancellationToken);
                
            _cache.TryAdd(imageUrl, new MetaCache
            {
                Id = imageUrl,
                Type = EMetaCacheType.IMAGE,
                Path = cachedFilePath,
                ValidUntil = DateTime.UtcNow.Add(_projectCacheDuration)
            });
            await SaveCacheAsync(cancellationToken);
            return await _bitmapService.GetBitmapAsync(cachedFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to get image in meta cache:");
            return null;
        }
    }

    /// <inheritdoc/>
    public string? GetImagePath(string imageUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_cache.TryGetValue(imageUrl, out var cached) && cached.IsValid())
                return cached.Path;
            
            var settings = _launcherStore.GetSettings() ?? throw new InvalidOperationException("Failed to resolve launcher settings while getting image path from meta cache.");
            var cacheDir = settings.Launcher.CacheDirectoryPath;
            Directory.CreateDirectory(cacheDir);
            string imagesDir = Path.Combine(cacheDir, "images");
            Directory.CreateDirectory(imagesDir);
            var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes(imageUrl));
            string hash = Convert.ToHexString(sha1);
            return Path.Combine(imagesDir, $"{hash}.png");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to generate image path in meta cache:");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<Project?> GetProjectAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes($"project:{id}"));
            string hash = Convert.ToHexString(sha1);

            if (_cache.TryGetValue(hash, out var cached) && cached.IsValid())
                return await ReadCachedFileAsync<Project>(cached.Path, ModrinthJsonContext.Default.Project);
            
            var response = await _modrinthApiClient.GetProjectAsync(id, cancellationToken);
            if (response != null)
            {
                var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);
                var cacheDir = settings.Launcher.CacheDirectoryPath;
                Directory.CreateDirectory(cacheDir);
                string projectDir = Path.Combine(cacheDir, "projects");
                Directory.CreateDirectory(projectDir);
                string cachedFilePath = Path.Combine(projectDir, $"{hash}.json");
                await JsonHelper.WriteJsonFileAsync(cachedFilePath, response, ModrinthJsonContext.Default.Project, cancellationToken);
                
                _cache.TryAdd(hash, new MetaCache
                {
                    Id = hash,
                    Type = EMetaCacheType.PROJECT,
                    Path = cachedFilePath,
                    ValidUntil = DateTime.UtcNow.Add(_projectCacheDuration)
                });
                await SaveCacheAsync(cancellationToken);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to get project in meta cache:");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<Project[]> GetProjectsAsync(List<string> ids, CancellationToken cancellationToken = default)
    {
         try
         {
             List<string> idsToFetch = [];
             List<Project> projects = [];

             foreach (var id in ids)
             {
                 var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes($"project:{id}"));
                 string hash = Convert.ToHexString(sha1);

                 if (_cache.TryGetValue(hash, out var cached) && cached.IsValid())
                 {
                     var cache = await ReadCachedFileAsync<Project>(cached.Path, ModrinthJsonContext.Default.Project);
                     if (cache != null)
                         projects.Add(cache);
                     else
                         idsToFetch.Add(id);
                 }
                 else
                     idsToFetch.Add(id);
             }
            
             var apiTask = idsToFetch.Count > 0 
                 ? _modrinthApiClient.GetProjectsAsync(idsToFetch, cancellationToken) 
                 : Task.FromResult(Array.Empty<Project>());
             
             var remaining = await apiTask;
             if (remaining.Any())
                 projects.AddRange(remaining);

             _ = Task.Run(async () =>
             {
                 var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);
                 var cacheDir = settings.Launcher.CacheDirectoryPath;
                 Directory.CreateDirectory(cacheDir);
                 string projectDir = Path.Combine(cacheDir, "projects");
                 Directory.CreateDirectory(projectDir);

                 var cacheTasks = remaining.Select(x =>
                 {
                     var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes($"project:{x.Id}"));
                     string hash = Convert.ToHexString(sha1);
                     string cachedFilePath = Path.Combine(projectDir, $"{hash}.json");

                     _cache.TryAdd(hash, new MetaCache
                     {
                         Id = hash,
                         Type = EMetaCacheType.PROJECT,
                         Path = cachedFilePath,
                         ValidUntil = DateTime.UtcNow.Add(_projectCacheDuration)
                     });

                     return JsonHelper.WriteJsonFileAsync(cachedFilePath, x, ModrinthJsonContext.Default.Project, cancellationToken);
                 });
                 await Task.WhenAll(cacheTasks);
                 
             }, cancellationToken);

             await SaveCacheAsync(cancellationToken);
             return projects.ToArray();
         }
         catch (Exception ex)
         {
             _logger.LogCritical(ex, $"Failed to get projects in meta cache:");
             return [];
         }
    }

    /// <inheritdoc/>
    public async Task<Version[]> GetVersionsAsync(List<string> ids, CancellationToken cancellationToken = default)
    {
        try
        {
            List<string> idsToFetch = [];
            List<Version> versions = [];

            foreach (var id in ids)
            {
                var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes($"version:{id}"));
                string hash = Convert.ToHexString(sha1);

                if (_cache.TryGetValue(hash, out var cached) && cached.IsValid())
                {
                    var cache = await ReadCachedFileAsync<Version>(cached.Path, ModrinthJsonContext.Default.Version);
                    if (cache != null)
                        versions.Add(cache);
                    else
                        idsToFetch.Add(id);
                }
                else
                    idsToFetch.Add(id);
            }
            
            var apiTask = idsToFetch.Any() 
                ? _modrinthApiClient.GetVersionsAsync(idsToFetch, cancellationToken) 
                : Task.FromResult(Array.Empty<Version>());
            
            var remaining = await apiTask;
            if (remaining.Any())
                versions.AddRange(remaining);
            
            _ = Task.Run(async () =>
            {
                var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);
                var cacheDir = settings.Launcher.CacheDirectoryPath;
                Directory.CreateDirectory(cacheDir);
                string projectDir = Path.Combine(cacheDir, "versions");
                Directory.CreateDirectory(projectDir);

                var cacheTasks = remaining.Select(x =>
                {
                    var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes($"version:{x.Id}"));
                    string hash = Convert.ToHexString(sha1);
                    string cachedFilePath = Path.Combine(projectDir, $"{hash}.json");

                    _cache.TryAdd(hash, new MetaCache
                    {
                        Id = hash,
                        Type = EMetaCacheType.VERSION,
                        Path = cachedFilePath,
                        ValidUntil = DateTime.UtcNow.Add(_projectCacheDuration)
                    });

                    return JsonHelper.WriteJsonFileAsync(cachedFilePath, x, ModrinthJsonContext.Default.Version, cancellationToken);
                });
                await Task.WhenAll(cacheTasks);
                await SaveCacheAsync(
                    cancellationToken);
            }, cancellationToken);

            return versions.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to get versions in meta cache:");
            return [];
        }
    }

    /// <inheritdoc/>
    public async Task<SearchResponse?> SearchModpacksAsync(string? query = null, string? version = null, List<string>? categories = null, int offset = 0,
        CancellationToken cancellationToken = default)
    {
       try
       {
           string hash = BuildQueryHash("modpack", query, version, categories, offset);
           
           if (_cache.TryGetValue(hash, out var cached) && cached.IsValid())
               return await ReadCachedFileAsync<SearchResponse>(cached.Path, ModrinthJsonContext.Default.SearchResponse);
            
           var response = await _modrinthApiClient.SearchModpacksAsync(query, version, categories, offset, cancellationToken);
           return await HandleSearchResponse(hash, response, cancellationToken);
       }
       catch (Exception ex)
       {
           _logger.LogCritical(ex, $"Failed to search modpacks in meta cache:");
           return null;
       }
    }

    /// <inheritdoc/>
    public async Task<SearchResponse?> SearchModsAsync(string? query = null, string? version = null, List<string>? categories = null, int offset = 0,
        CancellationToken cancellationToken = default)
    {
       try
       {
           string hash = BuildQueryHash("mod", query, version, categories, offset);
            
           if (_cache.TryGetValue(hash, out var cached) && cached.IsValid())
               return await ReadCachedFileAsync<SearchResponse>(cached.Path, ModrinthJsonContext.Default.SearchResponse);
            
           var response = await _modrinthApiClient.SearchModsAsync(query, version, categories, offset, cancellationToken);
           return await HandleSearchResponse(hash, response, cancellationToken);
       }
       catch (Exception ex)
       {
           _logger.LogCritical(ex, $"Failed to search mods in meta cache:");
           return null;
       }
    }

    /// <inheritdoc/>
    public async Task<SearchResponse?> SearchResourcePacksAsync(string? query = null, string? version = null, List<string>? categories = null, int offset = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string hash = BuildQueryHash("resource_pack", query, version, categories, offset);
            
            if (_cache.TryGetValue(hash, out var cached) && cached.IsValid())
                return await ReadCachedFileAsync<SearchResponse>(cached.Path, ModrinthJsonContext.Default.SearchResponse);
            
            var response = await _modrinthApiClient.SearchResourcePackAsync(query, version, categories, offset, cancellationToken);
            return await HandleSearchResponse(hash, response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to search resource packs in meta cache:");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<SearchResponse?> SearchShaderPacksAsync(string? query = null, string? version = null, List<string>? categories = null, int offset = 0,
        CancellationToken cancellationToken = default)
    {
         try
         {
             string hash = BuildQueryHash("shader_pack", query, version, categories, offset);
            
             if (_cache.TryGetValue(hash, out var cached) && cached.IsValid())
                 return await ReadCachedFileAsync<SearchResponse>(cached.Path, ModrinthJsonContext.Default.SearchResponse);
            
             var response = await _modrinthApiClient.SearchShaderPacksAsync(query, version, categories, offset, cancellationToken);
             return await HandleSearchResponse(hash, response, cancellationToken);
         }
         catch (Exception ex)
         {
             _logger.LogCritical(ex, $"Failed to search shaders in meta cache:");
             return null;
         }
    }

    /// <summary>
    /// Saves a search response to the meta cache and persists the updated cache index.
    /// </summary>
    /// <param name="hash">The cache key generated for the search query.</param>
    /// <param name="response">The Modrinth search response to cache.</param>
    /// <param name="cancellationToken">Cancellation token observed while writing cache files.</param>
    /// <returns>
    /// A task that resolves to the original <paramref name="response"/> if it was cached successfully;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    private async Task<SearchResponse?> HandleSearchResponse(string hash, SearchResponse? response, CancellationToken cancellationToken = default)
    {
        if (response == null)
            return null;
        
        var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);
        var cacheDir = settings.Launcher.CacheDirectoryPath;
        Directory.CreateDirectory(cacheDir);
        string searchDir = Path.Combine(cacheDir, "searchs");
        Directory.CreateDirectory(searchDir);
        string cachedFilePath = Path.Combine(searchDir, $"{hash}.json");
        await JsonHelper.WriteJsonFileAsync(cachedFilePath, response, ModrinthJsonContext.Default.SearchResponse, cancellationToken);
                
        _cache.TryAdd(hash, new MetaCache
        {
            Id = hash,
            Type = EMetaCacheType.SEARCH_RESULT,
            Path = cachedFilePath,
            ValidUntil = DateTime.UtcNow.Add(_searchCacheDuration)
        });
        await SaveCacheAsync(cancellationToken);
        
        return response;
    }

    /// <summary>
    /// Builds a deterministic SHA-1 hash for a Modrinth search query.
    /// </summary>
    /// <param name="name">A prefix identifying the search type (for example, modpack, mod, resource_pack, or shader_pack).</param>
    /// <param name="query">Optional search text.</param>
    /// <param name="version">Optional Minecraft version filter.</param>
    /// <param name="categories">Optional category filters used by the search.</param>
    /// <param name="offset">The result offset used for paging.</param>
    /// <returns>
    /// A hexadecimal SHA-1 hash string representing the full query input.
    /// </returns>
    private string BuildQueryHash(string name, string? query = null, string? version = null, List<string>? categories = null, int offset = 0)
    {
        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine($"{name}:");
        if (!string.IsNullOrEmpty(query))
            stringBuilder.AppendLine("q=" + query);
        if (!string.IsNullOrEmpty(version))
            stringBuilder.AppendLine("v=" + version);
        if (categories is { Count: > 0 })
            stringBuilder.AppendLine("categories=" + string.Join(", ", categories));
        stringBuilder.AppendLine("offset=" + offset);
            
        var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
        string hash = Convert.ToHexString(sha1);
        return hash;
    }
    
    /// <summary>
    /// Persists the current meta cache snapshot to disk.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token observed while writing the cache file.</param>
    /// <returns>A task that completes after the cache snapshot has been written.</returns>
    private async Task SaveCacheAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheSnapshot = _cache.Values.ToArray();
            await JsonHelper.WriteJsonFileAsync(PathHelper.MetaCachePath, cacheSnapshot, CommonJsonContex.Default.MetaCacheArray, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to save meta cache:");
        }
    }
    
    /// <summary>
    /// Asynchronously reads a cached file from the specified path.
    /// </summary>
    /// <typeparam name="T">The type of the object to read from the cache.</typeparam>
    /// <param name="path">The path to the file to read.</param>
    /// <param name="typeInfo">Type information for JSON serialization and deserialization.</param>
    /// <returns>A task that represents the asynchronous operation. The result is the cached value if found, otherwise null.</returns>
    private async Task<T?> ReadCachedFileAsync<T>(string path, JsonTypeInfo<T> typeInfo)
        where T : class
    {
        if (!File.Exists(path))
            return null;

        var fullPath = Path.GetFullPath(path);
        var lastWrite = File.GetLastWriteTimeUtc(fullPath);

        if (_fileMemoryCache.TryGetValue(fullPath, out CachedFileEntry? existing))
        {
            if (existing != null && existing.LastWriteUtc == lastWrite && existing.Value is T typed)
                return typed;
        }
        
        T? value;
        try
        {
            value = await JsonHelper.ReadJsonFileAsync(fullPath, typeInfo);
            if (value != null)
            {
                var entry = new CachedFileEntry(value, lastWrite);
                var options = new MemoryCacheEntryOptions
                {
                    SlidingExpiration = _fileCacheOptions.SlidingExpiration,
                    AbsoluteExpirationRelativeToNow = _fileCacheOptions.AbsoluteExpirationRelativeToNow,
                    Size = _fileCacheOptions.Size
                };
                options.RegisterPostEvictionCallback((_, val, _, _) =>
                {
                    if (val is CachedFileEntry { Value: IDisposable d }) try { d.Dispose(); }
                        catch
                        {
                            // ignored
                        }
                });

                _fileMemoryCache.Set(fullPath, entry, options);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, $"Failed to read cached file {fullPath}; falling back to null");
            value = null;
        }

        return value;
    }
}