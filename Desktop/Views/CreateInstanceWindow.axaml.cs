using Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views;

/// <summary>
/// Represents the window for creating a new instance in the application.
/// Initializes the UI components, attaches Avalonia Dev Tools in debug mode,
/// and sets the DataContext to a new <see cref="CreateInstanceViewModel"/>.
/// </summary>
public partial class CreateInstanceWindow : KonkordWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateInstanceWindow"/> class.
    /// Sets up the UI components, attaches dev tools in debug mode, and assigns the DataContext.
    /// </summary>
    public CreateInstanceWindow()
    {
        InitializeComponent();

#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif

        this.DataContext = new CreateInstanceViewModel(this);
    }

    protected override void FreeMemory()
    {

    }
}