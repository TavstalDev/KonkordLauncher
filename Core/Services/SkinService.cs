using System.Text;
using MinecraftSkinRender;
using MinecraftSkinRender.Image;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using Tavstal.KonkordLauncher.Core.Helpers.Network;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.User;

namespace Tavstal.KonkordLauncher.Core.Services;

/// <summary>
/// Provides services for managing and rendering Minecraft skins and capes.
/// </summary>
public static class SkinService
{
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(SkinService));
    private static readonly int MaxParallelDownloads = 16;

    /// <summary>
    /// Retrieves the full skin image for a given username.
    /// </summary>
    /// <param name="username">The username of the player.</param>
    /// <param name="skinUrl">Optional URL of the skin.</param>
    /// <param name="enableCape">Indicates whether the cape should be included in the render.</param>
    /// <returns>A byte array containing the skin image, or null if the operation fails.</returns>
    public static async Task<byte[]?> GetFullSkinAsync(string username, string? skinUrl = null, bool enableCape = true, CancellationToken cancellationToken = default)
    {
        try
        {
            string requestUrl =
                $"https://starlightskins.lunareclipse.studio/render/default/{username}/full?capeEnabled={enableCape}";
            if (skinUrl != null)
                requestUrl += $"&skinUrl={skinUrl}";

            return await HttpHelper.GetByteArrayAsync(requestUrl, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to download skin model");
            _logger.Error(ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Fetches and caches offline skins for a given account and username.
    /// </summary>
    /// <param name="cacheDir">The directory where skins are cached.</param>
    /// <param name="accountId">The account ID associated with the skin.</param>
    /// <param name="name">The username of the player.</param>
    public static async Task FetchOfflineSkins(string cacheDir, string accountId, string name, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if there is already a skin for this username
            byte[]? profileResult = await HttpHelper.GetByteArrayAsync(
                $"https://api.minecraftservices.com/minecraft/profile/lookup/name/{name}", cancellationToken);
            if (profileResult == null)
                return;

            string? uuid = JObject.Parse(Encoding.UTF8.GetString(profileResult))["id"]?.Value<string>();
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
                    await HttpHelper.GetStringAsync(
                        $"https://sessionserver.mojang.com/session/minecraft/profile/{uuid}", cancellationToken);
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

                byte[]? skinData = await HttpHelper.GetByteArrayAsync(skinUrl, cancellationToken);
                if (skinData == null)
                {
                    _logger.Warn("Failed to download skin data for UUID: " + uuid);
                    return;
                }

                await File.WriteAllBytesAsync(texturePath, skinData, cancellationToken);
            }

            await using var skinStream = File.OpenRead(texturePath);
            using var skinBitmap = SKBitmap.Decode(skinStream);
            if (skinBitmap == null)
            {
                _logger.Warn("Failed to decode skin bitmap for UUID: " + uuid);
                return;
            }

            // Make headshot
            Skin3DHeadTypeB.MakeHeadImage(skinBitmap, 15, 65).SavePng(headshotPath);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to fetch offline skin for username: " + name);
            _logger.Error(ex);
        }
    }

    /// <summary>
    /// Fetches and caches skins for a given account and UUID.
    /// </summary>
    /// <param name="cacheDir">The directory where skins are cached.</param>
    /// <param name="accountId">The account ID associated with the skin.</param>
    /// <param name="uuid">The UUID of the player.</param>
    /// <param name="skin">The skin details.</param>
    public static async Task FetchSkins(string cacheDir, string accountId, string uuid, AccountSkin skin, CancellationToken cancellationToken = default)
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
                    await HttpHelper.GetStringAsync(
                        $"https://sessionserver.mojang.com/session/minecraft/profile/{uuid}", cancellationToken);
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

                byte[]? skinData = await HttpHelper.GetByteArrayAsync(skinUrl, cancellationToken);
                if (skinData == null)
                {
                    _logger.Warn("Failed to download skin data for UUID: " + uuid);
                    return;
                }

                await File.WriteAllBytesAsync(texturePath, skinData, cancellationToken);
            }

            await using var skinStream = File.OpenRead(texturePath);
            using var skinBitmap = SKBitmap.Decode(skinStream);
            if (skinBitmap == null)
            {
                _logger.Warn("Failed to decode skin bitmap for UUID: " + uuid);
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
            _logger.Error("Failed to fetch skin for UUID: " + uuid);
            _logger.Error(ex);
        }
    }

    /// <summary>
    /// Fetches and caches capes for a list of cape objects.
    /// </summary>
    /// <param name="cacheDir">The directory where capes are cached.</param>
    /// <param name="capes">The list of capes to fetch.</param>
    public static async Task FetchCapes(string cacheDir, List<Cape> capes, CancellationToken cancellationToken = default)
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

                        var capeBytes = await HttpHelper.GetByteArrayAsync(cape.Url, cancellationToken);
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
                }, cancellationToken);
                tasks.Add(t);
            }

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to fetch capes");
            _logger.Error(ex);
        }
    }

    /// <summary>
    /// Fetches and caches a preview skin for a given UUID and username.
    /// </summary>
    /// <param name="cacheDir">The directory where skins are cached.</param>
    /// <param name="uuid">The UUID of the player.</param>
    /// <param name="username">The username of the player.</param>
    /// <param name="skinId">The ID of the skin.</param>
    /// <param name="isWide">Indicates whether the skin is wide.</param>
    public static async Task FetchPreviewSkin(string cacheDir, string uuid, string username, string? skinId, bool isWide, CancellationToken cancellationToken = default)
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

        byte[]? skinResult = await GetFullSkinAsync(username, enableCape: showCape, cancellationToken: cancellationToken);
        if (skinResult != null)
            await File.WriteAllBytesAsync(skinPath, skinResult, cancellationToken);
    }
}