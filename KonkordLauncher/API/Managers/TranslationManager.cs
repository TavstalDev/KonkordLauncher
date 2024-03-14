using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonkordLauncher.API.Managers
{
    public static class TranslationManager
    {
        private static Dictionary<string, string> _translations = new Dictionary<string ,string>();
        private static bool _initialized = false;

        public static Dictionary<string, string> Translations { get { return _translations; } }

        public static void Init(string language)
        {
            if (_initialized)
                return;

            

            _initialized = true;
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
                throw new NullReferenceException($"The translations does not contain the '{key}' key.");
            }

            return result;
        }

    }
}
