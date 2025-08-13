using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Models;
using JavaSelectorViewModel = Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models.JavaSelectorViewModel;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

/// <summary>
/// Represents a window for selecting a Java version.
/// </summary>
public partial class JavaSelectorWindow : Window
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
            this.DataContext = new JavaSelectorViewModel();
        else
        {
            var settings = LauncherHelper.GetLauncherSettings();
            this.DataContext = new JavaSelectorViewModel(settings.Launcher.JavaDirectoryPath);
        }
    }

    /// <summary>
    /// Handles the click event for the OK button.
    /// Closes the window and returns the selected Java version.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OkBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        if (this.DataContext is not JavaSelectorViewModel vm)
            return;

        this.Close(vm.SelectedJavaVersion);
    }

    /// <summary>
    /// Handles the click event for the Cancel button.
    /// Closes the window without returning a value.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void CancelBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        this.Close(null);
    }
}