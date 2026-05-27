namespace Tavstal.KonkordLauncher.Common.Services.Abstractions;

/// <summary>
/// Provides translation / localization lookup for application strings.
/// </summary>
public interface ITranslationService
{
    /// <summary>
    /// Changes the active application language.
    /// </summary>
    /// <param name="language">The target language code or identifier to switch to (for example, <c>"en"</c>.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the language change operation.</param>
    /// <returns>A task that completes when the language change has been applied.</returns>
    Task ChangeLanguageAsync(string language, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Translate the specified resource <paramref name="key"/> to the active language's string.
    /// </summary>
    /// <param name="key">The translation key (e.g. "instance.launch.title"). Must not be <c>null</c>.</param>
    /// <returns>The localized string for the provided <paramref name="key"/>.</returns>
    string Translate(string key);
    
    /// <summary>
    /// Translate the specified resource <paramref name="key"/> and apply composite-formatting with <paramref name="args"/>.
    /// </summary>
    /// <param name="key">The translation key (e.g. "instance.launch.progress"). Must not be <c>null</c>.</param>
    /// <param name="args">Optional format arguments to be applied to the translated string.</param>
    /// <returns>The formatted localized string.</returns>
    string Translate(string key, params object[]? args);
}