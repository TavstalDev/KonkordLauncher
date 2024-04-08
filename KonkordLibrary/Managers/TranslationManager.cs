using KonkordLibrary.Helpers;
using KonkordLibrary.Models;

namespace KonkordLibrary.Managers
{
    public static class TranslationManager
    {
        private static Dictionary<string, string> _translations = new Dictionary<string, string>();
        private static bool _initialized = false;

        public static Dictionary<string, string> Translations { get { return _translations; } }

        private static readonly Dictionary<string, string> _defaultTranslations = new Dictionary<string, string>()
        {
            { "messagebox_information", "Information" },
            { "messagebox_warning", "Warning" },
            { "messagebox_error", "Error" },
            { "error_unknown", "Unknown error." },
            { "launcher_account_not_found", "Failed to get the account data." },
            { "httplistener_stop_not_valid", "Can't stop the HTTP listener because it is null." },
            { "httplistener_start_fail", "Can't start the agent to listen transaction" },
            { "httplistener_authcode_not_found", "Couldn't get the auth code key from the url." },
            { "httplistener_already_active", "The HTTP listener is already active." },
            { "manifest_file_not_found", "Failed to get the {0} manifest." },
            { "version_meta_invalid", "Failed to get the {0} version meta" },
            { "java_version_process_null", "The process used to check the java version was null." },
            { "json_token_not_found", "Failed to get the {0} json token." },
            { "minecraft_ownership_validate_fail", "Failed to confirm your ownership." },
            { "minecraft_profile_not_found", "Failed to get your minecraft profile." },
            { "username_is_empty", "You must provide an username." },
            { "username_too_short", "Your username must be equal or longer than 3 characters." },
            { "username_too_long", "Your username must be equal or shorter than 16 characters." },
            { "file_not_found", "Failed to get the {0} file." },
            { "launch_fail_account_details", "Could not launch the game because failed to get the account details." },
            { "launch_fail_current_account", "Could not launch the game because failed to get the current account." },
            { "launch_fail_settings", "Could not launch the game because failed to get the launcher settings." },
            { "launch_fail_invalid_profile", "Could not launch the game because an invalid profile is selected." },
            { "minecraft_version_unsupported", "The '{0}' minecraft version is not supported." },
            { "instance_name_empty", "You must provide the name of the instance." },
            { "version_empty", "You must select a version." },
            { "ui_reading_file", "Reading the {0} file..." },
            { "ui_building", "Building {0}... {1}%" },
            { "ui_finding_recommended_java", "Trying to get recommended java path..." },
            { "ui_reading_manifest", "Reading the {0} Manifest file..." },
            { "ui_creating_directories", "Creating directories..." },
            { "ui_downloading_installer", "Downloading the {0} installer..." },
            { "ui_extracting_installer", "Extracting the {0} installer..." },
            { "ui_checking_installer_libraries", "Checking {0} installer libraries..." },
            { "ui_adding_arguments", "Adding {0} arguments..." },
            { "ui_copying_jar", "Copying the {0} jar file..." },
            { "ui_saving_lib_cache", "Saving library size cache file..." },
            { "ui_library_download", "Downloading the '{0}' library... {1}%" },
            { "ui_checking_forge_universal", "Checking forge universal library file..." },
            { "ui_downloading_forge_universal", "Downloadig forge universal library file..." },
            { "ui_downloading_version_json", "Downloading the {0} version json file..." },
            { "ui_reading_version_json", "Reading the {0} version json file..." },
            { "ui_downloading_loader", "Downloading the {0} loader..." },
            { "ui_checking_loader_libraries", "Checking {0} loader libraries..." },
            { "ui_getting_launch_arguments", "Getting launch arguments..." },
            { "ui_downloading_version_jar", "Downloading the {0} version jar file..." },
            { "ui_checking_asset_index_json", "Checking the '{0}' assetIndex json file..." },
            { "ui_downloading_asset_index_json", "Downloading the '{0}' assetIndex json file..." },
            { "ui_reading_asset_index_jso", "Reading the '{0}' assetIndex json file..." },
            { "ui_checking_assets", "Checking assets..." },
            { "ui_downloading_assets", "Downloading assets... {0}%" },
            { "ui_checking_logging", "Checking logging file..." },
            { "ui_downloading_logging", "Downloading logging file..." },
            { "ui_checking_client_mappings", "Checking client mappings..." },
            { "ui_downloading_client_mappings", "Downloading client mappings..." },
            { "ui_checking_libraries", "Checking the libraries..." },
            { "ui_calculating_lib_size", "Calculating library sizes..." },
            { "ui_checking_natives", "Checking natives..." },
            { "ui_building_args", "Building arguments..." }
        };

        public static Dictionary<string, string> DefaultTranslations { get { return _defaultTranslations; } }

        private static List<Language> _languagePacks = new List<Language>()
        {
            new Language("English", "en", "eng", "", true),
            new Language("German", "de", "deu", ""),
            new Language("Hungarian", "hu", "hun", "")
        };
        public static List<Language> LanguagePacks { get { return _languagePacks; } }

        /// <summary>
        /// Sets translations for the application.
        /// </summary>
        /// <param name="translation">A <see cref="Dictionary{TKey, TValue}"/> containing the translation data, where the key represents the original text and the value represents the translated text. Pass null to clear existing translations.</param>
        public static void SetTranslations(Dictionary<string, string>? translation)
        {
            if (_initialized)
                return;

            if (translation == null)
                _translations = _defaultTranslations;
            else
                _translations = translation;

            _initialized = true;
        }

        /// <summary>
        /// Reads translations from the specified file asynchronously.
        /// </summary>
        /// <param name="path">The path to the translation file.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="Dictionary{TKey, TValue}"/> where the key represents the original text and the value represents the translated text.
        /// </returns>
        public static async Task<Dictionary<string, string>> ReadTranslationAsync(string path)
        {
            try
            {
                var result = await JsonHelper.ReadJsonFileAsync<Dictionary<string, string>>(path);
                if (result != null)
                    return result;

                return new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                NotificationHelper.SendErrorMsg(ex.ToString(), "Error in SaveTranslations");
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Saves the provided translations to the specified file asynchronously.
        /// </summary>
        /// <param name="path">The path to save the translation file.</param>
        /// <param name="translation">A <see cref="Dictionary{TKey, TValue}"/> containing the translation data, where the key represents the original text and the value represents the translated text.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="bool"/> value indicating whether the saving operation was successful.
        /// </returns>
        public static async Task<bool> SaveTranslationAsync(string path, Dictionary<string, string> translation)
        {
            try
            {
                return await JsonHelper.WriteJsonFileAsync(path, translation);
            }
            catch (Exception ex)
            {
                NotificationHelper.SendErrorMsg(ex.ToString(), "Error in SaveTranslations");
                return false;
            }
        }

        /// <summary>
        /// Translates the provided key to its corresponding text.
        /// </summary>
        /// <param name="key">The key to be translated.</param>
        /// <param name="args">Optional parameters to be formatted into the translated text.</param>
        /// <returns>
        /// A <see cref="string"/> representing the translated text.
        /// </returns>
        public static string Translate(string key, params object[]? args)
        {
            string result = string.Empty;

            if (Translations.ContainsKey(key))
            {
                result = string.Format(Translations[key], args);
            }
            else
            {
                NotificationHelper.SendErrorMsg($"The translations does not contain the '{key}' key.", "Error in SaveTranslations");
                return string.Empty;
            }

            return result;
        }

    }
}
