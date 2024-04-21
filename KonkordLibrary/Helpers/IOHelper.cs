using Tavstal.KonkordLibrary.Enums;
using Tavstal.KonkordLibrary.Managers;
using Tavstal.KonkordLibrary.Models.Fabric;
using Tavstal.KonkordLibrary.Models.Forge;
using Tavstal.KonkordLibrary.Models.Installer;
using Tavstal.KonkordLibrary.Models.Launcher;
using Tavstal.KonkordLibrary.Models.Quilt;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using KonkordLibrary.Models.Launcher;

namespace Tavstal.KonkordLibrary.Helpers
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

        private static readonly string _tempDir = Path.Combine(_mainDir, "temp");
        public static string TempDir { get { return _tempDir; } }
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
        private static readonly string _skinLibJsonFile = Path.Combine(_mainDir, "skinLibrary.json");
        public static string SkinLibraryJsonFile { get { return _skinLibJsonFile; } }
        #endregion

        #region Validating
        /// <summary>
        /// Asynchronously validates the presence or status of Java installation.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="bool"/> value indicating whether Java is installed or not.
        /// </returns>
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
                NotificationHelper.SendErrorMsg(ex.ToString(), "Error in ValidateJava");
                return false;
            }

        }

        /// <summary>
        /// Validates the existence and integrity of the data folder.
        /// </summary>
        /// <returns>
        /// A <see cref="bool"/> value indicating whether the data folder is valid.
        /// </returns>
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
                    File.WriteAllText(SkinLibraryJsonFile, Newtonsoft.Json.JsonConvert.SerializeObject(new SkinLibData(), formatting: Newtonsoft.Json.Formatting.None));
                }

                return true;
            }
            catch (Exception ex)
            {
                NotificationHelper.SendErrorMsg(ex.ToString(), "Error in ValidateDataFolder");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously validates the application settings.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="bool"/> value indicating whether the settings are valid or not.
        /// </returns>
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

        /// <summary>
        /// Validates the user account file.
        /// </summary>
        /// <returns>
        /// A <see cref="bool"/> value indicating whether the user account file is valid.
        /// </returns>
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
                                return !(string.IsNullOrEmpty(account.AccessToken) && string.IsNullOrEmpty(account.RefreshToken));
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

        /// <summary>
        /// Asynchronously validates the translations for the application.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="bool"/> value indicating whether the translations are valid.
        /// </returns>
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
                else
                {
                    string localRawTrans = await File.ReadAllTextAsync(defaultTranslationFile);
                    Dictionary<string, string>? defaultTranslationsLocal = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(localRawTrans);
                    if (defaultTranslationsLocal == null || defaultTranslationsLocal.Count != TranslationManager.DefaultTranslations.Count)
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
                            // Left it untranslated because it is a translation error
                            NotificationHelper.SendErrorMsg($"Failed to read the translation file of '{locale}'. Loading defaults...", "Error");
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

                            string ? resultJson = await HttpHelper.GetStringAsync(lang.Url);
                            if (resultJson == null)
                                return false;
                            Dictionary<string, string> translation = new Dictionary<string, string>();
                            translation = JsonSerializer.Deserialize<Dictionary<string, string>>(resultJson) ?? TranslationManager.DefaultTranslations;

                            await TranslationManager.SaveTranslationAsync(localePath, translation ?? TranslationManager.DefaultTranslations);
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

        /// <summary>
        /// Asynchronously validates minecraft and modding framework manifests..
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="bool"/> value indicating whether the manifests are valid.
        /// </returns>
        public static async Task<bool> ValidateManifests() {
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
        #endregion

        #region Functions

        /// <summary>
        /// Deletes the directory and all its contents recursively.
        /// </summary>
        /// <param name="path">The path of the directory to delete.</param>
        public static void DeleteDirectory(string path)
        {
            var forgeInstallerDirInfo = new DirectoryInfo(path);
            foreach (FileInfo file in forgeInstallerDirInfo.GetFiles())
                file.Delete();
            foreach (DirectoryInfo subDirectory in forgeInstallerDirInfo.GetDirectories())
                subDirectory.Delete(true);
            Directory.Delete(path);
        }

        /// <summary>
        /// Moves a directory from the source to the destination directory.
        /// </summary>
        /// <param name="sourceDir">The path of the source directory.</param>
        /// <param name="destinationDir">The path of the destination directory.</param>
        /// <param name="recursive">A flag indicating whether to move the directory recursively.</param>
        /// <param name="deleteSource">A flag indicating whether to delete the source directory after moving.</param>
        public static void MoveDirectory(string sourceDir, string destinationDir, bool recursive, bool deleteSource = true, bool overwrite = true)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                if (overwrite || (!overwrite && !File.Exists(targetFilePath)))
                    file.CopyTo(targetFilePath, true);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    MoveDirectory(subDir.FullName, newDestinationDir, true, false);
                }
            }

            if (deleteSource)
                DeleteDirectory(sourceDir);
        }

        /// <summary>
        /// Checks the SHA1 hash of the file against a provided hash value, if provided.
        /// </summary>
        /// <param name="path">The path of the file to check.</param>
        /// <param name="compareHash">Optional: The SHA1 hash value to compare against.</param>
        /// <returns>
        /// True if the calculated SHA1 hash matches the provided hash value (if provided); otherwise, false.
        /// </returns>
        public static bool CheckSHA1(string path, string? compareHash)
        {
            if (string.IsNullOrEmpty(compareHash))
                return true;

            try
            {
                string fileHash = string.Empty;
                using (FileStream file = File.OpenRead(path))
                using (SHA1 hasher = SHA1.Create())
                {
                    var binaryHash = hasher.ComputeHash(file);
                    fileHash = BitConverter.ToString(binaryHash).Replace("-", "").ToLowerInvariant();
                }

                return fileHash == compareHash;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Retrieves the launcher settings, if available.
        /// </summary>
        /// <returns>
        /// A <see cref="LauncherSettings"/> object representing the launcher settings, or null if settings are not available.
        /// </returns>
        public static LauncherSettings? GetLauncherSettings()
        {
            return JsonHelper.ReadJsonFile<LauncherSettings?>(_launcherJsonFile);
        }

        /// <summary>
        /// Asynchronously retrieves the launcher settings, if available.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="LauncherSettings"/> object representing the launcher settings, or null if settings are not available.
        /// </returns>
        public static async Task<LauncherSettings?> GetLauncherSettingsAsync()
        {
            return await JsonHelper.ReadJsonFileAsync<LauncherSettings?>(_launcherJsonFile);
        }

        /// <summary>
        /// Retrieves the account data, if available.
        /// </summary>
        /// <returns>
        /// An <see cref="AccountData"/> object representing the account data, or null if data is not available.
        /// </returns>
        public static AccountData? GetAccountData()
        {
            return JsonHelper.ReadJsonFile<AccountData?>(_accountsJsonFile);
        }

        /// <summary>
        /// Asynchronously retrieves the account data, if available.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains an <see cref="AccountData"/> object representing the account data, or null if data is not available.
        /// </returns>
        public static async Task<AccountData?> GetAccountDataAsync()
        {
            return await JsonHelper.ReadJsonFileAsync<AccountData?>(_accountsJsonFile);
        }
        #endregion
    }
}
