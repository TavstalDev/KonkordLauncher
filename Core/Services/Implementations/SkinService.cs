using System.Text.Json;
using MinecraftSkinRender;
using MinecraftSkinRender.Image;
using SkiaSharp;
using Tavstal.KonkordLauncher.Core.Models.Accounts;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.User;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Core.Services.Implementations;

/// <inheritdoc/>
public class SkinService : ISkinService
{
    private readonly ICustomLogger _logger;
    private readonly IHttpService _httpService;
    private const int MaxParallelDownloads = 4;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SkinService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance used for recording diagnostic information, warnings, and errors related to skin and cape operations.</param>
    /// <param name="httpService">The HTTP service instance used for making asynchronous web requests to fetch skin textures, capes, and player profile data from external APIs.</param>
    public SkinService(ICustomLogger<SkinService> logger, IHttpService httpService)
    {
        _logger = logger;
        _httpService = httpService;
    }
    
    /// <inheritdoc/>
    public async Task<byte[]?> GetFullSkinAsync(string username, string? skinUrl = null, bool enableCape = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string requestUrl =
                $"https://starlightskins.lunareclipse.studio/render/default/{username}/full?capeEnabled={enableCape}";
            if (skinUrl != null)
                requestUrl += $"&skinUrl={skinUrl}";

            return await _httpService.GetByteArrayAsync(requestUrl, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to download skin model:");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task FetchOfflineSkinsAsync(string cacheDir, string accountId, string name,
        CancellationToken cancellationToken = default)
    {
       try
       {
           // Check if there is already a skin for this username
           byte[]? profileResult = await _httpService.GetByteArrayAsync(
               $"https://api.minecraftservices.com/minecraft/profile/lookup/name/{name}", cancellationToken);
           if (profileResult == null)
               return;

           string? uuid = null;
           using (JsonDocument doc = JsonDocument.Parse(profileResult))
           {
               if (doc.RootElement.TryGetProperty("id", out JsonElement idElement))
                   uuid = idElement.GetString();
           }
           if (uuid == null)
               return;

           string skinsDir = Path.Combine(cacheDir, "skins", accountId);

           if (!Directory.Exists(skinsDir))
               Directory.CreateDirectory(skinsDir);

           string texturePath = Path.Combine(skinsDir, "texture.png");
           string headshotPath = Path.Combine(skinsDir, "head.png");

           // Fetch texture
           if (!File.Exists(texturePath))
           {
               string? textureResult =
                   await _httpService.GetStringAsync(
                       $"https://sessionserver.mojang.com/session/minecraft/profile/{uuid}", cancellationToken);
               if (textureResult == null)
               {
                   _logger.LogWarning("Failed to download skin texture for UUID: " + uuid);
                   return;
               }

               string? base64Texture = null;
               using (JsonDocument doc = JsonDocument.Parse(textureResult))
               {
                   if (doc.RootElement.TryGetProperty("properties", out JsonElement element))
                   {
                       if (element.GetArrayLength() > 0 && element[0].TryGetProperty("value", out var value))
                       {
                           base64Texture = value.ToString();
                       }
                   }
               }
               
               if (base64Texture == null)
               {
                   _logger.LogWarning("Failed to parse skin texture for UUID: " + uuid);
                   return;
               }
               
               byte[] textureBytes = Convert.FromBase64String(base64Texture);
               string? skinUrl = null;
               using (JsonDocument textureData = JsonDocument.Parse(textureBytes))
               {
                   if (textureData.RootElement.TryGetProperty("textures", out JsonElement texturesElement) &&
                       texturesElement.TryGetProperty("SKIN", out JsonElement skinElement) &&
                       skinElement.TryGetProperty("url", out JsonElement urlElement))
                   {
                       skinUrl = urlElement.GetString();
                   }
               }
               if (skinUrl == null)
               {
                   _logger.LogWarning("No skin URL found for UUID: " + uuid);
                   return;
               }

               byte[]? skinData = await _httpService.GetByteArrayAsync(skinUrl, cancellationToken);
               if (skinData == null)
               {
                   _logger.LogWarning("Failed to download skin data for UUID: " + uuid);
                   return;
               }

               await File.WriteAllBytesAsync(texturePath, skinData, cancellationToken);
           }

           await using var skinStream = File.OpenRead(texturePath);
           using var skinBitmap = SKBitmap.Decode(skinStream);
           if (skinBitmap == null)
           {
               _logger.LogWarning("Failed to decode skin bitmap for UUID: " + uuid);
               return;
           }

           // Make headshot
           Skin3DHeadTypeB.MakeHeadImage(skinBitmap, 15, 65).SavePng(headshotPath);
       }
       catch (Exception ex)
       {
           _logger.LogCritical(ex, $"Failed to fetch offline skin for {name}:" );
       }
    }

    /// <inheritdoc/>
    public async Task FetchSkinsAsync(string cacheDir, string accountId, string uuid, AccountSkin skin,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string skinsDir = Path.Combine(cacheDir, "skins", accountId, skin.Id);

            if (!Directory.Exists(skinsDir))
                Directory.CreateDirectory(skinsDir);

            string texturePath = Path.Combine(skinsDir, "texture.png");
            string previewPath = Path.Combine(skinsDir, "preview.png");
            string headshotPath = Path.Combine(skinsDir, "head.png");

            // Fetch texture
            if (!File.Exists(texturePath))
            {
                string? textureResult =
                    await _httpService.GetStringAsync(
                        $"https://sessionserver.mojang.com/session/minecraft/profile/{uuid}", cancellationToken);
                if (textureResult == null)
                {
                    _logger.LogWarning("Failed to download skin texture for UUID: " + uuid);
                    return;
                }

                string? base64Texture = null;
                using (JsonDocument doc = JsonDocument.Parse(textureResult))
                {
                    if (doc.RootElement.TryGetProperty("properties", out JsonElement element))
                    {
                        if (element.GetArrayLength() > 0 && element[0].TryGetProperty("value", out var value))
                        {
                            base64Texture = value.ToString();
                        }
                    }
                }
                
                if (base64Texture == null)
                {
                    _logger.LogWarning("Failed to parse skin texture for UUID: " + uuid);
                    return;
                }

                byte[] textureBytes = Convert.FromBase64String(base64Texture);
                string? skinUrl = null;
                using (JsonDocument textureData = JsonDocument.Parse(textureBytes))
                {
                    if (textureData.RootElement.TryGetProperty("textures", out JsonElement texturesElement) &&
                        texturesElement.TryGetProperty("SKIN", out JsonElement skinElement) &&
                        skinElement.TryGetProperty("url", out JsonElement urlElement))
                    {
                        skinUrl = urlElement.GetString();
                    }
                }
                if (skinUrl == null)
                {
                    _logger.LogWarning("No skin URL found for UUID: " + uuid);
                    return;
                }

                byte[]? skinData = await _httpService.GetByteArrayAsync(skinUrl, cancellationToken);
                if (skinData == null)
                {
                    _logger.LogWarning("Failed to download skin data for UUID: " + uuid);
                    return;
                }

                await File.WriteAllBytesAsync(texturePath, skinData, cancellationToken);
            }

            await using var skinStream = File.OpenRead(texturePath);
            using var skinBitmap = SKBitmap.Decode(skinStream);
            if (skinBitmap == null)
            {
                _logger.LogWarning("Failed to decode skin bitmap for UUID: " + uuid);
                return;
            }

            // Make headshot
            Skin3DHeadTypeB.MakeHeadImage(skinBitmap, 15, 65).SavePng(headshotPath);

            // Make full skin
            Skin2DTypeB.MakeSkinImage(skinBitmap,
                skin.Model.Equals("classic", StringComparison.InvariantCultureIgnoreCase)
                    ? SkinType.New
                    : SkinType.NewSlim).SavePng(previewPath);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to fetch skin for {uuid}:");
        }
    }

