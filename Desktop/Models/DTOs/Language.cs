using System.Globalization;
using System.Text.Json.Serialization;

namespace Tavstal.KonkordLauncher.Desktop.Models.DTOs;

/// <summary>
///     Holds information about a language.
/// </summary>
public class Language
{
    /// <summary>
    ///     Gets or sets the language name.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    ///     Gets or sets the two-letter language code.
    /// </summary>
    [JsonPropertyName("twoLetterCode")]
    public required string TwoLetterCode { get; set; }

    /// <summary>
    ///     Gets or sets the URL of the translation file.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether this is the default language.
    /// </summary>
    [JsonPropertyName("isDefault")]
    public bool IsDefault { get; set; }

    /// <summary>
    ///     Gets the culture for this language.
    /// </summary>
    /// <returns>The culture that matches the language code.</returns>
    public CultureInfo GetCultureInfo()
    {
        return new CultureInfo(TwoLetterCode);
    }
}
