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
    void ReportProgress(double progress);

    /// <summary>
    /// Sets the status message.
    /// </summary>
    /// <param name="status">The status message to display.</param>
    void UpdateStatus(string status);

    /// <summary>
    /// Sets the status message using a translation key and optional arguments.
    /// </summary>
    /// <param name="key">The translation key for the status message.</param>
    /// <param name="args">Optional arguments for formatting the status message.</param>
    void UpdateStatusTranslated(string key, params object[]? args);

    /// <summary>
    /// Displays the progress reporter.
    /// </summary>
    void OpenReporter();

    /// <summary>
    /// Hides the progress reporter.
    /// </summary>
    void CloseReporter();
}