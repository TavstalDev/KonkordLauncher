using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using JavaSelectorViewModel = Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models.JavaSelectorViewModel;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

/// <summary>
/// Represents a window for selecting a Java version.
/// </summary>
public partial class JavaSelectorWindow : KonkordWindow<JavaSelectorViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JavaSelectorWindow"/> class.
    /// Sets up the data context and handles language changes.
    /// </summary>
    public JavaSelectorWindow()
    {
        InitializeComponent();

        DataContext = new JavaSelectorViewModel();
        
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