using Tavstal.KonkordLauncher.Desktop.Models.Enums;

namespace Tavstal.KonkordLauncher.Desktop.Models.Domain;

/// <summary>
///     An alert with a title, message, and type.
/// </summary>
public class Alert
{
    /// <summary>
    ///     Creates an alert.
    /// </summary>
    /// <param name="title">The alert title.</param>
    /// <param name="message">The alert message.</param>
    /// <param name="type">The alert type.</param>
    public Alert(string title, string message, EAlertType type)
    {
        Title = title;
        Message = message;
        Type = type;
    }

    /// <summary>
    ///     Gets or sets the alert title.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///     Gets or sets the alert message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    ///     Gets or sets the alert type.
    /// </summary>
    public EAlertType Type { get; set; }
}
