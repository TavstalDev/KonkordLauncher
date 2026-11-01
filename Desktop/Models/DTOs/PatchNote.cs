using System.Text.Json.Serialization;

namespace Tavstal.KonkordLauncher.Desktop.Models.DTOs;

/// <summary>
///     Holds a patch note with a title, content, and URL.
/// </summary>
public class PatchNote
{
    /// <summary>
    ///     Creates a new patch note.
    /// </summary>
    /// <param name="title">The title of the patch note.</param>
    /// <param name="content">The content of the patch note.</param>
    /// <param name="url">The URL with more details.</param>
    public PatchNote(string title, string content, string url)
    {
        Title = title;
        var safeMarkdown = content ?? string.Empty;
        Content = safeMarkdown;
        Url = url;
    }

    /// <summary>
    ///     Gets or sets the title of the patch note.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; }

    /// <summary>
    ///     Sets the title from the alternate JSON field.
    /// </summary>
    [JsonPropertyName("tag_name")]
    [JsonInclude]
    public string TitleAlternate
    {
        set => Title = value;
    }

    /// <summary>
    ///     Gets or sets the content of the patch note.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; }

    /// <summary>
    ///     Sets the content from the alternate JSON field.
    /// </summary>
    [JsonPropertyName("body")]
    [JsonInclude]
    public string ContentAlternate
    {
        set => Content = value;
    }

    /// <summary>
    ///     Gets or sets the URL with more details.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; }

    /// <summary>
    ///     Sets the URL from the alternate JSON field.
    /// </summary>
    [JsonPropertyName("html_url")]
    [JsonInclude]
    public string UrlAlternate
    {
        set => Url = value;
    }
}
