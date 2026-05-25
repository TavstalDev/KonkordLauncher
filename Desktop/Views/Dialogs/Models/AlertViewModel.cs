using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

/// <summary>
/// ViewModel for the alert dialog window.
/// </summary>
public partial class AlertViewModel : KonkordObservableObject
{
    [ObservableProperty]
    public partial string Title { get; set; }

    [ObservableProperty]
    public partial string Message { get; set; }

    [ObservableProperty]
    public partial EAlertType AlertType { get; set; }

    [ObservableProperty] private string _getIconColor;
    [ObservableProperty]
    public partial string GetIcon { get; set; }

    [ObservableProperty]
    public partial bool HasCancelButton { get; set; }

    #region Interactions

    public Interaction<Unit, Unit> MinimizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> MaximizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> ClickYesInteraction { get; } = new();
    
    #endregion

    /// <summary>
    /// Creates a new alert view model and configures its icon/color based on the supplied type.
    /// </summary>
    /// <param name="title">Alert dialog title.</param>
    /// <param name="message">Alert dialog message body.</param>
    /// <param name="type">The alert type that controls styling and button visibility.</param>
    public AlertViewModel(string title, string message, EAlertType type)
    {
        Title = title;
        Message = message;
        AlertType = type;

        switch (AlertType)
        {
            case EAlertType.Success:
                GetIcon = "\uf058";
                _getIconColor = "success";
                break;
            case EAlertType.Warning:
                GetIcon = "\uf071";
                _getIconColor = "warning";
                HasCancelButton = true;
                break;
            case EAlertType.Error:
                GetIcon = "\uf06a";
                _getIconColor = "error";
                HasCancelButton = true;
                break;
            case EAlertType.Confirm:
                GetIcon = "\uf059";
                _getIconColor = "confirm";
                HasCancelButton = true;
                break;
            default:
            case EAlertType.Info:
                GetIcon = "\uf05a";
                _getIconColor = "info";
                break;
        }
    }
    
    #region Window Commands
    
    /// <summary>
    /// Requests the view to minimize the window.
    /// </summary>
    [RelayCommand]
    public async Task MinimizeWindow() => await MinimizeWindowInteraction.Handle(Unit.Default);

    /// <summary>
    /// Requests the view to maximize or restore the window.
    /// </summary>
    [RelayCommand]
    public async Task MaximizeWindow() => await MaximizeWindowInteraction.Handle(Unit.Default);

    /// <summary>
    /// Requests the view to close the window.
    /// </summary>
    [RelayCommand]
    public async Task CloseWindow() => await CloseWindowInteraction.Handle(Unit.Default);

    /// <summary>
    /// Signals that the user clicked the confirmation ("Yes") button.
    /// </summary>
    [RelayCommand]
    public async Task YesButtonClick() => await ClickYesInteraction.Handle(Unit.Default);

    #endregion
}