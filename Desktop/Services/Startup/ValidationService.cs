using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tavstal.BeaconSDK.Enums;
using Tavstal.BeaconSDK.Models.Config;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.BeaconSDK.Services.Abstractions.Auth;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Services.Accounts;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;

namespace Tavstal.KonkordLauncher.Desktop.Services.Startup;

/// <inheritdoc />
public class ValidationService : IValidationService
{
    private readonly BeaconConfiguration _beaconConfiguration;
    private readonly IEncryptionService _encryptionService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILauncherStore _launcherStore;
    private readonly IBeaconLogger _logger;
    private readonly IManifestService _manifestService;
    private readonly IMicrosoftAuthService _microsoftAuthService;
    private readonly IScreenService _screenService;
    private readonly ISkinService _skinService;

    public ValidationService(IBeaconLogger<ValidationService> logger, IEncryptionService encryptionService,
        ILauncherStore launcherStore,
        IManifestService manifestService, IMicrosoftAuthService microsoftAuthService, ISkinService skinService,
        IScreenService screenService,
        IHttpClientFactory httpClientFactory, BeaconConfiguration beaconConfiguration)
    {
        _logger = logger;
        _encryptionService = encryptionService;
        _launcherStore = launcherStore;
        _manifestService = manifestService;
        _microsoftAuthService = microsoftAuthService;
        _skinService = skinService;
        _screenService = screenService;
        _httpClientFactory = httpClientFactory;
        _beaconConfiguration = beaconConfiguration;
    }

