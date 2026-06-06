using System.Text.Json.Serialization;


namespace Tavstal.KonkordLauncher.Common.Models;

/// <summary>
/// Represents a patch note with a title, content, and a URL for more details.
/// </summary>
public class PatchNote
{
    /// <summary>
    /// Gets or sets the title of the patch note.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; }
    
    /// <summary>
    /// Gets or sets the title of the patch note. This is used for display purposes.
    /// </summary>
    [JsonPropertyName("tag_name")]
    [JsonInclude]
    public string TitleAlternate 
    { 
        set => Title = value; 
    }

    /// <summary>
    /// Gets or sets the content or description of the patch note.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; }

    /// <summary>
    /// Gets or sets the content or description of the patch note.
    /// </summary>
    [JsonPropertyName("body")]
    [JsonInclude]
    public string ContentAlternate
    {
        set => Content = value; 
    }

    /// <summary>
    /// Gets or sets the URL for more information about the patch note.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; }
    
    /// <summary>
    /// Gets or sets the URL for more information about the patch note.
    /// </summary>
    [JsonPropertyName("html_url")]
    [JsonInclude]
    public string UrlAlternate
    {
        set => Url = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PatchNote"/> class with the specified title, content, and URL.
    /// </summary>
    /// <param name="title">The title of the patch note.</param>
    /// <param name="content">The content or description of the patch note.</param>
    /// <param name="url">The URL for more information about the patch note.</param>
    public PatchNote(string title, string content, string url)
    {
        Title = title;
        string safeMarkdown = content ?? string.Empty;
        Content = Markdig.Markdown.ToHtml(safeMarkdown);
        Url = url;
    }
}