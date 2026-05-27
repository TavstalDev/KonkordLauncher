using Tavstal.KonkordLauncher.Common.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Common.Helpers;

/// <summary>
/// Provides a static bridge to the active <see cref="ITranslationService"/> implementation.
/// </summary>
public class TranslationHelper
{
    /// <summary>
    /// Gets or sets the current translation service instance used for lookups.
    /// </summary>
    public static ITranslationService? Current { get; set; }

    /// <summary>
    /// Translates the specified key using the current translation service.
    /// </summary>
    /// <param name="key">The translation key to resolve.</param>
    /// <returns>The translated string if available; otherwise, the original <paramref name="key"/>.</returns>
    public static string Translate(string key)
        => Current?.Translate(key) ?? key;

    /// <summary>
    /// Translates the specified key and applies formatting arguments using the current translation service.
    /// </summary>
    /// <param name="key">The translation key to resolve.</param>
    /// <param name="args">Optional formatting arguments to apply to the resolved translation.</param>
    /// <returns>
    /// The translated and formatted string if available; otherwise, the original <paramref name="key"/>.
    /// </returns>
    public static string Translate(string key, params object[]? args)
        => Current?.Translate(key, args) ?? key;
}