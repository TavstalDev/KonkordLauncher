using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Primitives;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

public partial class InstanceLogsWindow : KonkordWindow<InstanceLogsViewModel>
{
    public InstanceLogsWindow()
    {
        InitializeComponent();
    }

    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public InstanceLogsWindow(InstanceLogsViewModel viewModel, IBitmapService bitmapService) : base(bitmapService)
    {
        InitializeComponent();
        DataContext = viewModel;

        if (Design.IsDesignMode)
            return;

        this.WhenActivated(disposables =>
        {
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.LogsScrollToEnd.RegisterHandler(action =>
            {
                LogsScrollViewer.Offset = new Vector(0, LogsScrollViewer.Extent.Height);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            });
        });
    }
}
