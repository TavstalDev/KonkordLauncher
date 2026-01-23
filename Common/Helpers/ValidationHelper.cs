using System.Xml.Linq;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;
using Tavstal.KonkordLauncher.Core.Models.Endpoints.Modding;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge;

namespace Tavstal.KonkordLauncher.Common.Helpers;

/// <summary>
/// Provides helper methods for validating various launcher components, such as data folders, settings, accounts, and manifests.
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Logger instance for the ValidationHelper module.
    /// </summary>
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(ValidationHelper));

    /// <summary>
    /// Validates the existence of required data folders and creates them if they do not exist.
    /// </summary>
    /// <returns>True if all required folders are validated or created successfully, otherwise false.</returns>
    public static bool ValidateDataFolder()
    {
        try
        {
            if (!Directory.Exists(PathHelper.LauncherLogsDir))
                Directory.CreateDirectory(PathHelper.LauncherLogsDir);
            
            string logsFilePath = Path.Combine(PathHelper.LauncherLogsDir, string.Format(PathHelper.LogsFileFormat, CoreLogger.StartTime));
            if (!File.Exists(logsFilePath))
                File.Create(logsFilePath);
            
            if (!Directory.Exists(PathHelper.ApplicationDir))
                Directory.CreateDirectory(PathHelper.ApplicationDir);
            
            // Note: Also creates the config file if it does not exist
            var settings = LauncherHelper.GetLauncherSettings();
            
            if (!Directory.Exists(settings.Launcher.InstancesDirectoryPath))
                Directory.CreateDirectory(settings.Launcher.InstancesDirectoryPath);

            if (!Directory.Exists(settings.Launcher.IconsDirectoryPath))
                Directory.CreateDirectory(settings.Launcher.IconsDirectoryPath);
            
            if (!Directory.Exists(settings.Launcher.JavaDirectoryPath))
                Directory.CreateDirectory(settings.Launcher.JavaDirectoryPath);

            if (!Directory.Exists(settings.Launcher.TranslationsDirectoryPath))
                Directory.CreateDirectory(settings.Launcher.TranslationsDirectoryPath);

            if (!Directory.Exists(settings.Launcher.VersionsDirectoryPath))
                Directory.CreateDirectory(settings.Launcher.VersionsDirectoryPath);

            if (!Directory.Exists(settings.Launcher.CacheDirectoryPath))
                Directory.CreateDirectory(settings.Launcher.CacheDirectoryPath);
            
            string skinsDir = Path.Combine(settings.Launcher.CacheDirectoryPath, "skins");
            if (!Directory.Exists(skinsDir))
                Directory.CreateDirectory(skinsDir);

            if (!Directory.Exists(settings.Launcher.LibrariesDirectoryPath))
                Directory.CreateDirectory(settings.Launcher.LibrariesDirectoryPath);

            if (!Directory.Exists(settings.Launcher.AssetsDirectoryPath))
                Directory.CreateDirectory(settings.Launcher.AssetsDirectoryPath);

            string indexes = Path.Combine(settings.Launcher.AssetsDirectoryPath, "indexes");
            if (!Directory.Exists(indexes))
                Directory.CreateDirectory(indexes);

            if (!Directory.Exists(settings.Launcher.ManifestsDirectoryPath))
                Directory.CreateDirectory(settings.Launcher.ManifestsDirectoryPath);

            return true;
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to validate data folder:");
            _logger.Error(ex.ToString());
            return false;
        }
    }

    /// <summary>
    /// Validates the launcher accounts file and ensures it contains valid account data.
    /// </summary>
    /// <returns>True if the accounts file is validated successfully, otherwise false.</returns>
    public static async Task<bool> ValidateAccounts()
    {
        try
        {
            if (!File.Exists(PathHelper.LauncherAccountsPath))
            {
                AccountData accountData = new();

                await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherAccountsPath, accountData);
                return true; // No account was found to check
            }

            AccountData? data = await JsonHelper.ReadJsonFileAsync<AccountData>(PathHelper.LauncherAccountsPath);
            if (data == null)
            {
                _logger.Error("Failed to read accounts data, file is corrupted or empty.");
                return false;
            }

            var account = data.Accounts.FirstOrDefault(a => a.Id == data.SelectedAccountId);
            if (account == null)
                return true;

            switch (account.Type)
            {
                case EAccountType.MICROSOFT:
                {
                    return !string.IsNullOrEmpty(account.AccessToken);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to validate accounts:");
            _logger.Error(ex.ToString());
            return false;
        }
    }
    
    /// <summary>
    /// Validates and updates various manifests required by the launcher, such as Vanilla, Fabric, Forge, NeoForge, and Quilt.
    /// Downloads the manifests if they are missing or outdated.
    /// </summary>
    /// <param name="progressReporter">
    /// An optional progress reporter to report the download progress of the manifests.
    /// </param>
    public static async Task<bool> ValidateManifests(IProgressReporter? progressReporter = null)
    {
        try
        {
            using var httpClient = new HttpClient();
            var settings = await LauncherHelper.GetLauncherSettingsAsync();
            bool refreshManifests = DateTime.Now > settings.CacheRefreshDate;
            
            // Vanilla
            if (!File.Exists(settings.Launcher.GetVanillaManifestPath()) || refreshManifests)
            {
                Progress<double> progress = new Progress<double>();
                progress.ProgressChanged += (_, e) =>
                {
                    progressReporter?.SetStatusTranslated("startup.validation.manifests.download", "minecraft", e.ToString("0.00"));
                };
                await HttpHelper.DownloadFileAsync(MicrosoftEndpoints.MinecraftManifestUrl, settings.Launcher.GetVanillaManifestPath(), progress);
            }
            if (await ManifestHelper.GetMinecraftManifestAsync(settings.Launcher.GetVanillaManifestPath()) == null)
                _logger.Error("Failed to load Minecraft manifest");

            // Fabric
            if (!File.Exists(settings.Launcher.GetFabricManifestPath()) || refreshManifests)
            {
                Progress<double> progress = new Progress<double>();
                progress.ProgressChanged += (_, e) =>
                {
                    progressReporter?.SetStatusTranslated("startup.validation.manifests.download", "fabric", e.ToString("0.00"));
                };
                await HttpHelper.DownloadFileAsync(FabricEndpoints.VersionManifestUrl, settings.Launcher.GetFabricManifestPath(), progress);
            }
            if (await ManifestHelper.GetFabricManifestAsync(settings.Launcher.GetFabricManifestPath()) == null)
                _logger.Error("Failed to load Fabric manifest");

            // Forge
            if (!File.Exists(settings.Launcher.GetForgeManifestPath()) || refreshManifests)
            {
                string raw = await httpClient.GetStringAsync(ForgeEndpoints.VersionManifest);
                XDocument doc = XDocument.Parse(raw);
                XElement? metadata = doc.Element("metadata");
                if (metadata == null)
                {
                    _logger.Error("Forge manifest metadata not found in the XML.");
                    return false;
                }
                
                var versions = metadata
                    .Element("versioning")
                    ?.Element("versions")
                    ?.Elements("version")
                    .Select(v => v.Value);
                if (versions == null)
                {
                    _logger.Error("Forge manifest versions not found in the XML.");
                    return false;
                }

                List<ForgeManifest> manifest = [];
                foreach (var version in versions)
                {
                    var splittedVersion = version.Split('-');
                    manifest.Add(new ForgeManifest(splittedVersion[1], splittedVersion[0]));
                }
                
                await JsonHelper.WriteJsonFileAsync(settings.Launcher.GetForgeManifestPath(), manifest);
            }
            if (await ManifestHelper.GetForgeManifestAsync(settings.Launcher.GetForgeManifestPath()) == null)
                _logger.Error("Failed to load Forge manifest");
            
            
            // NeoForge
            if (!File.Exists(settings.Launcher.GetNeoForgeManifestPath()) || refreshManifests)
            {
                string raw = await httpClient.GetStringAsync(NeoForgeEndpoints.VersionManifest);
                XDocument doc = XDocument.Parse(raw);
                XElement? metadata = doc.Element("metadata");
                if (metadata == null)
                {
                    _logger.Error("Forge manifest metadata not found in the XML.");
                    return false;
                }
                
                var versions = metadata
                    .Element("versioning")
                    ?.Element("versions")
                    ?.Elements("version")
                    .Select(v => v.Value);
                if (versions == null)
                {
                    _logger.Error("Forge manifest versions not found in the XML.");
                    return false;
                }

                List<ForgeManifest> manifest = [];
                foreach (var version in versions)
                {
                    var parts = version.Split('.');
                    string gameVersion = $"1.{parts[0]}.{parts[1]}";
                    
                    manifest.Add(new ForgeManifest(version, gameVersion));
                }
                
                await JsonHelper.WriteJsonFileAsync(settings.Launcher.GetNeoForgeManifestPath(), manifest);
            }
            if (await ManifestHelper.GetNeoForgeManifestAsync(settings.Launcher.GetNeoForgeManifestPath()) == null)
                _logger.Error("Failed to load NeoForge manifest");

            // Quilt
            if (!File.Exists(settings.Launcher.GetQuiltManifestPath()) || refreshManifests)
            {
                Progress<double> progress = new Progress<double>();
                progress.ProgressChanged += (_, e) =>
                {
                    progressReporter?.SetStatusTranslated("startup.validation.manifests.download", "quilt", e.ToString("0.00"));
                };
                await HttpHelper.DownloadFileAsync(QuiltEndpoints.VersionManifestUrl, settings.Launcher.GetQuiltManifestPath(), progress);
            }
            if (await ManifestHelper.GetQuiltManifestAsync(settings.Launcher.GetQuiltManifestPath()) == null)
                _logger.Error("Failed to load Quilt manifest");

            return true;
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to validate manifests:");
            _logger.Error(ex.ToString());
            return false;
        }
    }
}