using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

public partial class AlertViewModel : KonkordObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _message;
    [ObservableProperty] private EAlertType _alertType;
    
    [ObservableProperty] private string _getIconColor;
    [ObservableProperty] private string _getIcon;
    [ObservableProperty] private bool _hasCancelButton;
    
    public Interaction<Unit, Unit> MinimizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> MaximizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();

    public AlertViewModel(string title, string message, EAlertType type)
    {
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
    
    #region Window Commands
    [RelayCommand]
    public async Task MinimizeWindow()
    {
        await MinimizeWindowInteraction.Handle(Unit.Default);
    }

    [RelayCommand]
    public async Task MaximizeWindow()
    {
        await MaximizeWindowInteraction.Handle(Unit.Default);
    }

    [RelayCommand]
    public async Task CloseWindow()
    {
        await CloseWindowInteraction.Handle(Unit.Default);
    }
    #endregion
}