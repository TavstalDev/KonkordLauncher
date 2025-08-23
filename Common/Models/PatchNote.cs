namespace Tavstal.KonkordLauncher.Common.Models;

/// <summary>
/// Represents a patch note with a title, content, and a URL for more details.
/// </summary>
public class PatchNote
{
    /// <summary>
    /// Gets or sets the title of the patch note.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the content or description of the patch note.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Gets or sets the URL for more information about the patch note.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PatchNote"/> class with the specified title, content, and URL.
    /// </summary>
    /// <param name="title">The title of the patch note.</param>
    /// <param name="content">The content or description of the patch note.</param>
    /// <param name="url">The URL for more information about the patch note.</param>
    public PatchNote(string title, string content, string url)
    {
        Title = title;
        Content = content;
        Url = url;
    }
}