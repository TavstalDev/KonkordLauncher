using System.Diagnostics;
using System.Text.Json;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Managers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Fabric;
using Tavstal.KonkordLauncher.Core.Models.Forge;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.Launcher;
using Tavstal.KonkordLauncher.Core.Models.Quilt;

namespace Tavstal.KonkordLauncher.Core.Helpers;

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
            psi.FileName = "java.exe";
            psi.Arguments = " -version";
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;

            Process? pr = Process.Start(psi);
            if (pr == null)
            {
                throw new NullReferenceException(TranslationManager.Translate($"java_version_process_null"));
            }
            else
            {
                /*string javaVersion = pr.StandardError.ReadLine().Split(' ')[2].Replace("\"", "");

                string majorVersion = javaVersion.Split(".")[0];
                int majorV = int.Parse(majorVersion);
                if (majorV < 17)
                {
                    NotificationHelper.SendWarning($"Found an unrecommended version of java ({javaVersion}), we recommend you to use java 17+.", "Java Version");
                }*/
                return true;
            }
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
            if (!Directory.Exists(MainDirectory))
            {
                Directory.CreateDirectory(MainDirectory);
                Directory.CreateDirectory(InstancesDir);
                Directory.CreateDirectory(TranslationsDir);
                Directory.CreateDirectory(VersionsDir);
                Directory.CreateDirectory(CacheDir);
                Directory.CreateDirectory(LibrariesDir);
                Directory.CreateDirectory(AssetsDir);
                Directory.CreateDirectory(Path.Combine(AssetsDir, "indexes"));
                Directory.CreateDirectory(TempDir);
            }
            else
            {
                if (!Directory.Exists(InstancesDir))
                    Directory.CreateDirectory(InstancesDir);

                if (!Directory.Exists(TranslationsDir))
                    Directory.CreateDirectory(TranslationsDir);

                if (!Directory.Exists(VersionsDir))
                    Directory.CreateDirectory(VersionsDir);

                if (!Directory.Exists(CacheDir))
                    Directory.CreateDirectory(CacheDir);

                if (!Directory.Exists(LibrariesDir))
                    Directory.CreateDirectory(LibrariesDir);

                if (!Directory.Exists(AssetsDir))
                    Directory.CreateDirectory(AssetsDir);

                string indexes = Path.Combine(AssetsDir, "indexes");
                if (!Directory.Exists(indexes))
                    Directory.CreateDirectory(indexes);

                if (!Directory.Exists(TempDir))
                    Directory.CreateDirectory(TempDir);
            }

            if (!File.Exists(SkinLibraryJsonFile))
            {
                File.WriteAllText(SkinLibraryJsonFile,
                    Newtonsoft.Json.JsonConvert.SerializeObject(new SkinLibData(),
                        formatting: Newtonsoft.Json.Formatting.None));
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.
            return false;
        }
    }
    
    public static async Task<bool> ValidateSettings()
    {
        try
        {
            LauncherSettings? settings = null;
            if (!File.Exists(LauncherJsonFile))
            {
                settings = new LauncherSettings();
                using (var stream = new MemoryStream())
                {
                    await JsonSerializer.SerializeAsync(stream, settings, options: new JsonSerializerOptions()
                    {
                        IgnoreReadOnlyFields = true,
                        IgnoreReadOnlyProperties = true,
                        WriteIndented = true

                    });
                    stream.Position = 0;
                    var reader = new StreamReader(stream);
                    string content = await reader.ReadToEndAsync();
                    await File.WriteAllTextAsync(LauncherJsonFile, content);
                }

                return true;
            }

            return true;
        }
        catch (Exception ex)
        {
            NotificationHelper.SendErrorMsg(ex.ToString(), "Error in ValidateSettings");
            return false;
        }
    }
    
    public static async Task<bool> ValidateAccounts()
    {
        try
        {
            if (!File.Exists(AccountsJsonFile))
            {
                AccountData accountData = new AccountData()
                {
                    SelectedAccountId = "",
                    Accounts = new Dictionary<string, Account> { }
                };

                await JsonHelper.WriteJsonFileAsync(AccountsJsonFile, accountData);
                return true; // No account was found to check
            }

            AccountData? data = await JsonHelper.ReadJsonFileAsync<AccountData>(AccountsJsonFile);
            if (data == null)
            {
                // It should not be null at all if the file exists.
                return true;
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
            NotificationHelper.SendErrorMsg(ex.ToString(), "Error in ValidateAccounts");
            return false;
        }
    }
    
    public static async Task<bool> ValidateTranslations()
    {
        try
        {
            LauncherSettings? settings = GetLauncherSettings();

            string defaultTranslationFile = Path.Combine(TranslationsDir, "en.json");
            if (!File.Exists(defaultTranslationFile))
            {
                await TranslationManager.SaveTranslationAsync(defaultTranslationFile,
                    TranslationManager.DefaultTranslations);
            }
            else
            {
                string localRawTrans = await File.ReadAllTextAsync(defaultTranslationFile);
                Dictionary<string, string>? defaultTranslationsLocal =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(localRawTrans);
                if (defaultTranslationsLocal == null ||
                    defaultTranslationsLocal.Count != TranslationManager.DefaultTranslations.Count)
                    await TranslationManager.SaveTranslationAsync(defaultTranslationFile,
                        TranslationManager.DefaultTranslations);
            }


            if (settings != null)
            {
                string locale = settings.Language;
                string localePath = Path.Combine(TranslationsDir, $"{locale}.json");
                if (File.Exists(localePath))
                {
                    var localLocale = await TranslationManager.ReadTranslationAsync(localePath);
                    if (localLocale != null)
                    {
                        if (localLocale.Count > 0)
                        {
                            if (TranslationManager.DefaultTranslations.Count - localLocale.Count != 0)
                            {
                                Dictionary<string, string> mixedLocalization = TranslationManager.DefaultTranslations;
                                foreach (var l in localLocale)
                                {
                                    if (mixedLocalization.ContainsKey(l.Key))
                                        mixedLocalization[l.Key] = l.Value;
                                    else
                                        mixedLocalization.Add(l.Key, l.Value);
                                }

                                await TranslationManager.SaveTranslationAsync(localePath, mixedLocalization);
                                TranslationManager.SetTranslations(mixedLocalization);
                            }
                            else
                                TranslationManager.SetTranslations(localLocale);

                        }
                        else if (localLocale.Count == 0 && locale == "en")
                        {
                            await TranslationManager.SaveTranslationAsync(localePath,
                                TranslationManager.DefaultTranslations);
                            TranslationManager.SetTranslations(TranslationManager.DefaultTranslations);
                        }
                    }
                    else
                    {
                        // Left it untranslated because it is a translation error
                        NotificationHelper.SendErrorMsg(
                            $"Failed to read the translation file of '{locale}'. Loading defaults...", "Error");
                        TranslationManager.SetTranslations(TranslationManager.DefaultTranslations);
                    }
                }
                else
                {
                    if (TranslationManager.LanguagePacks.Any(x => x.TwoLetterCode == locale))
                    {
                        Language? lang = TranslationManager.LanguagePacks.Find(x => x.TwoLetterCode == locale);
                        if (lang == null)
                            return false;

                        string? resultJson = await HttpHelper.GetStringAsync(lang.Url);
                        if (resultJson == null)
                            return false;
                        Dictionary<string, string> translation = new Dictionary<string, string>();
                        translation = JsonSerializer.Deserialize<Dictionary<string, string>>(resultJson) ??
                                      TranslationManager.DefaultTranslations;

                        await TranslationManager.SaveTranslationAsync(localePath,
                            translation ?? TranslationManager.DefaultTranslations);
                        TranslationManager.SetTranslations(translation);
                    }
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            NotificationHelper.SendErrorMsg(ex.ToString(), "Error in ValidateTranslations");
            return false;
        }
    }
    
    public static async Task<bool> ValidateManifests()
    {
        if (!Directory.Exists(_manifestDir))
            Directory.CreateDirectory(_manifestDir);

        using (var httpClient = new HttpClient())
        {
            #region Vanilla

            if (!File.Exists(_vanillaManifestJsonFile))
            {
                string? json = await httpClient.GetStringAsync(MinecraftInstaller.MCVerisonManifestUrl);
                if (json != null)
                    File.WriteAllText(_vanillaManifestJsonFile, json);
            }

            #endregion

            #region Fabric

            if (!File.Exists(_fabricManifestJsonFile))
            {
                string? json = await httpClient.GetStringAsync(FabricInstaller.FabricVersionManifestUrl);
                if (json != null)
                    File.WriteAllText(_fabricManifestJsonFile, json);
            }

            #endregion

            #region Forge & NeoForge

            if (!File.Exists(_forgeManifestJsonFile))
            {

                string? json = await httpClient.GetStringAsync(ForgeInstallerBase.ForgeVersionManifest);
                if (json != null)
                    File.WriteAllText(_forgeManifestJsonFile, json);
            }

            #endregion

            #region Quilt

            if (!File.Exists(_quiltManifestJsonFile))
            {
                string? json = await httpClient.GetStringAsync(QuiltInstaller.QuiltVersionManifestUrl);
                if (json != null)
                    File.WriteAllText(_quiltManifestJsonFile, json);
            }

            #endregion
        }

        return true;
    }
}