    /// <inheritdoc />
    public async Task<bool> ValidateLauncherDirectoryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Note: Also creates the config file if it does not exist
            var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);

            Directory.CreateDirectory(settings.Launcher.InstancesDirectoryPath);
            Directory.CreateDirectory(settings.Launcher.IconsDirectoryPath);
            Directory.CreateDirectory(settings.Launcher.JavaDirectoryPath);
            Directory.CreateDirectory(settings.Launcher.TranslationsDirectoryPath);
            Directory.CreateDirectory(settings.Launcher.VersionsDirectoryPath);
            Directory.CreateDirectory(settings.Launcher.CacheDirectoryPath);

            var skinsDir = Path.Combine(settings.Launcher.CacheDirectoryPath, "skins");
            Directory.CreateDirectory(skinsDir);

            Directory.CreateDirectory(settings.Launcher.LibrariesDirectoryPath);
            Directory.CreateDirectory(settings.Launcher.AssetsDirectoryPath);

            var indexes = Path.Combine(settings.Launcher.AssetsDirectoryPath, "indexes");
            Directory.CreateDirectory(indexes);

            Directory.CreateDirectory(settings.Launcher.ManifestsDirectoryPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to validate data folder:");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ValidateAccounts(CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await _launcherStore.GetAccountDataAsync(cancellationToken);
            var shouldUpdate = false;
            foreach (var acc in data.Accounts)
            {
                if (acc.Type != EAccountType.MICROSOFT)
                    continue;

                if (acc.AccessTokenExpireDate < DateTime.UtcNow)
                {
                    var refreshToken = Encoding.UTF8.GetString(acc.GetRefreshToken(_encryptionService));
                    await _microsoftAuthService.RefreshLoginAsync(refreshToken, cancellationToken);
                    // ReSharper disable once RedundantAssignment
                    refreshToken = string.Empty;
                    acc.MojangProfile = _microsoftAuthService.MojangProfile;
                    acc.AccessTokenExpireDate = _microsoftAuthService.Account!.AccessTokenExpireDate;
                    acc.SetAccessToken(_microsoftAuthService.Account.GetAccessToken(_encryptionService),
                        _encryptionService);
                    acc.SetRefreshToken(_microsoftAuthService.Account.GetRefreshToken(_encryptionService),
                        _encryptionService);
                    _microsoftAuthService.Reset();
                    shouldUpdate = true;
                }
                else
                {
                    var accessToken = Encoding.UTF8.GetString(acc.GetAccessToken(_encryptionService));
                    var mojangProfile =
                        await _microsoftAuthService.FetchMinecraftProfileAsync(accessToken, cancellationToken);

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                    if (mojangProfile?.Id == null)
                        continue;

                    acc.MojangProfile = mojangProfile;
                    shouldUpdate = true;
                }
            }

            if (shouldUpdate)
                await _launcherStore.SaveAccountDataAsync(data, cancellationToken);

            var account = data.Accounts.FirstOrDefault(a => a.Id == data.SelectedAccountId);
            if (account == null)
                return true;

            return account.Type switch
            {
#pragma warning disable CS0618 // Type or member is obsolete
                EAccountType.MICROSOFT => account.EncryptedAccessToken.Length > 0,
#pragma warning restore CS0618 // Type or member is obsolete
                _ => true
            };
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to validate accounts:");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ValidateManifests(IKonkordProgress? progressReporter = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);
            var refreshManifests = DateTime.UtcNow > settings.CacheRefreshDate;

            // Vanilla
            if (!File.Exists(settings.Launcher.GetVanillaManifestPath()) || refreshManifests)
            {
                var progress = new Progress<double>();
                progress.ProgressChanged += (_, e) =>
                {
                    progressReporter?.Report(e, "startup.validation.manifests.download", "minecraft",
                        e.ToString("0.00"));
                };
                await _manifestService.DownloadMinecraftManifestAsync(settings.Launcher.GetVanillaManifestPath(),
                    progress, cancellationToken);
            }

            if (await _manifestService.GetMinecraftManifestAsync(settings.Launcher.GetVanillaManifestPath(),
                    cancellationToken) == null)
                _logger.LogError("Failed to load Minecraft manifest");

            // Fabric
            if (!File.Exists(settings.Launcher.GetFabricManifestPath()) || refreshManifests)
            {
                var progress = new Progress<double>();
                progress.ProgressChanged += (_, e) =>
                {
                    progressReporter?.Report(e, "startup.validation.manifests.download", "fabric", e.ToString("0.00"));
                };
                await _manifestService.DownloadFabricManifestAsync(settings.Launcher.GetFabricManifestPath(), progress,
                    cancellationToken);
            }

            if (await _manifestService.GetFabricManifestAsync(settings.Launcher.GetFabricManifestPath(),
                    cancellationToken) == null)
                _logger.LogError("Failed to load Fabric manifest");

            // Quilt
            if (!File.Exists(settings.Launcher.GetQuiltManifestPath()) || refreshManifests)
            {
                var progress = new Progress<double>();
                progress.ProgressChanged += (_, e) =>
                {
                    progressReporter?.Report(e, "startup.validation.manifests.download", "quilt", e.ToString("0.00"));
                };
                await _manifestService.DownloadQuiltManifestAsync(settings.Launcher.GetQuiltManifestPath(), progress,
                    cancellationToken);
            }

            if (await _manifestService.GetQuiltManifestAsync(settings.Launcher.GetQuiltManifestPath(),
                    cancellationToken) == null)
                _logger.LogError("Failed to load Quilt manifest");

            // Forge
            if (!File.Exists(settings.Launcher.GetForgeManifestPath()) || refreshManifests)
            {
                var progress = new Progress<double>();
                progress.ProgressChanged += (_, e) =>
                {
                    progressReporter?.Report(e, "startup.validation.manifests.download", "forge", e.ToString("0.00"));
                };
                await _manifestService.DownloadForgeManifestAsync(settings.Launcher.GetForgeManifestPath(), progress,
                    cancellationToken);
            }

            if (await _manifestService.GetForgeManifestAsync(settings.Launcher.GetForgeManifestPath(),
                    cancellationToken) == null)
                _logger.LogError("Failed to load Forge manifest");


            // NeoForge
            try
            {
                if (!File.Exists(settings.Launcher.GetNeoForgeManifestPath()) || refreshManifests)
                {
                    var progress = new Progress<double>();
                    progress.ProgressChanged += (_, e) =>
                    {
                        progressReporter?.Report(e, "startup.validation.manifests.download", "neoforge",
                            e.ToString("0.00"));
                    };
                    await _manifestService.DownloadNeoForgeManifestAsync(settings.Launcher.GetNeoForgeManifestPath(),
                        progress, cancellationToken);
                }

                if (await _manifestService.GetNeoForgeManifestAsync(settings.Launcher.GetNeoForgeManifestPath(),
                        cancellationToken) == null)
                    _logger.LogError("Failed to load NeoForge manifest");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to validate NeoForge manifest:");
                // Skipping due to known issues with the NeoForge manifest (as of 2026. 03. 09.)
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to validate manifests:");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ValidateCachesAsync(IKonkordProgress? progressReporter = null,
        CancellationToken cancellationToken = default)
    {
        // Refresh GitHub cache
        progressReporter?.Report("startup.validation.github.status");
        var settings = await _launcherStore.GetSettingsAsync(_screenService.GetResolution(), cancellationToken);
        var shouldRefreshCache = settings.CacheRefreshDate < DateTime.UtcNow;
        var githubCachePath = Path.Combine(settings.Launcher.CacheDirectoryPath, "github_cache.json");
        if (!File.Exists(githubCachePath) || shouldRefreshCache)
        {
            using var client = _httpClientFactory.CreateClient("default");
            string response;
            try
            {
                response =
                    await client.GetStringAsync("https://api.github.com/repos/TavstalDev/KonkordLauncher/releases",
                        cancellationToken);
                if (string.IsNullOrWhiteSpace(response))
                {
                    _logger.LogError("Failed to fetch GitHub cache data");
                    response = "[]";
                }
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "Failed to fetch GitHub cache data");
                response = "[]";
            }

            await File.WriteAllTextAsync(githubCachePath, response, cancellationToken);
        }

        // Refresh skins cache
        progressReporter?.Report("startup.validation.skins");
        var accountData = await _launcherStore.GetAccountDataAsync(cancellationToken);
        var semaphore = new SemaphoreSlim(_beaconConfiguration.Http.MaxParallelDownloads);
        var tasks = new List<Task>();
        foreach (var account in accountData.Accounts)
        {
            await semaphore.WaitAsync(cancellationToken);
            var t = Task.Run(async () =>
            {
                try
                {
                    foreach (var skin in account.Skins)
                        await _skinService.FetchSkinsAsync(settings.Launcher.CacheDirectoryPath, account.Id,
                            account.Uuid,
                            skin, cancellationToken);
                    var capes = account.MojangProfile?.Capes ?? [];
                    if (capes.Count > 0)
                        await _skinService.FetchCapesAsync(settings.Launcher.CacheDirectoryPath, capes,
                            cancellationToken);
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken);
            tasks.Add(t);
        }

        await Task.WhenAll(tasks);
        return shouldRefreshCache;
    }
}
