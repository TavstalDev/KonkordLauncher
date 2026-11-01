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
///     A dialog window for exporting an instance.
/// </summary>
public partial class ExportWindow : KonkordWindow<ExportViewModel>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ExportWindow" /> class for the designer.
    /// </summary>
    public ExportWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExportWindow" /> class
    ///     with the given view model.
    /// </summary>
    /// <param name="viewModel">The view model that holds the window data.</param>
    /// <param name="bitmapService">The service used to load bitmaps.</param>
    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public ExportWindow(ExportViewModel viewModel, IBitmapService bitmapService) : base(bitmapService)
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;

        DataContext = viewModel;

        this.WhenActivated(disposables =>
        {
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
        });
    }
}
