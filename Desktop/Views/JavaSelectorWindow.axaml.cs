using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views;

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

        this.DataContext = new JavaSelectorViewModel();
        App.OnLanguageChanged += HandleLanguageChange;
        HandleLanguageChange(string.Empty);
    }

    /// <summary>
    /// Handles changes in the application's language by updating UI text.
    /// </summary>
    /// <param name="language">The new language code.</param>
    private void HandleLanguageChange(string language)
    {
        this.Title = TranslationManager.Translate("java.title");
        SelectJavaTb.Text = TranslationManager.Translate("java.select");

        OkBtn.Content = TranslationManager.Translate("common.ok");
        CancelBtn.Content = TranslationManager.Translate("common.cancel");

        if (this.DataContext is not JavaSelectorViewModel vm)
            return;

        vm.TableMajorText = TranslationManager.Translate("java.table.major");
        vm.TableVersionText = TranslationManager.Translate("java.table.version");
        vm.TableArchitectureText = TranslationManager.Translate("java.table.architecture");
        vm.TablePathText = TranslationManager.Translate("java.table.path");
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