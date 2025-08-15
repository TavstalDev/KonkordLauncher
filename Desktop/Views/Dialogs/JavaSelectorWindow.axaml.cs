using Avalonia;
using Avalonia.Controls;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models;
using JavaSelectorViewModel = Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models.JavaSelectorViewModel;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

/// <summary>
/// Represents a window for selecting a Java version.
/// </summary>
public partial class JavaSelectorWindow : KonkordWindow
{
    /// <summary>
    /// Logger instance for the JavaSelectorWindow class.
    /// </summary>
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(JavaSelectorWindow));

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaSelectorWindow"/> class.
    /// Sets up the data context and handles language changes.
    /// </summary>
    public JavaSelectorWindow()
    {
        InitializeComponent();

#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif

        if (Design.IsDesignMode)
            this.DataContext = new JavaSelectorViewModel(this);
        else
        {
            var settings = LauncherHelper.GetLauncherSettings();
            this.DataContext = new JavaSelectorViewModel(this, settings.Launcher.JavaDirectoryPath);
            settings = null;
        }
    }
    
    /// <summary>
    /// Releases resources associated with the <see cref="JavaSelectorWindow"/>.
    /// Logs a debug message indicating that memory is being freed.
    /// </summary>
    protected override void FreeMemory()
    {
        _logger.Debug("Freeing memory in JavaSelectorWindow.");
    }
}