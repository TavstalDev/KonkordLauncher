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
        new(
            "English",
            "en",
            "https://raw.githubusercontent.com/TavstalDev/KonkordLauncher/master/KonkordLauncher/assets/translations/default.json",
            true
        ),
        new(
            "German",
            "de",
            "https://raw.githubusercontent.com/TavstalDev/KonkordLauncher/master/KonkordLauncher/assets/translations/german.json"
        ),
        new(
            "Hungarian",
            "hu",
            "https://raw.githubusercontent.com/TavstalDev/KonkordLauncher/master/KonkordLauncher/assets/translations/hungarian.json"
        )
    ];

    /// <summary>
    /// Gets the list of available language packs.
    /// </summary>
    public static List<Language> LanguagePacks => _languagePacks;
}