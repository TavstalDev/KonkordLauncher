using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MinecraftSkinRender;
using MinecraftSkinRender.Image;
using SkiaSharp;
using Tavstal.BeaconSDK.Models.Accounts;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Models.MojangApi.User;
using Tavstal.BeaconSDK.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Desktop.Services.Accounts;

/// <inheritdoc cref="ISkinService" />
public class SkinService : ISkinService
{
    private const int MaxParallelDownloads = 4;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IBeaconLogger<SkinService> _logger;

    public SkinService(IBeaconLogger<SkinService> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc />
    public async Task<byte[]?> GetFullSkinAsync(string username, string? skinUrl = null, bool enableCape = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var requestUrl =
                $"https://starlightskins.lunareclipse.studio/render/default/{username}/full?capeEnabled={enableCape}";
            if (skinUrl != null)
                requestUrl += $"&skinUrl={Uri.EscapeDataString(skinUrl)}";

            return await DownloadBytesAsync(requestUrl, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to download skin model.");
            return null;
        }
    }

    /// <inheritdoc />
    public async Task FetchOfflineSkinsAsync(string cacheDir, string accountId, string name,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient();
            var profileResult = await client.GetStringAsync(
                $"https://api.minecraftservices.com/minecraft/profile/lookup/name/{name}", cancellationToken);
            if (string.IsNullOrWhiteSpace(profileResult))
                return;

            string? uuid = null;
            using (var doc = JsonDocument.Parse(profileResult))
            {
                if (doc.RootElement.TryGetProperty("id", out var idElement))
                    uuid = idElement.GetString();
            }

            if (uuid == null)
                return;

            var skinsDir = Path.Combine(cacheDir, "skins", accountId);
            Directory.CreateDirectory(skinsDir);

            var texturePath = Path.Combine(skinsDir, "texture.png");
            var headshotPath = Path.Combine(skinsDir, "head.png");

            if (!File.Exists(texturePath))
            {
                var skinUrl = await ResolveSkinUrlAsync(client, uuid, cancellationToken);
                if (skinUrl == null)
                    return;

                var skinData = await DownloadBytesAsync(skinUrl, cancellationToken);
                if (skinData == null)
                    return;

                await File.WriteAllBytesAsync(texturePath, skinData, cancellationToken);
            }

            await using var skinStream = File.OpenRead(texturePath);
            using var skinBitmap = SKBitmap.Decode(skinStream);
            if (skinBitmap == null)
            {
                _logger.LogWarning("Failed to decode offline skin bitmap for UUID: " + uuid);
                return;
            }

            Skin3DHeadTypeB.MakeHeadImage(skinBitmap, 15, 65).SavePng(headshotPath);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to fetch offline skin for {name}.");
        }
    }

    /// <inheritdoc />
    public async Task FetchSkinsAsync(string cacheDir, string accountId, string uuid, AccountSkin skin,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient();
            var skinsDir = Path.Combine(cacheDir, "skins", accountId);
            Directory.CreateDirectory(skinsDir);

            var texturePath = Path.Combine(skinsDir, "texture.png");
            var previewPath = Path.Combine(skinsDir, "preview.png");
            var headshotPath = Path.Combine(skinsDir, "head.png");

            if (!File.Exists(texturePath))
            {
                var skinUrl = await ResolveSkinUrlAsync(client, uuid, cancellationToken);
                if (skinUrl == null)
                    return;

                var skinData = await DownloadBytesAsync(skinUrl, cancellationToken);
                if (skinData == null)
                    return;

                await File.WriteAllBytesAsync(texturePath, skinData, cancellationToken);
            }

            await using var skinStream = File.OpenRead(texturePath);
            using var skinBitmap = SKBitmap.Decode(skinStream);
            if (skinBitmap == null)
            {
                _logger.LogWarning("Failed to decode skin bitmap for UUID: " + uuid);
                return;
            }

            Skin3DHeadTypeB.MakeHeadImage(skinBitmap, 15, 65).SavePng(headshotPath);
            Skin2DTypeB.MakeSkinImage(skinBitmap,
                skin.Model.Equals("slim", StringComparison.InvariantCultureIgnoreCase)
                    ? SkinType.NewSlim
                    : SkinType.New).SavePng(previewPath);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to fetch skin for {uuid}.");
        }
    }

    /// <inheritdoc />
    public async Task FetchCapesAsync(string cacheDir, List<Cape> capes, CancellationToken cancellationToken = default)
    {
        try
        {
            var capesDir = Path.Combine(cacheDir, "capes");
            Directory.CreateDirectory(capesDir);

            using var semaphore = new SemaphoreSlim(MaxParallelDownloads);
            var tasks = new List<Task>();

            foreach (var cape in capes)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        await semaphore.WaitAsync(cancellationToken);
                        var capePath = Path.Combine(capesDir, $"{cape.Id}.png");
                        if (File.Exists(capePath))
                            return;

                        var capeBytes = await DownloadBytesAsync(cape.Url, cancellationToken);
                        if (capeBytes == null)
                        {
                            _logger.LogWarning("Failed to download cape image for cape ID: " + cape.Id);
                            return;
                        }

                        await using var capeStream = new MemoryStream(capeBytes);
                        using var capeBitmap = SKBitmap.Decode(capeStream);
                        if (capeBitmap == null)
                        {
                            _logger.LogWarning("Failed to decode cape image for cape ID: " + cape.Id);
                            return;
                        }

                        Cape2DTypaA.MakeCapeImage(capeBitmap).SavePng(capePath);
                    }
                    finally
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        semaphore.Release();
                    }
                }, cancellationToken));
            }

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to fetch capes.");
        }
    }

    private async Task<string?> ResolveSkinUrlAsync(HttpClient client, string uuid,
        CancellationToken cancellationToken)
    {
        var textureResult = await client.GetStringAsync(
            $"https://sessionserver.mojang.com/session/minecraft/profile/{uuid}", cancellationToken);
        if (string.IsNullOrWhiteSpace(textureResult))
        {
            _logger.LogWarning("Failed to download skin texture for UUID: " + uuid);
            return null;
        }

        string? base64Texture = null;
        using (var doc = JsonDocument.Parse(textureResult))
        {
            if (doc.RootElement.TryGetProperty("properties", out var properties) &&
                properties.GetArrayLength() > 0 &&
                properties[0].TryGetProperty("value", out var value))
                base64Texture = value.GetString();
        }

        if (base64Texture == null)
        {
            _logger.LogWarning("Failed to parse skin texture for UUID: " + uuid);
            return null;
        }

        var textureBytes = Convert.FromBase64String(base64Texture);
        using var textureData = JsonDocument.Parse(textureBytes);
        if (textureData.RootElement.TryGetProperty("textures", out var textures) &&
            textures.TryGetProperty("SKIN", out var skin) &&
            skin.TryGetProperty("url", out var skinUrl))
            return skinUrl.GetString();

        _logger.LogWarning("No skin URL found for UUID: " + uuid);
        return null;
    }

    private async Task<byte[]?> DownloadBytesAsync(string url, CancellationToken cancellationToken)
    {
        using var client = _httpClientFactory.CreateClient();
        return await client.GetByteArrayAsync(url, cancellationToken);
    }
}
