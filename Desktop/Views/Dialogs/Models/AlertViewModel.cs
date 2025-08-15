using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

/// <summary>
/// Represents the ViewModel for an alert dialog, providing properties for title, message, 
/// alert type, and icon details. Inherits from KonkordObservableObject.
/// </summary>
public partial class AlertViewModel : KonkordObservableObject
{
    private AlertWindow? _parentWindow;
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _message;
    [ObservableProperty] private EAlertType _alertType;
    
    [ObservableProperty] private string _getIconColor;
    [ObservableProperty] private string _getIcon;
    [ObservableProperty] private bool _hasCancelButton = false;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AlertViewModel"/> class with the specified parameters.
    /// Configures the alert dialog properties such as title, message, alert type, icon, and icon color.
    /// </summary>
    /// <param name="parentWindow">The parent window associated with this alert dialog.</param>
    /// <param name="title">The title of the alert dialog.</param>
    /// <param name="message">The message displayed in the alert dialog.</param>
    /// <param name="type">The type of alert, represented by the <see cref="EAlertType"/> enumeration.</param>
    public AlertViewModel(AlertWindow parentWindow, string title, string message, EAlertType type)
    {
        _parentWindow = parentWindow;
        _title = title;
        _message = message;
        _alertType = type;

        switch (_alertType)
        {
            case EAlertType.Success:
                _getIcon = "\uf058";
                _getIconColor = "success";
                break;
            case EAlertType.Warning:
                _getIcon = "\uf071";
                _getIconColor = "warning";
                _hasCancelButton = true;
                break;
            case EAlertType.Error:
                _getIcon = "\uf06a";
                _getIconColor = "error";
                _hasCancelButton = true;
                break;
            case EAlertType.Confirm:
                _getIcon = "\uf059";
                _getIconColor = "confirm";
                _hasCancelButton = true;
                break;
            default:
            case EAlertType.Info:
                _getIcon = "\uf05a";
                _getIconColor = "info";
                break;
        }
    }

    /// <summary>
    /// Frees memory resources by resetting the title, message, icon, and icon color properties.
    /// </summary>
    public override void FreeMemory()
    {
        Title = string.Empty;
        Message = string.Empty;
        GetIcon = string.Empty;
        GetIconColor = string.Empty;
        _parentWindow = null;
    }
    
    /// <summary>
    /// Closes the parent alert window with the specified result value.
    /// </summary>
    /// <param name="value">The result value indicating the outcome of the alert dialog.</param>
    [RelayCommand]
    public void Close(bool value) => _parentWindow?.Close(value);
}