using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Avalonia.Media.Imaging;
using Modrinth.Models;
using Tavstal.KonkordLauncher.Common.Models.MetaCache;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Serialization;
using Tavstal.KonkordLauncher.Core.Models;
using File = System.IO.File;
using Version = Modrinth.Models.Version;

namespace Tavstal.KonkordLauncher.Common.Helpers;

public static class MetaCacheHelper
{
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(MetaCacheHelper));
    private static readonly HttpClient _httpClient = new();
    private static readonly ConcurrentDictionary<string, MetaCache> _cache = new();
    private static readonly ConcurrentDictionary<string, Bitmap> _bitmaps = new();
    private static readonly TimeSpan _searchCacheDuration = TimeSpan.FromHours(1);
    private static readonly TimeSpan _projectCacheDuration = TimeSpan.FromHours(24);
    private static bool _isInitialized = false;

    public static async Task InitAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
            return;
        
        try
        {
            if (!File.Exists(PathHelper.MetaCachePath))
            {
                _isInitialized = true;
                return; // No cache exists
            }

            var result = await JsonHelper.ReadJsonFileAsync<List<MetaCache>>(PathHelper.MetaCachePath, cancellationToken);
            if (result != null)
                foreach (var metaCache in result)
                {
                    if (!File.Exists(metaCache.Path) || !metaCache.IsValid())
                        continue;
                    
                    _cache.TryAdd(metaCache.Id, metaCache);
                }
            
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to initialize meta cache: {ex}");
        }
    }

    public static async Task<Bitmap?> GetImageAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes($"image:{imageUrl}"));
            string hash = Convert.ToHexString(sha1);

            if (_cache.TryGetValue(hash, out var cached) && cached.IsValid())
            {
                if (_bitmaps.TryGetValue(hash, out var cachedBitmap))
                    return cachedBitmap;
                await using var stream = File.OpenRead(cached.Path);
                var bitmap = new Bitmap(stream);
                _bitmaps.TryAdd(hash, bitmap);
                return bitmap;
            }
            
            var response = await _httpClient.GetAsync(imageUrl, cancellationToken);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            
            var settings = await LauncherHelper.GetLauncherSettingsAsync(cancellationToken: cancellationToken);
            var cacheDir = settings.Launcher.CacheDirectoryPath;
            Directory.CreateDirectory(cacheDir);
            string imagesDir = Path.Combine(cacheDir, "images");
            Directory.CreateDirectory(imagesDir);
            string cachedFilePath = Path.Combine(imagesDir, $"{hash}.png");
            await File.WriteAllBytesAsync(cachedFilePath, data, cancellationToken);
                
            _cache.TryAdd(hash, new MetaCache
            {
                Id = hash,
                Type = EMetaCacheType.IMAGE,
                Path = cachedFilePath,
                ValidUntil = DateTime.UtcNow.Add(_projectCacheDuration)
            });
            await SaveCacheAsync(cancellationToken);
            
            using var ms = new MemoryStream(data);
            Bitmap image = new Bitmap(ms);
            _bitmaps.TryAdd(hash, image);
            return image;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to get image in meta cache: {ex}");
            return null;
        }
    }
    
    public static async Task<Project?> GetProjectAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes($"project:{id}"));
            string hash = Convert.ToHexString(sha1);

            if (_cache.TryGetValue(hash, out var cached) && cached.IsValid())
                return await JsonHelper.ReadJsonFileAsync<Project>(cached.Path, cancellationToken);
            
            var response = await ModrinthHelper.GetProjectAsync(id, cancellationToken);
            if (response != null)
            {
                var settings = await LauncherHelper.GetLauncherSettingsAsync(cancellationToken: cancellationToken);
                var cacheDir = settings.Launcher.CacheDirectoryPath;
                Directory.CreateDirectory(cacheDir);
                string projectDir = Path.Combine(cacheDir, "projects");
                Directory.CreateDirectory(projectDir);
                string cachedFilePath = Path.Combine(projectDir, $"{hash}.json");
                await JsonHelper.WriteJsonFileAsync(cachedFilePath, response, cancellationToken);
                
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
            _logger.Error($"Failed to get project in meta cache: {ex}");
            return null;
        }
    }
    
    public static async Task<Project[]> GetProjectsAsync(List<string> ids, CancellationToken cancellationToken = default)
    {
        try
        {
            List<string> idsToFetch = [];
            List<Project> projects = [];
            var cacheReadTasks = new List<Task<Project>>();

            foreach (var id in ids)
            {
                var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes($"project:{id}"));
                string hash = Convert.ToHexString(sha1);

                if (_cache.TryGetValue(hash, out var cached) && cached.IsValid())
                    cacheReadTasks.Add(JsonHelper.ReadJsonFileAsync<Project>(cached.Path, cancellationToken)!);
                else
                    idsToFetch.Add(id);
            }
            
            var apiTask = idsToFetch.Any() 
                ? ModrinthHelper.GetProjectsAsync(idsToFetch, cancellationToken) 
                : Task.FromResult(Array.Empty<Project>());

            await Task.WhenAll(Task.WhenAll(cacheReadTasks), apiTask);

            projects.AddRange(cacheReadTasks.Select(t => t.Result));
            var remaining = await apiTask;
            if (remaining.Any())
                projects.AddRange(remaining);

            _ = Task.Run(async () =>
            {
                var settings = await LauncherHelper.GetLauncherSettingsAsync(cancellationToken: cancellationToken);
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

                    return JsonHelper.WriteJsonFileAsync(cachedFilePath, x, cancellationToken);
                });
                await Task.WhenAll(cacheTasks);
                await SaveCacheAsync(
                    cancellationToken);
            }, cancellationToken);

            return projects.ToArray();
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to get projects in meta cache: {ex}");
            return [];
        }
    }

    public static async Task<Version[]> GetVersionsAsync(List<string> ids, CancellationToken cancellationToken = default)
    {
        try
        {
            List<string> idsToFetch = [];
            List<Version> versions = [];
            var cacheReadTasks = new List<Task<Version>>();

            foreach (var id in ids)
            {
                var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes($"version:{id}"));
                string hash = Convert.ToHexString(sha1);

                if (_cache.TryGetValue(hash, out var cached) && cached.IsValid())
                    cacheReadTasks.Add(JsonHelper.ReadJsonFileAsync<Version>(cached.Path, cancellationToken)!);
                else
                    idsToFetch.Add(id);
            }
            
            var apiTask = idsToFetch.Any() 
                ? ModrinthHelper.GetVersionsAsync(idsToFetch, cancellationToken) 
                : Task.FromResult(Array.Empty<Version>());

            await Task.WhenAll(Task.WhenAll(cacheReadTasks), apiTask);

            versions.AddRange(cacheReadTasks.Select(t => t.Result));
            var remaining = await apiTask;
            if (remaining.Any())
                versions.AddRange(remaining);

            _ = Task.Run(async () =>
            {
                var settings = await LauncherHelper.GetLauncherSettingsAsync(cancellationToken: cancellationToken);
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

                    return JsonHelper.WriteJsonFileAsync(cachedFilePath, x, cancellationToken);
                });
                await Task.WhenAll(cacheTasks);
                await SaveCacheAsync(
                    cancellationToken);
            }, cancellationToken);

            return versions.ToArray();
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to get versions in meta cache: {ex}");
            return [];
        }
    }
    
    public static async Task<SearchResponse?> SearchModpacksAsync(string? query = null, string? version = null, List<string>? categories = null,
        int offset = 0, CancellationToken cancellationToken = default)
    {
        try
        {
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine("modpack:");
            if (!string.IsNullOrEmpty(query))
                stringBuilder.AppendLine("q=" + query);
            if (!string.IsNullOrEmpty(version))
                stringBuilder.AppendLine("v=" + version);
            if (categories is { Count: > 0 })
                stringBuilder.AppendLine("categories=" + string.Join(", ", categories));
            stringBuilder.AppendLine("offset=" + offset);
            
            var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
            string hash = Convert.ToHexString(sha1);
            
            if (_cache.TryGetValue(hash, out var cached) && cached.IsValid())
                return await JsonHelper.ReadJsonFileAsync<SearchResponse>(cached.Path, cancellationToken);
            
            var response = await ModrinthHelper.SearchModpacksAsync(query, version, categories, offset, cancellationToken);
            if (response != null)
            {
                var settings = await LauncherHelper.GetLauncherSettingsAsync(cancellationToken: cancellationToken);
                var cacheDir = settings.Launcher.CacheDirectoryPath;
                Directory.CreateDirectory(cacheDir);
                string searchDir = Path.Combine(cacheDir, "searchs");
                Directory.CreateDirectory(searchDir);
                string cachedFilePath = Path.Combine(searchDir, $"{hash}.json");
                await JsonHelper.WriteJsonFileAsync(cachedFilePath, response, cancellationToken);
                
                _cache.TryAdd(hash, new MetaCache
                {
                    Id = hash,
                    Type = EMetaCacheType.SEARCH_RESULT,
                    Path = cachedFilePath,
                    ValidUntil = DateTime.UtcNow.Add(_searchCacheDuration)
                });
                await SaveCacheAsync(cancellationToken);
            }
            return response;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to search modpacks in meta cache: {ex}");
            return null;
        }
    }
    
    public static async Task<SearchResponse?> SearchModsAsync(string? query = null, string? version = null, List<string>? categories = null, 
        int offset = 0, CancellationToken cancellationToken = default)
    {
        try
        {
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine("mod:");
            if (!string.IsNullOrEmpty(query))
                stringBuilder.AppendLine("q=" + query);
            if (!string.IsNullOrEmpty(version))
                stringBuilder.AppendLine("v=" + version);
            if (categories is { Count: > 0 })
                stringBuilder.AppendLine("categories=" + string.Join(", ", categories));
            stringBuilder.AppendLine("offset=" + offset);
            
            var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
            string hash = Convert.ToHexString(sha1);
            
            if (_cache.TryGetValue(hash, out var cached) && cached.IsValid())
                return await JsonHelper.ReadJsonFileAsync<SearchResponse>(cached.Path, cancellationToken);
            
            var response = await ModrinthHelper.SearchModsAsync(query, version, categories, offset, cancellationToken);
            if (response != null)
            {
                var settings = await LauncherHelper.GetLauncherSettingsAsync(cancellationToken: cancellationToken);
                var cacheDir = settings.Launcher.CacheDirectoryPath;
                Directory.CreateDirectory(cacheDir);
                string searchDir = Path.Combine(cacheDir, "searchs");
                Directory.CreateDirectory(searchDir);
                string cachedFilePath = Path.Combine(searchDir, $"{hash}.json");
                await JsonHelper.WriteJsonFileAsync(cachedFilePath, response, cancellationToken);
                
                _cache.TryAdd(hash, new MetaCache
                {
                    Id = hash,
                    Type = EMetaCacheType.SEARCH_RESULT,
                    Path = cachedFilePath,
                    ValidUntil = DateTime.UtcNow.Add(_searchCacheDuration)
                });
                await SaveCacheAsync(cancellationToken);
            }
            return response;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to search mods in meta cache: {ex}");
            return null;
        }
    }
    
    public static async Task<SearchResponse?> SearchResourcePacksAsync(string? query = null, string? version = null, List<string>? categories = null, 
        int offset = 0, CancellationToken cancellationToken = default)
    {
        try
        {
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine("resource_pack:");
            if (!string.IsNullOrEmpty(query))
                stringBuilder.AppendLine("q=" + query);
            if (!string.IsNullOrEmpty(version))
                stringBuilder.AppendLine("v=" + version);
            if (categories is { Count: > 0 })
                stringBuilder.AppendLine("categories=" + string.Join(", ", categories));
            stringBuilder.AppendLine("offset=" + offset);
            
            var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
            string hash = Convert.ToHexString(sha1);
            
            if (_cache.TryGetValue(hash, out var cached) && cached.IsValid())
                return await JsonHelper.ReadJsonFileAsync<SearchResponse>(cached.Path, cancellationToken);
            
            var response = await ModrinthHelper.SearchResourcePackAsync(query, version, categories, offset, cancellationToken);
            if (response != null)
            {
                var settings = await LauncherHelper.GetLauncherSettingsAsync(cancellationToken: cancellationToken);
                var cacheDir = settings.Launcher.CacheDirectoryPath;
                Directory.CreateDirectory(cacheDir);
                string searchDir = Path.Combine(cacheDir, "searchs");
                Directory.CreateDirectory(searchDir);
                string cachedFilePath = Path.Combine(searchDir, $"{hash}.json");
                await JsonHelper.WriteJsonFileAsync(cachedFilePath, response, cancellationToken);
                
                _cache.TryAdd(hash, new MetaCache
                {
                    Id = hash,
                    Type = EMetaCacheType.SEARCH_RESULT,
                    Path = cachedFilePath,
                    ValidUntil = DateTime.UtcNow.Add(_searchCacheDuration)
                });
                await SaveCacheAsync(cancellationToken);
            }
            return response;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to search resource packs in meta cache: {ex}");
            return null;
        }
    }
    
    public static async Task<SearchResponse?> SearchShaderPacksAsync(string? query = null, string? version = null, List<string>? categories = null, 
        int offset = 0, CancellationToken cancellationToken = default)
    {
        try
        {
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine("shader_pack:");
            if (!string.IsNullOrEmpty(query))
                stringBuilder.AppendLine("q=" + query);
            if (!string.IsNullOrEmpty(version))
                stringBuilder.AppendLine("v=" + version);
            if (categories is { Count: > 0 })
                stringBuilder.AppendLine("categories=" + string.Join(", ", categories));
            stringBuilder.AppendLine("offset=" + offset);
            
            var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
            string hash = Convert.ToHexString(sha1);
            
            if (_cache.TryGetValue(hash, out var cached) && cached.IsValid())
                return await JsonHelper.ReadJsonFileAsync<SearchResponse>(cached.Path, cancellationToken);
            
            var response = await ModrinthHelper.SearchShaderPacksAsync(query, version, categories, offset, cancellationToken);
            if (response != null)
            {
                var settings = await LauncherHelper.GetLauncherSettingsAsync(cancellationToken: cancellationToken);
                var cacheDir = settings.Launcher.CacheDirectoryPath;
                Directory.CreateDirectory(cacheDir);
                string searchDir = Path.Combine(cacheDir, "searchs");
                Directory.CreateDirectory(searchDir);
                string cachedFilePath = Path.Combine(searchDir, $"{hash}.json");
                await JsonHelper.WriteJsonFileAsync(cachedFilePath, response, cancellationToken);
                
                _cache.TryAdd(hash, new MetaCache
                {
                    Id = hash,
                    Type = EMetaCacheType.SEARCH_RESULT,
                    Path = cachedFilePath,
                    ValidUntil = DateTime.UtcNow.Add(_searchCacheDuration)
                });
                await SaveCacheAsync(cancellationToken);
            }
            return response;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to search shaders in meta cache: {ex}");
            return null;
        }
    }

    private static async Task SaveCacheAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheSnapshot = _cache.Values.ToArray();
            await JsonHelper.WriteJsonFileAsync(PathHelper.MetaCachePath, cacheSnapshot, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to save meta cache: {ex}");
        }
    }
}