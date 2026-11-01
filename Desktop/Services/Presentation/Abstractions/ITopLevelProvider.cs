using Avalonia.Controls;

namespace Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;

/// <summary>
///     Provides the current top-level window.
/// </summary>
public interface ITopLevelProvider
{
    /// <summary>
    ///     Gets the active top-level window.
    /// </summary>
    /// <returns>The active top level, or <see langword="null" /> if none is set.</returns>
    TopLevel? Get();

    /// <summary>
    ///     Sets the active top-level window.
    /// </summary>
    /// <param name="topLevel">The window to use.</param>
    void SetActive(TopLevel? topLevel);

    /// <summary>
    ///     Clears the active window if it is the given one.
    /// </summary>
    /// <param name="topLevel">The window to compare with the active window.</param>
    void ClearIfMatches(TopLevel? topLevel);
}
