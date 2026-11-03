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

/// <summary>
///     A dialog window that shows the log output of an instance.
/// </summary>
public partial class InstanceLogsWindow : KonkordWindow<InstanceLogsViewModel>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InstanceLogsWindow" /> class for the designer.
    /// </summary>
    public InstanceLogsWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstanceLogsWindow" /> class
    ///     with the given view model.
    /// </summary>
    /// <param name="viewModel">The view model that holds the window data.</param>
    /// <param name="bitmapService">The service used to load bitmaps.</param>
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
                LogsScrollViewer.Scroll!.Offset = new Vector(0, LogsScrollViewer.Scroll!.Extent.Height);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            });
        });
    }
}
