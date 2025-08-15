using Avalonia;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models;
using InstallViewModel = Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models.InstallViewModel;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

/// <summary>
/// Represents the installation window in the application, which implements the <see cref="IProgressReporter"/> interface
/// to report progress and status updates during installation.
/// </summary>
public partial class InstallWindow : KonkordWindow, IProgressReporter
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
    /// Releases resources associated with the <see cref="InstallWindow"/>.
    /// This method is intended to be overridden in derived classes to free unmanaged resources or perform cleanup tasks.
    /// </summary>
    protected override void FreeMemory() { }

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