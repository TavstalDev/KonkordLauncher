using Tavstal.KonkordLauncher.Common.Models;

namespace Tavstal.KonkordLauncher.Common.Translation;

/// <summary>
/// Provides a collection of language packs available for the application.
/// </summary>
public static class LanguagePackProvider
{
    /// <summary>
    /// A list of available language packs.
    /// </summary>
    private static readonly List<Language> _languagePacks = 
    [
        new()
        {
            Name = "English",
            TwoLetterCode = "en",
            Url = "https://raw.githubusercontent.com/TavstalDev/KonkordLauncher/master/KonkordLauncher/assets/translations/default.json",
            IsDefault = true
        },
        new()
        {
            Name = "German",
            TwoLetterCode = "de",
            Url = "https://raw.githubusercontent.com/TavstalDev/KonkordLauncher/master/KonkordLauncher/assets/translations/german.json",
        },
        new()
        {
            Name = "Hungarian",
            TwoLetterCode = "hu",
            Url = "https://raw.githubusercontent.com/TavstalDev/KonkordLauncher/master/KonkordLauncher/assets/translations/hungarian.json"
        }
    ];

    /// <summary>
    /// Gets the list of available language packs.
    /// </summary>
    public static List<Language> LanguagePacks => _languagePacks;
}