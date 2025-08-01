namespace Tavstal.KonkordLauncher.Core.Models;

/// <summary>
/// Represents a progress reporter interface for tracking and displaying progress and status updates.
/// </summary>
public interface IProgressReporter
{
    /// <summary>
    /// Sets the progress value.
    /// </summary>
    /// <param name="progress">The progress value as a double, typically between 0.0 and 1.0.</param>
    void SetProgress(double progress);

    /// <summary>
    /// Sets the status message.
    /// </summary>
    /// <param name="status">The status message to display.</param>
    void SetStatus(string status);

    /// <summary>
    /// Sets the status message using a translation key and optional arguments.
    /// </summary>
    /// <param name="statusKey">The translation key for the status message.</param>
    /// <param name="args">Optional arguments for formatting the status message.</param>
    void SetStatusTranslated(string statusKey, params object[]? args);

    /// <summary>
    /// Displays the progress reporter.
    /// </summary>
    void Show();

    /// <summary>
    /// Hides the progress reporter.
    /// </summary>
    void Hide();
}