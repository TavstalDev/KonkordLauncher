using System.Xml.Linq;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Serialization;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;
using Tavstal.KonkordLauncher.Core.Models.Endpoints.Modding;
using Tavstal.KonkordLauncher.Core.Models.Json;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Services.Abstractions.Auth;

namespace Tavstal.KonkordLauncher.Common.Services.Implementations;

/// <inheritdoc/>
public class ValidationService : IValidationService
{
    private readonly ICustomLogger _logger;
    private readonly IHttpService _httpService;
    private readonly ILauncherStore _launcherStore;
    private readonly IManifestService _manifestService;
    private readonly IMicrosoftAuthService _microsoftAuthService;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationService"/> class.
    /// </summary>
    /// <param name="logger">Logger used to record validation diagnostics and failures.</param>
    /// <param name="httpService">HTTP service used to download remote manifests and related resources.</param>
    /// <param name="launcherStore">Launcher store used to read and create launcher configuration data.</param>
    /// <param name="manifestService">Manifest service used to load and cache validated manifest data.</param>
    /// <param name="microsoftAuthService">Microsoft authentication service used to validate Microsoft accounts.</param>
    public ValidationService(ICustomLogger<ValidationService> logger, IHttpService httpService, ILauncherStore launcherStore, IManifestService manifestService, IMicrosoftAuthService microsoftAuthService)
    {
        _logger = logger;
        _httpService = httpService;
        _launcherStore = launcherStore;
        _manifestService = manifestService;
        _microsoftAuthService = microsoftAuthService;
    }
    
    /// <inheritdoc/>
    public async Task<bool> ValidateLauncherDirectoryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Directory.CreateDirectory(PathHelper.LauncherLogsDir);
            Directory.CreateDirectory(PathHelper.ApplicationDir);
            
