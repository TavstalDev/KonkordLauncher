using Avalonia;
using Avalonia.Controls;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views;

/// <summary>
/// Represents the installation window in the application, which implements the <see cref="IProgressReporter"/> interface
/// to report progress and status updates during installation.
/// </summary>
public partial class InstallWindow : Window, IProgressReporter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InstallWindow"/> class.
    /// </summary>
    public InstallWindow()
    {
        InitializeComponent();

#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes in debug mode.
        this.AttachDevTools();
#endif

        // Sets the data context of the window to an instance of the InstallViewModel.
        this.DataContext = new InstallViewModel();
    }

    /// <summary>
    /// Updates the progress value in the associated view model.
    /// </summary>
    /// <param name="progress">The progress value to set, typically a percentage (0-100).</param>
    public void SetProgress(double progress)
    {
        if (this.DataContext is not InstallViewModel viewModel)
            return;
        
        viewModel.ProgressValue = progress;
    }

    /// <summary>
    /// Updates the status text in the associated view model.
    /// </summary>
    /// <param name="status">The status message to display.</param>
    public void SetStatus(string status)
    {
        if (this.DataContext is not InstallViewModel viewModel)
            return;
        
        viewModel.ProgressText = status;
    }

    /// <summary>
    /// Updates the status text in the associated view model using a translated string.
    /// </summary>
    /// <param name="statusKey">The translation key for the status message.</param>
    /// <param name="args">Optional arguments to format the translated message.</param>
    public void SetStatusTranslated(string statusKey, params object[]? args)
    {
        if (this.DataContext is not InstallViewModel viewModel)
            return;
        
        viewModel.ProgressText = TranslationManager.Translate(statusKey, args);
    }
}