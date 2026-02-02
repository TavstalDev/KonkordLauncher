using System.Text;
using MinecraftSkinRender;
using MinecraftSkinRender.Image;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.User;

namespace Tavstal.KonkordLauncher.Core.Services;

/// <summary>
/// Provides services for interacting with the Starlight Skin API to retrieve skin and cape data.
/// </summary>
public static class SkinService
{
    /// <summary>
    /// Logger instance for logging errors and information related to the StartlightSkinService.
    /// </summary>
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(SkinService));
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
    
    public static async Task FetchSkins(string cacheDir, string uuid, AccountSkin skin)
    {
        string skinsDir = Path.Combine(cacheDir, "skins", uuid, skin.Id);
        
        if (!Directory.Exists(skinsDir))
            Directory.CreateDirectory(skinsDir);
        
        string texturePath = Path.Combine(skinsDir, "texture.png");
        string previewPath = Path.Combine(skinsDir, "preview.png");
        string headshotPath = Path.Combine(skinsDir, "head.png");
        
        // Fetch texture
        if (!File.Exists(texturePath))
        {
            string? textureResult = await HttpHelper.GetStringAsync($"https://sessionserver.mojang.com/session/minecraft/profile/{uuid}");
            if (textureResult == null)
            {
                _logger.Warn("Failed to download skin texture for UUID: " + uuid);
                return;
            }
            
            JObject textureJson = JObject.Parse(textureResult);
            string? base64Texture = textureJson["properties"]?.FirstOrDefault()?["value"]?.Value<string>();
            if (base64Texture == null)
            {
                _logger.Warn("Failed to parse skin texture for UUID: " + uuid);
                return;
            }
            byte[] textureBytes = Convert.FromBase64String(base64Texture);
            JObject textureData = JObject.Parse(Encoding.UTF8.GetString(textureBytes));
            string? skinUrl = textureData["textures"]?["SKIN"]?["url"]?.Value<string>();
            if (skinUrl == null)
            {
                _logger.Warn("No skin URL found for UUID: " + uuid);
                return;
            }
            
            byte[]? skinData = await HttpHelper.GetByteArrayAsync(skinUrl);
            if (skinData == null)
            {
                _logger.Warn("Failed to download skin data for UUID: " + uuid);
                return;
            }
            await File.WriteAllBytesAsync(texturePath, skinData);
        }

        await using var skinStream = File.OpenRead(texturePath);
        using var skinBitmap = SKBitmap.Decode(skinStream);
        if (skinBitmap == null)
        {
            _logger.Warn("Failed to decode skin bitmap for UUID: " + uuid);
            return;
        }

        // Make headshot
        Skin3DHeadTypeA.MakeHeadImage(skinBitmap).SavePng(headshotPath);
            
        // Make full skin
        Skin2DTypeB.MakeSkinImage(skinBitmap, skin.Model.Equals("classic", StringComparison.InvariantCultureIgnoreCase) ? SkinType.New : SkinType.NewSlim).SavePng(previewPath);
    }

    public static async Task FetchCapes(string cacheDir, List<Cape> capes)
    {
        string capesDir = Path.Combine(cacheDir, "capes");
        if (!Directory.Exists(capesDir))
            Directory.CreateDirectory(capesDir);
        
        var semaphore = new SemaphoreSlim(MaxParallelDownloads);
        var tasks = new List<Task>();
        
        foreach (Cape cape in capes)
        {
            await semaphore.WaitAsync();
            Task t = Task.Run(async () =>
            {
                try
                {
                    string capePath = Path.Combine(capesDir, $"{cape.Id}.png");
                    if (File.Exists(capePath))
                        return;
                    
                    var capeBytes = await HttpHelper.GetByteArrayAsync(cape.Url);
                    if (capeBytes == null)
                    {
                        _logger.Warn("Failed to download cape image for cape ID: " + cape.Id);
                        return;
                    }
                    await using var capeStream = new MemoryStream(capeBytes);
                    using var capeBitmap = SKBitmap.Decode(capeStream);
                    Cape2DTypaA.MakeCapeImage(capeBitmap).SavePng(capePath);
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