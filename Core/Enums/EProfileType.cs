namespace Tavstal.KonkordLauncher.Core.Enums;

/// <summary>
/// Represents the type of profile used in the application.
/// </summary>
public enum EProfileType
{
    /// <summary>
    /// Represents the latest release version of the profile.
    /// </summary>
    LATEST_RELEASE = 0,

    /// <summary>
    /// Represents the latest snapshot version of the profile.
    /// </summary>
    LATEST_SNAPSHOT = 1,

    /// <summary>
    /// Represents a custom profile type.
    /// </summary>
    CUSTOM = 2
}