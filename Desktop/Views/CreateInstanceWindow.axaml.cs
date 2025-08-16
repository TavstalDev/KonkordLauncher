using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using ReactiveUI;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views;

public partial class CreateInstanceWindow : KonkordWindow<CreateInstanceViewModel>
{
    public CreateInstanceWindow()
    {
        InitializeComponent();

#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif

        DataContext = new CreateInstanceViewModel();
        this.WhenActivated(disposables =>
        {
            DataContext.CloseWindow.RegisterHandler(action =>
            {
                Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.ShowAlertDialog.RegisterHandler(async action =>
            {
                AlertWindow alertWindow = new(action.Input.Title, action.Input.Message, action.Input.Type);
                await alertWindow.ShowDialog(this);
                action.SetOutput(Unit.Default);
            }).DisposeWith(disposables);
            DataContext.ShowIconSelector.RegisterHandler(async action =>
            {
                IconSelectorWindow window = new();
                var result = await window.ShowDialog<IconDataModel>(this);
                action.SetOutput(result);
            }).DisposeWith(disposables);
        });
    }
}