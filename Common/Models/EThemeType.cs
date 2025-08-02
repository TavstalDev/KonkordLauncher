namespace Tavstal.KonkordLauncher.Common.Models;

/// <summary>
/// Represents the theme type used in the application.
/// </summary>
public enum EThemeType
{
    /// <summary>
    /// Automatically selects the theme based on system settings or preferences.
    /// </summary>
    Automatic = 0,

    /// <summary>
    /// Represents the light theme.
    /// </summary>
    Light = 1,

    /// <summary>
    /// Represents the dark theme.
    /// </summary>
    Dark = 2,
}