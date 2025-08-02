using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;

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
            if (!Directory.Exists(PathHelper.ApplicationDir))
                Directory.CreateDirectory(PathHelper.ApplicationDir);
            
            // Note: Also creates the config file if it does not exist
            var settings = LauncherHelper.GetLauncherSettings();
            
            if (!Directory.Exists(settings.Launcher.InstancesDirectoryPath))
                Directory.CreateDirectory(settings.Launcher.InstancesDirectoryPath);

            if (!Directory.Exists(settings.Launcher.IconsDirectoryPath))
                Directory.CreateDirectory(settings.Launcher.IconsDirectoryPath);
            
            if (!Directory.Exists(settings.Launcher.TranslationsDirectoryPath))
                Directory.CreateDirectory(settings.Launcher.TranslationsDirectoryPath);

            if (!Directory.Exists(settings.Launcher.VersionsDirectoryPath))
                Directory.CreateDirectory(settings.Launcher.VersionsDirectoryPath);

            if (!Directory.Exists(settings.Launcher.CacheDirectoryPath))
                Directory.CreateDirectory(settings.Launcher.CacheDirectoryPath);

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
                AccountData accountData = new AccountData()
                {
                    SelectedAccountId = "",
                    Accounts = new Dictionary<string, Account>()
                };

                await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherAccountsPath, accountData);
                return true; // No account was found to check
            }

            AccountData? data = await JsonHelper.ReadJsonFileAsync<AccountData>(PathHelper.LauncherAccountsPath);
            if (data == null)
            {
                _logger.Error("Failed to read accounts data, file is corrupted or empty.");
                return false;
            }

            if (data.Accounts.TryGetValue(data.SelectedAccountId, out Account? account))
            {
                switch (account.Type)
                {
                    case EAccountType.OFFLINE:
                    {
                        return true;
                    }
                    case EAccountType.MICROSOFT:
                    {
                        return !string.IsNullOrEmpty(account.AccessToken);
                    }
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
    /// Validates the existence of required manifest files and downloads them if they do not exist.
    /// </summary>
    /// <returns>True if all required manifest files are validated or downloaded successfully, otherwise false.</returns>
    public static async Task<bool> ValidateManifests()
    {
        try
        {
            using var httpClient = new HttpClient();
            var settings = await LauncherHelper.GetLauncherSettingsAsync();

            // Vanilla
            if (!File.Exists(settings.Launcher.GetVanillaManifestPath()))
            {
                string json = await httpClient.GetStringAsync(MicrosoftEndpoints.MinecraftManifestUrl);
                await File.WriteAllTextAsync(settings.Launcher.GetVanillaManifestPath(), json);
            }

            // Fabric
            if (!File.Exists(settings.Launcher.GetFabricManifestPath()))
            {
                string json = await httpClient.GetStringAsync(FabricEndpoints.VersionManifestUrl);
                await File.WriteAllTextAsync(settings.Launcher.GetFabricManifestPath(), json);
            }

            // Forge
            if (!File.Exists(settings.Launcher.GetForgeManifestPath()))
            {
                string json = await httpClient.GetStringAsync(ForgeEndpoints.VersionManifest);
                await File.WriteAllTextAsync(settings.Launcher.GetForgeManifestPath(), json);
            }
            
            // NeoForge
            if (!File.Exists(settings.Launcher.GetNeoForgeManifestPath()))
            {
                string json = await httpClient.GetStringAsync(NeoForgeEndpoints.VersionManifest);
                await File.WriteAllTextAsync(settings.Launcher.GetNeoForgeManifestPath(), json);
            }

            // Quilt
            if (!File.Exists(settings.Launcher.GetQuiltManifestPath()))
            {
                string json = await httpClient.GetStringAsync(QuiltEndpoints.VersionManifestUrl);
                await File.WriteAllTextAsync(settings.Launcher.GetQuiltManifestPath(), json);
            }

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