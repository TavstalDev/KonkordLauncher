using System.Globalization;

using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Common.Models;

/// <summary>
/// Represents a language with its associated metadata and functionality.
/// </summary>
public class Language
{
    /// <summary>
    /// Gets or sets the name of the language.
    /// </summary>
    [JsonProperty("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the two-letter ISO 639-1 code for the language.
    /// </summary>
    [JsonProperty("twoLetterCode")]
    public required string TwoLetterCode { get; set; }

    /// <summary>
    /// Gets or sets the URL for the language's translation file.
    /// </summary>
    [JsonProperty("url")]
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this language is the default language.
    /// </summary>
    [JsonProperty("isDefault")]
    public bool IsDefault { get; set; }

    /// <summary>
    /// Gets the <see cref="CultureInfo"/> object associated with the language's two-letter code.
    /// </summary>
    /// <returns>A <see cref="CultureInfo"/> object for the language.</returns>
    public CultureInfo GetCultureInfo()
    {
        return new CultureInfo(TwoLetterCode);
    }
}