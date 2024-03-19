using KonkordLauncher.API.Enums;
using KonkordLauncher.API.Managers;
using KonkordLauncher.API.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace KonkordLauncher.API.Helpers
{
    public static class IOHelper
    {
        private static readonly string _appDataRoamingDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string AppDataRoamingDir { get {  return _appDataRoamingDir; } }

        private static readonly string _mainDir = Path.Combine(_appDataRoamingDir, ".konkordlauncher");
        public static string MainDirectory { get { return _mainDir; } }

        private static readonly string _instancesDir = Path.Combine(_mainDir, "instances");
        public static string InstancesDir { get { return _instancesDir; } }

        private static readonly string _translationsDir = Path.Combine(_mainDir, "translations");
        public static string TranslationsDir { get { return _translationsDir; } }

        private static readonly string _versionsDir = Path.Combine(_mainDir, "versions");
        public static string VersionsDir { get { return _versionsDir; } }
        private static readonly string _mainfestDir = Path.Combine(_mainDir, "manifests");
        public static string MainfestDir { get { return _mainfestDir; } }

        private static readonly string _cacheDir = Path.Combine(_mainDir, "cache");
        public static string CacheDir { get { return _cacheDir; } }

        private static readonly string _librariesDir = Path.Combine(_mainDir, "libraries");
        public static string LibrariesDir { get { return _librariesDir; } }

        private static readonly string _assetsDir = Path.Combine(_mainDir, "assets");
        public static string AssetsDir { get { return _assetsDir; } }

        public static LauncherSettings? GetLauncherSettings()
        {
            FileStream stream = File.OpenRead(Path.Combine(MainDirectory, "launcher.json"));
            if (stream == null)
                return null;

            try
            {
                LauncherSettings? settings = JsonSerializer.Deserialize<LauncherSettings>(stream);
                stream.Close();
                return settings;
            }
            catch (Exception ex)
            {
                NotificationHelper.SendError(ex.ToString(), "Error in GetLauncherSettings");
                stream.Close();
                return null;
            }
        }

        public static async Task<bool> ValidateJava()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "java.exe";
                psi.Arguments = " -version";
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;

                Process? pr = Process.Start(psi);
                if (pr == null)
                {
                    throw new NullReferenceException($"The process to check the java version was null.");
                }
                else
                {
                    string javaVersion = pr.StandardError.ReadLine().Split(' ')[2].Replace("\"", "");

                    string majorVersion = javaVersion.Split(".")[0];
                    int majorV = int.Parse(majorVersion);
                    if (majorV < 17)
                    {
                        NotificationHelper.SendWarning($"Found an unrecommended version of java ({javaVersion}), we recommend you to use java 17+.", "Java Version");
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                NotificationHelper.SendError(ex.ToString(), "Error in ValidateJava");
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
                }

                return true;
            }
            catch (Exception ex)
            {
                NotificationHelper.SendError(ex.ToString(), "Error in ValidateDataFolder");
                return false;
            }
        }

        public static async Task<bool> ValidateSettings()
        {
            try
            {
                string filePath = Path.Combine(MainDirectory, "launcher.json");
                LauncherSettings? settings = null;
                if (!File.Exists(filePath))
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
                        await File.WriteAllTextAsync(filePath, content);
                    }
                    return true;
                }

                return true;
            }
            catch (Exception ex)
            {
                NotificationHelper.SendError(ex.ToString(), "Error in ValidateSettings");
                return false;
            }
        }

        public static async Task<bool> ValidateAccounts()
        {
            try
            {
                string path = Path.Combine(MainDirectory, "accounts.json");

                if (!File.Exists(path))
                {
                    AccountData accountData = new AccountData()
                    {
                        SelectedAccountId = "",
                        MojanClientToken = "",
                        Accounts = new Dictionary<string, Account> { }
                    };

                    await JsonHelper.WriteJsonFile(path, accountData);
                    return true; // No account was found to check
                }

                AccountData? data = await JsonHelper.ReadJsonFile<AccountData>(path);
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
                                return !(string.IsNullOrEmpty(account.AccessToken) && string.IsNullOrEmpty(account.RefreshToken));
                            }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                NotificationHelper.SendError(ex.ToString(), "Error in ValidateAccounts");
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
                    await TranslationManager.SaveTranslationAsync(defaultTranslationFile, TranslationManager.DefaultTranslations);
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
                                await TranslationManager.SaveTranslationAsync(localePath, TranslationManager.DefaultTranslations);
                                TranslationManager.SetTranslations(TranslationManager.DefaultTranslations);
                            }
                        }
                        else
                        {
                            NotificationHelper.SendError($"Failed to read the translation file of '{locale}'. Loading defaults...", "Error");
                            TranslationManager.SetTranslations(TranslationManager.DefaultTranslations);
                        }    
                    }
                    else
                    {
                        if (TranslationManager.LanguagePacks.ContainsKey(locale))
                        {
                            HttpClient client = new HttpClient();

                            string resultJson = await client.GetStringAsync(TranslationManager.LanguagePacks[locale]);
                            Dictionary<string, string> translation = TranslationManager.DefaultTranslations;
                            using (var stream = new MemoryStream())
                            {
                                translation = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream);
                            }

                            await TranslationManager.SaveTranslationAsync(localePath, translation ?? TranslationManager.DefaultTranslations);
                            TranslationManager.SetTranslations(translation);
                        }

                        
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                NotificationHelper.SendError(ex.ToString(), "Error in ValidateTranslations");
                return false;
            }
        }
    
        public static async Task<bool> ValidateManifests() {
            if (!Directory.Exists(_mainfestDir))
                Directory.CreateDirectory(_mainfestDir);

            #region Vanilla
            string path = Path.Combine(_mainfestDir, "vanillaManifest.json");
            if (!File.Exists(path))
            {
                using (var httpClient = new HttpClient())
                {
                    string? json = await httpClient.GetStringAsync(GameManager.MCVerisonManifestUrl);
                    if (json != null)
                        File.WriteAllText(path, json);
                }
            }
            #endregion

            #region Fabric
            path = Path.Combine(_mainfestDir, "fabricManifest.json");
            if (!File.Exists(path))
            {
                using (var httpClient = new HttpClient())
                {
                    string? json = await httpClient.GetStringAsync(GameManager.FabricVersionManifestUrl);
                    if (json != null)
                        File.WriteAllText(path, json);
                }
            }
            #endregion

            #region Forge & NeoForge
            path = Path.Combine(_mainfestDir, "forgeManifest.json");
            if (!File.Exists(path))
            {
                using (var httpClient = new HttpClient())
                {
                    string? json = await httpClient.GetStringAsync(GameManager.ForgeVersionManifest);
                    if (json != null)
                        File.WriteAllText(path, json);
                }
            }
            #endregion

            #region Quilt
            path = Path.Combine(_mainfestDir, "quiltManifest.json");
            if (!File.Exists(path))
            {
                using (var httpClient = new HttpClient())
                {
                    string? json = await httpClient.GetStringAsync(GameManager.QuiltVersionManifestUrl);
                    if (json != null)
                        File.WriteAllText(path, json);
                }
            }
            #endregion

            return true;
        }
    }
}
