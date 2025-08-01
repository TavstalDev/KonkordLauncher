using System.Text.Json;
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
            
            if (!Directory.Exists(PathHelper.InstancesDir))
                Directory.CreateDirectory(PathHelper.InstancesDir);

            if (!Directory.Exists(PathHelper.TranslationsDir))
                Directory.CreateDirectory(PathHelper.TranslationsDir);

            if (!Directory.Exists(PathHelper.VersionsDir))
                Directory.CreateDirectory(PathHelper.VersionsDir);

            if (!Directory.Exists(PathHelper.CacheDir))
                Directory.CreateDirectory(PathHelper.CacheDir);

            if (!Directory.Exists(PathHelper.LibrariesDir))
                Directory.CreateDirectory(PathHelper.LibrariesDir);

            if (!Directory.Exists(PathHelper.AssetsDir))
                Directory.CreateDirectory(PathHelper.AssetsDir);

            string indexes = Path.Combine(PathHelper.AssetsDir, "indexes");
            if (!Directory.Exists(indexes))
                Directory.CreateDirectory(indexes);

            if (!Directory.Exists(PathHelper.ManifestDir))
                Directory.CreateDirectory(PathHelper.ManifestDir);

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
    /// Validates the launcher settings file and creates a default one if it does not exist.
    /// </summary>
    /// <returns>True if the settings file is validated or created successfully, otherwise false.</returns>
    public static async Task<bool> ValidateSettings()
    {
        try
        {
            if (!File.Exists(PathHelper.LauncherConfigPath))
            {
                var settings = new LauncherSettings();
                using var stream = new MemoryStream();
                await JsonSerializer.SerializeAsync(stream, settings, options: new JsonSerializerOptions()
                {
                    IgnoreReadOnlyFields = true,
                    IgnoreReadOnlyProperties = true,
                    WriteIndented = true
                });
                stream.Position = 0;
                var reader = new StreamReader(stream);
                string content = await reader.ReadToEndAsync();
                await File.WriteAllTextAsync(PathHelper.LauncherConfigPath, content);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to validate settings:");
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

            // Vanilla
            if (!File.Exists(PathHelper.VanillaManifestPath))
            {
                string json = await httpClient.GetStringAsync(MicrosoftEndpoints.MinecraftManifestUrl);
                await File.WriteAllTextAsync(PathHelper.VanillaManifestPath, json);
            }

            // Fabric
            if (!File.Exists(PathHelper.FabricManifestPath))
            {
                string json = await httpClient.GetStringAsync(FabricEndpoints.VersionManifestUrl);
                await File.WriteAllTextAsync(PathHelper.FabricManifestPath, json);
            }

            // Forge
            if (!File.Exists(PathHelper.ForgeManifestPath))
            {
                string json = await httpClient.GetStringAsync(ForgeEndpoints.VersionManifest);
                await File.WriteAllTextAsync(PathHelper.ForgeManifestPath, json);
            }

            // Quilt
            if (!File.Exists(PathHelper.QuiltManifestPath))
            {
                string json = await httpClient.GetStringAsync(QuiltEndpoints.VersionManifestUrl);
                await File.WriteAllTextAsync(PathHelper.QuiltManifestPath, json);
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