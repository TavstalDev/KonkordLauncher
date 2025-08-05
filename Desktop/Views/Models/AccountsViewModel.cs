using ReactiveUI;
using Tavstal.KonkordLauncher.Desktop.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

/// <summary>
/// Represents the view model for managing accounts in the application.
/// </summary>
public class AccountsViewModel : ViewModelBase
{
    private bool isLoggingInMicrosoftAccount;

    /// <summary>
    /// Gets or sets a value indicating whether the user is currently logging in with a Microsoft account.
    /// </summary>
    public bool IsLoggingInMicrosoftAccount
    {
        get => isLoggingInMicrosoftAccount;
        set => this.RaiseAndSetIfChanged(ref isLoggingInMicrosoftAccount, value);
    }
    
    /// <summary>
    /// The progress value, typically between 0.0 and 1.0.
    /// </summary>
    private double _progress = 0;

    /// <summary>
    /// Gets or sets the progress value.
    /// </summary>
    public double Progress
    {
        get => _progress;
        set => this.RaiseAndSetIfChanged(ref _progress, value);
    }

    /// <summary>
    /// The text displayed to indicate the current progress status.
    /// </summary>
    private string _progressText = "Loading...";

    /// <summary>
    /// Gets or sets the progress status text.
    /// </summary>
    public string ProgressText
    {
        get => _progressText;
        set => this.RaiseAndSetIfChanged(ref _progressText, value);
    }
}