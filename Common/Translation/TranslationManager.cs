using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Common.Translation;

/// <summary>
/// Manages translations for the application, including initialization, retrieval, and fallback handling.
/// </summary>
public static class TranslationManager
{
    /// <summary>
    /// Logger instance for the TranslationManager module.
    /// </summary>
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(TranslationManager));

    /// <summary>
    /// Gets the fallback translations from the default translation provider.
    /// </summary>
    private static Dictionary<string, string> _fallbackTranslations => DefaultTranslationProvider.Translations;

    /// <summary>
    /// Stores the current translations for the selected language.
    /// </summary>
    private static Dictionary<string, string>? _translations = new();

    /// <summary>
    /// Stores the current language code.
    /// </summary>
    private static string? _currentLanguage;

    /// <summary>
    /// Indicates whether the translations have been initialized.
    /// </summary>
    private static bool _initialized;

    /// <summary>
    /// Initializes the translations by loading them from local files or remote sources.
    /// </summary>
    /// <param name="progressReporter">Optional progress reporter to update the status during initialization.</param>
    public static async Task InitializeTranslations(IProgressReporter? progressReporter = null)
    {
        if (_initialized)
            return;

        try
        {
            progressReporter?.SetStatus("Initializing translations...");
            _initialized = true;
            var settings = await LauncherHelper.GetLauncherSettingsAsync();

            progressReporter?.SetStatus("Reading current translations...");
            string locale = settings.Launcher.Language;
            string localePath = Path.Combine(settings.Launcher.TranslationsDirectoryPath, $"{locale}.json");
            if (File.Exists(localePath))
            {
                var localTranslations = await ReadTranslationAsync(localePath);
                if (localTranslations == null)
                {
                    _logger.Error("Failed to read local translation file.");
                    progressReporter?.SetStatus($"Failed to read '{locale}.json' translation file.");
                    return;
                }

                _translations = localTranslations;
                _currentLanguage = locale;
                return;
            }

            var languagePack = LanguagePackProvider.LanguagePacks.Find(x => x.TwoLetterCode == locale);
            if (languagePack == null)
            {
                _logger.Warn("Language pack not found for the current locale.");
                progressReporter?.SetStatus($"Language pack not found for '{locale}' locale.");
                return;
            }

            string? resultJson = await HttpHelper.GetStringAsync(languagePack.Url);
            if (resultJson == null)
            {
                _logger.Warn("Failed to fetch translations from the URL.");
                progressReporter?.SetStatus($"Failed to fetch translations from '{languagePack.Url}'.");
                return;
            }

            Dictionary<string, string>? translation = JsonConvert.DeserializeObject<Dictionary<string, string>>(resultJson);
            if (translation == null)
            {
                _logger.Error("Failed to deserialize translations from the URL.");
                progressReporter?.SetStatus($"Failed to deserialize translations from '{languagePack.Url}'.");
                return;
            }

            _currentLanguage = locale;
            _translations = translation;
            progressReporter?.SetStatus("Saving translations...");
            await SaveTranslationAsync(localePath, translation);
        }
        catch (Exception ex)
        {
            _logger.Exc("Failed to validate translations.");
            _logger.Error(ex);
        }
    }

    /// <summary>
    /// Sets the translations for a specific language.
    /// </summary>
    /// <param name="language">The language code.</param>
    /// <param name="translation">The dictionary of translations for the language.</param>
    public static void SetTranslations(string language, Dictionary<string, string>? translation)
    {
        if (!_initialized)
            return;

        _currentLanguage = language;
        _translations = translation ?? DefaultTranslationProvider.Translations;
    }
    
    /// <summary>
    /// Ensures that the language file for the specified language exists locally.
    /// If the file does not exist, it attempts to download and save it from a remote source.
    /// </summary>
    /// <param name="language">The language code for which the file should be ensured.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a boolean value:
    /// true if the language file exists or was successfully created, otherwise false.
    /// </returns>
    public static async Task<bool> EnsureLanguageFileExistsAsync(string language)
    {
        try
        {
            var settings = await LauncherHelper.GetLauncherSettingsAsync();
            string localePath = Path.Combine(settings.Launcher.TranslationsDirectoryPath, $"{language}.json");
            if (File.Exists(localePath))
                return true;

            var languagePack = LanguagePackProvider.LanguagePacks.Find(x => x.TwoLetterCode == language);
            if (languagePack == null)
            {
                _logger.Warn("Language pack not found for the current locale.");
                return false;
            }

            string? resultJson = await HttpHelper.GetStringAsync(languagePack.Url);
            if (resultJson == null)
            {
                _logger.Warn("Failed to fetch translations from the URL.");
                return false;
            }

            Dictionary<string, string>? translation = JsonConvert.DeserializeObject<Dictionary<string, string>>(resultJson);
            if (translation == null)
            {
                _logger.Error("Failed to deserialize translations from the URL.");
                return false;
            }
            
            return await SaveTranslationAsync(localePath, translation);
        }
        catch (Exception ex)
        {
            _logger.Exc("Failed to ensure language file exists.");
            _logger.Error(ex);
            return false;
        }
    }

    /// <summary>
    /// Reads a translation file asynchronously.
    /// </summary>
    /// <param name="path">The file path to the translation file.</param>
    /// <returns>A dictionary of translations or null if an error occurs.</returns>
    public static async Task<Dictionary<string, string>?> ReadTranslationAsync(string path)
    {
        try
        {
            var result = await JsonHelper.ReadJsonFileAsync<Dictionary<string, string>>(path);
            return result ?? new Dictionary<string, string>();
        }
        catch (Exception ex)
        {
            _logger.Exc("Failed to read translation file.");
            _logger.Error(ex);
            return null;
        }
    }

    /// <summary>
    /// Saves a translation file asynchronously.
    /// </summary>
    /// <param name="path">The file path to save the translation file.</param>
    /// <param name="translation">The dictionary of translations to save.</param>
    /// <returns>True if the file was saved successfully, otherwise false.</returns>
    public static async Task<bool> SaveTranslationAsync(string path, Dictionary<string, string> translation)
    {
        try
        {
            return await JsonHelper.WriteJsonFileAsync(path, translation);
        }
        catch (Exception ex)
        {
            _logger.Exc("Failed to save translation file.");
            _logger.Error(ex);
            return false;
        }
    }

    /// <summary>
    /// Translates a key using the current translations or falls back to the default translations.
    /// </summary>
    /// <param name="key">The translation key.</param>
    /// <param name="args">Optional arguments for formatting the translation string.</param>
    /// <returns>The translated string.</returns>
    public static string Translate(string key, params object[]? args)
    {
        if (_translations == null || _currentLanguage == null)
            return FallbackTranslate(key, args);

        if (!_translations.ContainsKey(key))
        {
            _logger.Warn($"Translation key '{key}' not found for the '{_currentLanguage}' language.");
            return FallbackTranslate(key, args);
        }

        try
        {
            return args == null || args.Length == 0 ? _translations[key] : string.Format(_translations[key], args);
        }
        catch (Exception ex)
        {
            _logger.Exc($"Unknown error while formatting translation for the '{key}' key.");
            _logger.Error(ex);
            return string.Empty;
        }
    }

    /// <summary>
    /// Provides a fallback translation for a key if it is not found in the current translations.
    /// </summary>
    /// <param name="key">The translation key.</param>
    /// <param name="args">Optional arguments for formatting the translation string.</param>
    /// <returns>The fallback translated string.</returns>
    private static string FallbackTranslate(string key, params object[]? args)
    {
        if (!_fallbackTranslations.ContainsKey(key))
        {
            _logger.Error("Fallback translation key '{key}' not found.");
            return string.Empty;
        }

        try
        {
            return args == null || args.Length == 0
                ? _fallbackTranslations[key]
                : string.Format(_fallbackTranslations[key], args);
        }
        catch (Exception ex)
        {
            _logger.Exc($"Unknown error while formatting fallback translation for the '{key}' key.");
            _logger.Error(ex);
            return string.Empty; 
        }
    }
}