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
///     A dialog window that asks the user for text input.
/// </summary>
public partial class InputWindow : KonkordWindow<InputViewModel>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InputWindow" /> class for the designer.
    /// </summary>
    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public InputWindow()
    {
        InitializeComponent();
        DataContext = new InputViewModel("Design Time Title");
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InputWindow" /> class with the given title.
    /// </summary>
    /// <param name="title">The title shown in the window.</param>
    /// <param name="bitmapService">The service used to load bitmaps.</param>
    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public InputWindow(string title, IBitmapService bitmapService) : base(bitmapService)
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = new InputViewModel(title);
            return;
        }

        // Sets the DataContext to an instance of InputViewModel with the provided title.
        DataContext = new InputViewModel(title);
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
