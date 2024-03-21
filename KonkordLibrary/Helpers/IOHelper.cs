using KonkordLibrary.Enums;
using KonkordLibrary.Managers;
using KonkordLibrary.Models;
using System.Diagnostics;
using System.Text.Json;
using System.IO;
using System.Net.Http;

namespace KonkordLibrary.Helpers
{
    public static class IOHelper
    {
        #region Directories
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
        private static readonly string _manifestDir = Path.Combine(_mainDir, "manifests");
        public static string ManifestDir { get { return _manifestDir; } }

        private static readonly string _cacheDir = Path.Combine(_mainDir, "cache");
        public static string CacheDir { get { return _cacheDir; } }

        private static readonly string _librariesDir = Path.Combine(_mainDir, "libraries");
        public static string LibrariesDir { get { return _librariesDir; } }

        private static readonly string _assetsDir = Path.Combine(_mainDir, "assets");
        public static string AssetsDir { get { return _assetsDir; } }
        #endregion

        #region Files
        private static readonly string _launcherJsonFile = Path.Combine(_mainDir, "launcher.json");
        public static string LauncherJsonFile { get { return _launcherJsonFile; } }
        private static readonly string _accountsJsonFile = Path.Combine(_mainDir, "accounts.json");
        public static string AccountsJsonFile { get { return _accountsJsonFile; } }

        private static readonly string _vanillaManifestJsonFile = Path.Combine(_manifestDir, "vanillaManifest.json");
        public static string VanillaManifesJsonFile { get { return _vanillaManifestJsonFile; } }
        private static readonly string _forgeManifestJsonFile = Path.Combine(_manifestDir, "forgeManifest.json");
        public static string ForgeManifestJsonFile { get { return _forgeManifestJsonFile; } }
        private static readonly string _fabricManifestJsonFile = Path.Combine(_manifestDir, "fabricManifest.json");
        public static string FabricManifestJsonFile { get { return _fabricManifestJsonFile; } }
        private static readonly string _quiltManifestJsonFile = Path.Combine(_manifestDir, "quiltManifest.json");
        public static string QuiltManifestJsonFile { get { return _quiltManifestJsonFile; } }
        #endregion

        #region Validating
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
                    throw new NullReferenceException($"The process to check the java version was null.");
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
                NotificationHelper.SendError(ex.ToString(), "Error in ValidateSettings");
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
                        MojanClientToken = "",
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
                            Dictionary<string, string> translation = new Dictionary<string, string>();
                            using (var stream = new MemoryStream())
                            {
                                translation = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream) ?? TranslationManager.DefaultTranslations;
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
            if (!Directory.Exists(_manifestDir))
                Directory.CreateDirectory(_manifestDir);

            using (var httpClient = new HttpClient())
            {
                #region Vanilla
                if (!File.Exists(_vanillaManifestJsonFile))
                {

                    string? json = await httpClient.GetStringAsync(GameManager.MCVerisonManifestUrl);
                    if (json != null)
                        File.WriteAllText(_vanillaManifestJsonFile, json);
                }
                #endregion

                #region Fabric
                if (!File.Exists(_fabricManifestJsonFile))
                {
                    string? json = await httpClient.GetStringAsync(GameManager.FabricVersionManifestUrl);
                    if (json != null)
                        File.WriteAllText(_fabricManifestJsonFile, json);
                }
                #endregion

                #region Forge & NeoForge
                if (!File.Exists(_forgeManifestJsonFile))
                {

                    string? json = await httpClient.GetStringAsync(GameManager.ForgeVersionManifest);
                    if (json != null)
                        File.WriteAllText(_forgeManifestJsonFile, json);
                }
                #endregion

                #region Quilt
                if (!File.Exists(_quiltManifestJsonFile))
                {
                    string? json = await httpClient.GetStringAsync(GameManager.QuiltVersionManifestUrl);
                    if (json != null)
                        File.WriteAllText(_quiltManifestJsonFile, json);
                }
                #endregion
            }

            return true;
        }
        #endregion

        #region Functions
        public static LauncherSettings? GetLauncherSettings()
        {
            return JsonHelper.ReadJsonFile<LauncherSettings?>(_launcherJsonFile);
        }

        public static async Task<LauncherSettings?> GetLauncherSettingsAsync()
        {
            return await JsonHelper.ReadJsonFileAsync<LauncherSettings?>(_launcherJsonFile);
        }

        public static AccountData? GetAccountData()
        {
            return JsonHelper.ReadJsonFile<AccountData?>(_accountsJsonFile);
        }

        public static async Task<AccountData?> GetAccountDataAsync()
        {
            return await JsonHelper.ReadJsonFileAsync<AccountData?>(_accountsJsonFile);
        }
        #endregion
    }
}
