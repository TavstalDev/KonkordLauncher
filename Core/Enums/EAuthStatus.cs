namespace Tavstal.KonkordLauncher.Core.Enums;

/// <summary>
/// Represents the status of an authentication process.
/// </summary>
public enum EAuthStatus
{
    /// <summary>
    /// Represents an undefined or uninitialized authentication status.
    /// </summary>
    NONE = 0,
    
    /// <summary>
    /// The authentication process is pending.
    /// </summary>
    PENDING = 1,

    /// <summary>
    /// The authentication process was successful.
    /// </summary>
    SUCCESS = 2,

    /// <summary>
    /// The authentication process failed.
    /// </summary>
    FAILED = 3,
}