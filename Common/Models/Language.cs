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
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the two-letter ISO 639-1 code for the language.
    /// </summary>
    [JsonProperty("twoLetterCode")]
    public string TwoLetterCode { get; set; }

    /// <summary>
    /// Gets or sets the URL for the language's translation file.
    /// </summary>
    [JsonProperty("url")]
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this language is the default language.
    /// </summary>
    [JsonProperty("isDefault")]
    public bool IsDefault { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Language"/> class.
    /// </summary>
    public Language() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Language"/> class with the specified name, codes, and URL.
    /// </summary>
    /// <param name="name">The name of the language.</param>
    /// <param name="twoLetterCode">The two-letter ISO 639-1 code for the language.</param>
    /// <param name="url">The URL for the language's translation file.</param>
    public Language(string name, string twoLetterCode, string url)
    {
        Name = name;
        TwoLetterCode = twoLetterCode;
        Url = url;
        IsDefault = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Language"/> class with the specified name, codes, URL, and default status.
    /// </summary>
    /// <param name="name">The name of the language.</param>
    /// <param name="twoLetterCode">The two-letter ISO 639-1 code for the language.</param>
    /// <param name="url">The URL for the language's translation file.</param>
    /// <param name="isDefault">A value indicating whether this language is the default language.</param>
    public Language(string name, string twoLetterCode, string url, bool isDefault)
    {
        Name = name;
        TwoLetterCode = twoLetterCode;
        Url = url;
        IsDefault = isDefault;
    }

    /// <summary>
    /// Gets the <see cref="CultureInfo"/> object associated with the language's two-letter code.
    /// </summary>
    /// <returns>A <see cref="CultureInfo"/> object for the language.</returns>
    public CultureInfo GetCultureInfo()
    {
        return new CultureInfo(TwoLetterCode);
    }
}