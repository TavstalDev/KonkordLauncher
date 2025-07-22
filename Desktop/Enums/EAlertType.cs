namespace Tavstal.KonkordLauncher.Desktop.Enums;

/// <summary>
/// Represents the different types of alerts that can be displayed in the application.
/// </summary>
public enum EAlertType
{
    /// <summary>
    /// Informational alert type, used for displaying general information.
    /// </summary>
    Info = 0,

    /// <summary>
    /// Success alert type, used for indicating successful operations.
    /// </summary>
    Success = 1,

    /// <summary>
    /// Warning alert type, used for cautionary messages.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Error alert type, used for displaying error messages.
    /// </summary>
    Error = 3,

    /// <summary>
    /// Confirmation alert type, used for requesting user confirmation.
    /// </summary>
    Confirm = 4
}