    /// <inheritdoc/>
    public async Task FetchCapesAsync(string cacheDir, List<Cape> capes, CancellationToken cancellationToken = default)
    {
        try
        {
            string capesDir = Path.Combine(cacheDir, "capes");
            if (!Directory.Exists(capesDir))
                Directory.CreateDirectory(capesDir);

            var semaphore = new SemaphoreSlim(MaxParallelDownloads);
            var tasks = new List<Task>();

            foreach (Cape cape in capes)
            {
                await semaphore.WaitAsync(cancellationToken);
                Task t = Task.Run(async () =>
                {
                    try
                    {
                        string capePath = Path.Combine(capesDir, $"{cape.Id}.png");
                        if (File.Exists(capePath))
                            return;

                        var capeBytes = await _httpService.GetByteArrayAsync(cape.Url, cancellationToken);
                        if (capeBytes == null)
                        {
                            _logger.LogWarning("Failed to download cape image for cape ID: " + cape.Id);
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
                }, cancellationToken);
                tasks.Add(t);
            }

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to fetch capes:");
        }
    }

    /// <inheritdoc/>
    public async Task FetchPreviewSkinAsync(string cacheDir, string uuid, string username, string? skinId, bool isWide,
        CancellationToken cancellationToken = default)
    {
        string skinsDir = Path.Combine(cacheDir, "skins", uuid);
        Directory.CreateDirectory(skinsDir);
        bool showCape = skinId == null;
        skinId ??= "preview";
        skinId += isWide ? "_wide" : "_classic";
        string skinPath = Path.Combine(skinsDir, $"{skinId}.png");
        if (File.Exists(skinPath))
            return;

        byte[]? skinResult = await GetFullSkinAsync(username, enableCape: showCape, cancellationToken: cancellationToken);
        if (skinResult != null)
            await File.WriteAllBytesAsync(skinPath, skinResult, cancellationToken);
    }
}