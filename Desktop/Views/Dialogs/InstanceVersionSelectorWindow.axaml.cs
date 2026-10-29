using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Primitives;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

/// <summary>
///     Represents a window for selecting the version of an instance.
/// </summary>
public partial class InstanceVersionSelectorWindow : KonkordWindow<InstanceVersionSelectorViewModel>
{
    public InstanceVersionSelectorWindow()
    {
        InitializeComponent();
    }

    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public InstanceVersionSelectorWindow(InstanceVersionSelectorViewModel viewModel, IBitmapService bitmapService) : base(bitmapService)
    {
        InitializeComponent();
        DataContext = viewModel;

        if (Design.IsDesignMode)
            return;

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