            // Note: Also creates the config file if it does not exist
            var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);
            
            Directory.CreateDirectory(settings.Launcher.InstancesDirectoryPath);
            Directory.CreateDirectory(settings.Launcher.IconsDirectoryPath);
            Directory.CreateDirectory(settings.Launcher.JavaDirectoryPath);
            Directory.CreateDirectory(settings.Launcher.TranslationsDirectoryPath);
            Directory.CreateDirectory(settings.Launcher.VersionsDirectoryPath);
            Directory.CreateDirectory(settings.Launcher.CacheDirectoryPath);
            
            string skinsDir = Path.Combine(settings.Launcher.CacheDirectoryPath, "skins");
            Directory.CreateDirectory(skinsDir);

            Directory.CreateDirectory(settings.Launcher.LibrariesDirectoryPath);
            Directory.CreateDirectory(settings.Launcher.AssetsDirectoryPath);

            string indexes = Path.Combine(settings.Launcher.AssetsDirectoryPath, "indexes");
            Directory.CreateDirectory(indexes);

            Directory.CreateDirectory(settings.Launcher.ManifestsDirectoryPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to validate data folder:");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ValidateAccounts(CancellationToken cancellationToken = default)
    {
        try
        {
            AccountData data = await _launcherStore.GetAccountDataAsync(cancellationToken);
            bool shouldUpdate = false;
            foreach (var acc in data.Accounts)
            {
                if (acc.Type != EAccountType.MICROSOFT)
                    continue;

                if (acc.AccessTokenExpireDate < DateTime.Now)
                {
                    await _microsoftAuthService.RefreshLoginAsync(acc.GetRefreshToken(), cancellationToken);
                    acc.MojangProfile = _microsoftAuthService.MojangProfile;
                    acc.AccessTokenExpireDate = _microsoftAuthService.Account!.AccessTokenExpireDate;
                    acc.SetAccessToken(_microsoftAuthService.Account.GetAccessToken());
                    acc.SetRefreshToken(_microsoftAuthService.Account.GetRefreshToken());
                    _microsoftAuthService.Reset();
                    shouldUpdate = true;
                }
                else
                {
                    var mojangProfile =
                        await _microsoftAuthService.FetchMinecraftProfileAsync(acc.GetAccessToken(), cancellationToken);
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
                EAccountType.MICROSOFT => !string.IsNullOrEmpty(account.GetAccessToken()),
                _ => true
            };
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to validate accounts:");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ValidateManifests(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);
            bool refreshManifests = DateTime.Now > settings.CacheRefreshDate;
            
            // Vanilla
            if (!File.Exists(settings.Launcher.GetVanillaManifestPath()) || refreshManifests)
            {
                Progress<double> progress = new Progress<double>();
                progress.ProgressChanged += (_, e) =>
                {
                    progressReporter?.UpdateStatusTranslated("startup.validation.manifests.download", "minecraft", e.ToString("0.00"));
                };
                await _httpService.DownloadFileAsync(MicrosoftEndpoints.MinecraftManifestUrl, settings.Launcher.GetVanillaManifestPath(), progress, cancellationToken);
            }
            if (await _manifestService.GetMinecraftManifestAsync(settings.Launcher.GetVanillaManifestPath(), cancellationToken) == null)
                _logger.LogError("Failed to load Minecraft manifest");

            // Fabric
            if (!File.Exists(settings.Launcher.GetFabricManifestPath()) || refreshManifests)
            {
                Progress<double> progress = new Progress<double>();
                progress.ProgressChanged += (_, e) =>
                {
                    progressReporter?.UpdateStatusTranslated("startup.validation.manifests.download", "fabric", e.ToString("0.00"));
                };
                await _httpService.DownloadFileAsync(FabricEndpoints.VersionManifestUrl, settings.Launcher.GetFabricManifestPath(), progress, cancellationToken);
            }
            if (await _manifestService.GetFabricManifestAsync(settings.Launcher.GetFabricManifestPath(), cancellationToken) == null)
                _logger.LogError("Failed to load Fabric manifest");
            
            // Forge
            if (!File.Exists(settings.Launcher.GetForgeManifestPath()) || refreshManifests)
            {
                string? raw = await _httpService.GetStringAsync(ForgeEndpoints.VersionManifest, cancellationToken);
                if (raw == null)
                    throw new Exception("Failed to download Forge manifest, response was empty.");
                XDocument doc = XDocument.Parse(raw);
                XElement? metadata = doc.Element("metadata");
                if (metadata == null)
                {
                    _logger.LogError("Forge manifest metadata not found in the XML.");
                    return false;
                }
                
                var versions = metadata
                    .Element("versioning")
                    ?.Element("versions")
                    ?.Elements("version")
                    .Select(v => v.Value);
                if (versions == null)
                {
                    _logger.LogError("Forge manifest versions not found in the XML.");
                    return false;
                }

                List<ForgeManifest> manifest = [];
                foreach (var version in versions)
                {
                    var splittedVersion = version.Split('-');
                    manifest.Add(new ForgeManifest(splittedVersion[1], splittedVersion[0]));
                }
                
                await JsonHelper.WriteJsonFileAsync(settings.Launcher.GetForgeManifestPath(), manifest, CoreJsonContext.Default.ListForgeManifest, cancellationToken);
            }
            if (await _manifestService.GetForgeManifestAsync(settings.Launcher.GetForgeManifestPath(), cancellationToken) == null)
                _logger.LogError("Failed to load Forge manifest");
            
            
            // NeoForge
            try
            {
                if (!File.Exists(settings.Launcher.GetNeoForgeManifestPath()) || refreshManifests)
                {
                    string? raw = await _httpService.GetStringAsync(NeoForgeEndpoints.VersionManifest, cancellationToken);
                    if (raw == null)
                        throw new Exception("Failed to download NeoForge manifest, response was empty.");
                    XDocument doc = XDocument.Parse(raw);
                    XElement? metadata = doc.Element("metadata");
                    if (metadata == null)
                    {
                        _logger.LogError("Forge manifest metadata not found in the XML.");
                        return false;
                    }

                    var versions = metadata
                        .Element("versioning")
                        ?.Element("versions")
                        ?.Elements("version")
                        .Select(v => v.Value);
                    if (versions == null)
                    {
                        _logger.LogError("Forge manifest versions not found in the XML.");
                        return false;
                    }

                    List<ForgeManifest> manifest = [];
                    foreach (var version in versions)
                    {
                        var parts = version.Split('.');
                        string gameVersion = $"1.{parts[0]}.{parts[1]}";

                        manifest.Add(new ForgeManifest(version, gameVersion));
                    }

                    await JsonHelper.WriteJsonFileAsync(settings.Launcher.GetNeoForgeManifestPath(), manifest, CoreJsonContext.Default.ListForgeManifest, cancellationToken);
                }

                if (await _manifestService.GetNeoForgeManifestAsync(settings.Launcher.GetNeoForgeManifestPath()) == null)
                    _logger.LogError("Failed to load NeoForge manifest");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Failed to validate NeoForge manifest:");
                // Skipping due to known issues with the NeoForge manifest (as of 2026. 03. 09.)
            }

            // Quilt
            if (!File.Exists(settings.Launcher.GetQuiltManifestPath()) || refreshManifests)
            {
                Progress<double> progress = new Progress<double>();
                progress.ProgressChanged += (_, e) =>
                {
                    progressReporter?.UpdateStatusTranslated("startup.validation.manifests.download", "quilt", e.ToString("0.00"));
                };
                await _httpService.DownloadFileAsync(QuiltEndpoints.VersionManifestUrl, settings.Launcher.GetQuiltManifestPath(), progress, cancellationToken);
            }
            if (await _manifestService.GetQuiltManifestAsync(settings.Launcher.GetQuiltManifestPath(), cancellationToken) == null)
                _logger.LogError("Failed to load Quilt manifest");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to validate manifests:");
            return false;
        }
    }
}