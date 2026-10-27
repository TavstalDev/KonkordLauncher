using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Primitives;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using IconSelectorViewModel = Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models.IconSelectorViewModel;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

public partial class IconSelectorWindow : KonkordWindow<IconSelectorViewModel>
{
    public IconSelectorWindow()
    {
        InitializeComponent();
    }

    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public IconSelectorWindow(IconSelectorViewModel viewModel)
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;

        DataContext = viewModel;

        this.WhenActivated(disposables =>
        {
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close(action.Input);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
        });
    }
}
