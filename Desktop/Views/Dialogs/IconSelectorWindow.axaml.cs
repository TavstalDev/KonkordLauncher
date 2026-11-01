using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Primitives;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using IconSelectorViewModel = Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models.IconSelectorViewModel;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

/// <summary>
///     A dialog window for choosing an icon.
/// </summary>
public partial class IconSelectorWindow : KonkordWindow<IconSelectorViewModel>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="IconSelectorWindow" /> class for the designer.
    /// </summary>
    public IconSelectorWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="IconSelectorWindow" /> class
    ///     with the given view model.
    /// </summary>
    /// <param name="viewModel">The view model that holds the window data.</param>
    /// <param name="bitmapService">The service used to load bitmaps.</param>
    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public IconSelectorWindow(IconSelectorViewModel viewModel, IBitmapService bitmapService) : base(bitmapService)
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
