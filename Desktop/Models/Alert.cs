using Tavstal.KonkordLauncher.Desktop.Models.Enums;

namespace Tavstal.KonkordLauncher.Desktop.Models;

/// <summary>
/// Represents an alert with a title, message, and type.
/// </summary>
public class Alert
{
    /// <summary>
    /// Gets or sets the title of the alert.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the message of the alert.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the type of the alert.
    /// </summary>
    public EAlertType Type { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Alert"/> class with the specified title, message, and type.
    /// </summary>
    /// <param name="title">The title of the alert.</param>
    /// <param name="message">The message of the alert.</param>
    /// <param name="type">The type of the alert.</param>
    public Alert(string title, string message, EAlertType type)
    {
        Title = title;
        Message = message;
        Type = type;
    }
}