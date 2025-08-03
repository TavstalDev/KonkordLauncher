using ReactiveUI;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

/// <summary>
/// ViewModel for managing alert dialogs in the application.
/// </summary>
public class AlertViewModel : ViewModelBase
{
    private string _title;
    private string _message = string.Empty;
    private EAlertType _alertType = EAlertType.Info;
    private string _acceptedButtonText = "OK";
    private string _cancelButtonText = "Cancel";

    /// <summary>
    /// Gets or sets the title of the alert dialog.
    /// </summary>
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    /// <summary>
    /// Gets or sets the message content of the alert dialog.
    /// </summary>
    public string Message
    {
        get => _message;
        set => this.RaiseAndSetIfChanged(ref _message, value);
    }

    /// <summary>
    /// Gets or sets the type of the alert, which determines its appearance and behavior.
    /// </summary>
    public EAlertType AlertType
    {
        get => _alertType;
        set => this.RaiseAndSetIfChanged(ref _alertType, value);
    }

    /// <summary>
    /// Gets or sets the text for the accepted button in the alert dialog.
    /// </summary>
    public string AcceptedButtonText
    {
        get => _acceptedButtonText;
        set => this.RaiseAndSetIfChanged(ref _acceptedButtonText, value);
    }

    /// <summary>
    /// Gets or sets the text for the cancel button in the alert dialog.
    /// </summary>
    public string CancelButtonText
    {
        get => _cancelButtonText;
        set => this.RaiseAndSetIfChanged(ref _cancelButtonText, value);
    }

    /// <summary>
    /// Gets the color associated with the alert type for the icon.
    /// </summary>
    public string GetIconColor
    {
        get
        {
            switch (_alertType)
            {
                case EAlertType.Success:
                    return "success";
                case EAlertType.Warning:
                    return "warning";
                case EAlertType.Error:
                    return "error";
                case EAlertType.Confirm:
                    return "confirm";
                default:
                case EAlertType.Info:
                    return "info";
            }
        }
    }

    /// <summary>
    /// Gets the icon associated with the alert type, represented as a FontAwesome Unicode string.
    /// </summary>
    public string GetIcon
    {
        get
        {
            switch (_alertType)
            {
                case EAlertType.Success:
                    return "\uf058";
                case EAlertType.Warning:
                    return "\uf071";
                case EAlertType.Error:
                    return "\uf06a";
                case EAlertType.Confirm:
                    return "\uf059";
                default:
                case EAlertType.Info:
                    return "\uf05a";
            }
        }
    }
}