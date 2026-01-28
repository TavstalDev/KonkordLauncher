using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.User;

namespace Tavstal.KonkordLauncher.Core.Services;

/// <summary>
/// Provides services for interacting with the Starlight Skin API to retrieve skin and cape data.
/// </summary>
public static class StarlightSkinService
{
    /// <summary>
    /// Logger instance for logging errors and information related to the StartlightSkinService.
    /// </summary>
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(StarlightSkinService));
    private static readonly int MaxParallelDownloads = 16;

    /// <summary>
    /// HTTP client used for making requests to the Starlight Skin API.
    /// </summary>
    private static HttpClient? _httpClient = new()
    {
        DefaultRequestHeaders =
        {
            { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:125.0) Gecko/20100101 Firefox/125.0" },
            { "Accept", "*/*" },
            { "Accept-Encoding", "gzip, deflate, br" },
            { "Connection", "keep-alive" }
        }
    };

    /// <summary>
    /// Retrieves the full skin model for a given username.
    /// </summary>
    /// <param name="username">The username of the player whose skin is to be retrieved.</param>
    /// <param name="skinUrl">Optional custom skin URL to override the default skin.</param>
    /// <param name="enableCape">Indicates whether the cape should be included in the skin model.</param>
    /// <returns>A byte array containing the skin data, or null if the request fails.</returns>
    public static async Task<byte[]?> GetFullSkinAsync(string username, string? skinUrl = null, bool enableCape = true)
    {
        try
        {
            string requestUrl =
                $"https://starlightskins.lunareclipse.studio/render/default/{username}/full?capeEnabled={enableCape}";
            if (skinUrl != null)
                requestUrl += $"&skinUrl={skinUrl}";

            return await HttpHelper.GetByteArrayAsync(requestUrl);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to download skin model");
            _logger.Error(ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Retrieves a cape view for a given username and cape URL.
    /// </summary>
    /// <param name="username">The username of the player whose cape view is to be retrieved.</param>
    /// <param name="capeUrl">The URL of the cape texture to be used.</param>
    /// <param name="skinUrl">Optional custom skin URL to override the default skin.</param>
    /// <returns>A byte array containing the cape view data, or null if the request fails.</returns>
    public static async Task<byte[]?> GetCapeViewAsync(string username, string capeUrl, string? skinUrl = null)
    {
        try
        {
            string requestUrl =
                $"https://starlightskins.lunareclipse.studio/render/default/{username}/full?cameraPosition={{\"x\":\"0\",\"y\":\"18\",\"z\":\"15\"}}&cameraFocalPoint={{\"x\":\"0\",\"y\":\"15.9\",\"z\":\"3.35\"}}&capeEnabled=true&capeTexture={capeUrl}";

            if (skinUrl != null)
                requestUrl += $"&skinUrl={skinUrl}";

            return await HttpHelper.GetByteArrayAsync(requestUrl);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to download cape view");
            _logger.Error(ex.Message);
            return null;
        }
    }
    
    public static async Task<byte[]?> GetHeadshotAsync(string username, string? skinUrl = null)
    {
        try
        {
            string requestUrl =
                $"https://starlightskins.lunareclipse.studio/render/head/{username}/full";
            if (skinUrl != null)
                requestUrl += $"?skinUrl={skinUrl}";

            return await HttpHelper.GetByteArrayAsync(requestUrl);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to download skin model");
            _logger.Error(ex.Message);
            return null;
        }
    }
    
    public static async Task FetchSkins(string cacheDir, string uuid, string username, List<Cape> capes)
    {
        string skinsDir = Path.Combine(cacheDir, "skins", uuid);
        string capesDir = Path.Combine(cacheDir, "capes");
        if (!Directory.Exists(skinsDir))
            Directory.CreateDirectory(skinsDir);
        if (!Directory.Exists(capesDir))
            Directory.CreateDirectory(capesDir);
        
        var semaphore = new SemaphoreSlim(MaxParallelDownloads);
        var tasks = new List<Task>();

        // Fetch headshot
        Task t = Task.Run(async () =>
        {
            byte[]? skinResult = await StarlightSkinService.GetHeadshotAsync(username);
            if (skinResult != null)
            {
                string skinPath = Path.Combine(skinsDir, "head.png");
                await File.WriteAllBytesAsync(skinPath, skinResult);
            }
        });
        tasks.Add(t);
            
        // Fetch full skin
        t = Task.Run(async () =>
        {
            byte[]? skinResult = await StarlightSkinService.GetFullSkinAsync(username, enableCape: false);
            if (skinResult != null)
            {
                string skinPath = Path.Combine(skinsDir, "preview.png");
                await File.WriteAllBytesAsync(skinPath, skinResult);
            }
        });
        tasks.Add(t);
                
        // Fetch capes if not already cached
        foreach (Cape cape in capes)
        {
            await semaphore.WaitAsync();
            t = Task.Run(async () =>
            {
                try
                {
                    string capePath = Path.Combine(capesDir, $"{cape.Id}.png");
                    if (File.Exists(capePath))
                        return;
                    byte[]? capeResult =
                        await GetCapeViewAsync(username, cape.Url,
                            "https://textures.minecraft.net/texture/9d4f187f41cae641558f8787bf1e7be72a6d72911b21c97d916f0a7faaf28f7");
                    if (capeResult == null)
                    {
                        _logger.Warn("Failed to download cape view for cape ID: " + cape.Id);
                        return;
                    }

                    await File.WriteAllBytesAsync(capePath, capeResult);
                }
                finally
                {
                    semaphore.Release();
                }
            });
            tasks.Add(t);
        }
        
        await Task.WhenAll(tasks);
    }

    public static async Task FetchPreviewSkin(string cacheDir, string uuid, string username, string? skinId, bool isWide)
    {
        string skinsDir = Path.Combine(cacheDir, "skins", uuid);
        if (!Directory.Exists(skinsDir))
            Directory.CreateDirectory(skinsDir);
        bool showCape = skinId == null;
        skinId ??= "preview";
        skinId += isWide ? "_wide" : "_classic";
        string skinPath = Path.Combine(skinsDir, $"{skinId}.png");
        if (File.Exists(skinPath))
            return;
        
        byte[]? skinResult = await GetFullSkinAsync(username, enableCape: showCape);
        if (skinResult != null)
            await File.WriteAllBytesAsync(skinPath, skinResult);
    }
}