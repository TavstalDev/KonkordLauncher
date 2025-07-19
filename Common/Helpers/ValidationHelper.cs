using System.Diagnostics;
using System.Text.Json;
using Tavstal.KonkordLauncher.Common.Managers;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge;

namespace Tavstal.KonkordLauncher.Common.Helpers;

public static class ValidationHelper
{
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(ValidationHelper));
    
    // TODO: Probably should be moved to Desktop
    
    public static async Task<bool> ValidateJava()
    {
        try
        {
            await Task.Delay(1); // Disable warning, made async because it might be changed in the future
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "java";
            psi.Arguments = " --version";
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;

            Process? pr = Process.Start(psi);
            if (pr == null)
            {
                _logger.Error("Failed to start Java process. Is Java installed?");
                return false;
            }

            /*string javaVersion = pr.StandardError.ReadLine().Split(' ')[2].Replace("\"", "");
            string majorVersion = javaVersion.Split(".")[0];
            int majorV = int.Parse(majorVersion);*/
            return true;
        }
        catch (Exception ex)
        {
            _logger.Exc("Failed to validate Java:");
            _logger.Error(ex.ToString());
            return false;
        }

    }
    
    public static bool ValidateDataFolder()
    {
        try
        {
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
    
    public static async Task<bool> ValidateAccounts()
    {
        try
        {
            if (!File.Exists(PathHelper.LauncherAccountsPath))
            {
                AccountData accountData = new AccountData()
                {
                    SelectedAccountId = "",
                    Accounts = new Dictionary<string, Account> { }
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

    public static async Task<bool> ValidateTranslations()
    {
        try
        {
            LauncherSettings? settings = LauncherHelper.GetLauncherSettings();

            string defaultTranslationFile = Path.Combine(PathHelper.TranslationsDir, "eng.json");
            if (!File.Exists(defaultTranslationFile))
            {
                await TranslationManager.SaveTranslationAsync(defaultTranslationFile,
                    TranslationManager.DefaultTranslations);
            }
            else
            {
                string localRawTrans = await File.ReadAllTextAsync(defaultTranslationFile);
                var defaultTranslationsLocal = Newtonsoft.Json.JsonConvert
                    .DeserializeObject<Dictionary<string, string>>(localRawTrans);

                if (defaultTranslationsLocal == null ||
                    defaultTranslationsLocal.Count != TranslationManager.DefaultTranslations.Count)
                {
                    await TranslationManager.SaveTranslationAsync(defaultTranslationFile,
                        TranslationManager.DefaultTranslations);
                }
            }

            if (settings != null)
            {
                string localePath = Path.Combine(PathHelper.TranslationsDir, $"{settings.Language}.json");
                if (File.Exists(localePath))
                {
                    var localLocale = await TranslationManager.ReadTranslationAsync(localePath);
                    if (localLocale is { Count: > 0 })
                    {
                        if (TranslationManager.DefaultTranslations.Count != localLocale.Count)
                        {
                            var mixedLocalization =
                                new Dictionary<string, string>(TranslationManager.DefaultTranslations);
                            foreach (var entry in localLocale)
                            {
                                mixedLocalization[entry.Key] = entry.Value;
                            }

                            await TranslationManager.SaveTranslationAsync(localePath, mixedLocalization);
                            TranslationManager.SetTranslations(mixedLocalization);
                        }
                        else
                        {
                            TranslationManager.SetTranslations(localLocale);
                        }
                    }
                    else if (localLocale?.Count == 0 && settings.Language == "en")
                    {
                        await TranslationManager.SaveTranslationAsync(localePath,
                            TranslationManager.DefaultTranslations);
                        TranslationManager.SetTranslations(TranslationManager.DefaultTranslations);
                    }
                    else
                    {
                        _logger.Error("Failed to read translations from the file, using default translations.");
                        TranslationManager.SetTranslations(TranslationManager.DefaultTranslations);
                    }
                }
                else if (TranslationManager.LanguagePacks.Any(x => x.TwoLetterCode == settings.Language))
                {
                    var lang = TranslationManager.LanguagePacks.Find(x => x.TwoLetterCode == settings.Language);
                    if (lang != null)
                    {
                        string? resultJson = await HttpHelper.GetStringAsync(lang.Url);
                        var translation = JsonSerializer.Deserialize<Dictionary<string, string>>(resultJson)
                                          ?? TranslationManager.DefaultTranslations;

                        await TranslationManager.SaveTranslationAsync(localePath, translation);
                        TranslationManager.SetTranslations(translation);
                    }
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to validate translations:");
            _logger.Error(ex.ToString());
            return false;
        }
    }

    public static async Task<bool> ValidateManifests()
    {
        try
        {
            using var httpClient = new HttpClient();
            
            // Vanilla
            if (!File.Exists(PathHelper.VanillaManifestPath))
            {
                string? json = await httpClient.GetStringAsync(MicrosoftEndpoints.MinecraftManifestUrl);
                await File.WriteAllTextAsync(PathHelper.VanillaManifestPath, json);
            }
            
            // Fabric
            if (!File.Exists(PathHelper.FabricManifestPath))
            {
                string? json = await httpClient.GetStringAsync(FabricEndpoints.VersionManifestUrl);
                await File.WriteAllTextAsync(PathHelper.FabricManifestPath, json);
            }
            
            // Forge
            if (!File.Exists(PathHelper.ForgeManifestPath))
            {
                string? json = await httpClient.GetStringAsync(ForgeEndpoints.VersionManifest);
                await File.WriteAllTextAsync(PathHelper.ForgeManifestPath, json);
            }
            
            // Quilt
            if (!File.Exists(PathHelper.QuiltManifestPath))
            {
                string? json = await httpClient.GetStringAsync(QuiltEndpoints.VersionManifestUrl);
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