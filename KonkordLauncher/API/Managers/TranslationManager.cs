using KonkordLauncher.API.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace KonkordLauncher.API.Managers
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

        public static async Task<Dictionary<string, string>?> ReadTranslationAsync(string path)
        {
            try
            {
                return await JsonHelper.ReadJsonFile<Dictionary<string, string>>(path);
            }
            catch (Exception ex)
            {
                NotificationHelper.SendError(ex.ToString(), "Error in SaveTranslations");
                return null;
            }
        }

        public static async Task<bool> SaveTranslationAsync(string path, Dictionary<string, string> translation)
        {
            try
            {
                return await JsonHelper.WriteJsonFile(path, translation);
            }
            catch (Exception ex)
            {
                NotificationHelper.SendError(ex.ToString(), "Error in SaveTranslations");
                return false;
            }
        }

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
