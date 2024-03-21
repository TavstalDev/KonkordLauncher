using KonkordLibrary.Helpers;

namespace KonkordLibrary.Managers
{
    public static class TranslationManager
    {
        private static Dictionary<string, string> _translations = new Dictionary<string, string>();
        private static bool _initialized = false;

        public static Dictionary<string, string> Translations { get { return _translations; } }

        private static readonly Dictionary<string, string> _defaultTranslations = new Dictionary<string, string>()
        {
            { "", "" },
        };

        public static Dictionary<string, string> DefaultTranslations { get { return _defaultTranslations; } }

        private static Dictionary<string, string> _languagePacks = new Dictionary<string, string>()
        {

        };
        public static Dictionary<string, string> LanguagePacks { get { return _languagePacks; } }

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
                NotificationHelper.SendError(ex.ToString(), "Error in SaveTranslations");
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
                NotificationHelper.SendError(ex.ToString(), "Error in SaveTranslations");
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
                NotificationHelper.SendError($"The translations does not contain the '{key}' key.", "Error in SaveTranslations");
                return string.Empty;
            }

            return result;
        }

    }
}
