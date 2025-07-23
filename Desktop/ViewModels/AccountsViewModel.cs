using ReactiveUI;

namespace Tavstal.KonkordLauncher.Desktop.ViewModels;

public class AccountsViewModel : ViewModelBase
{
    private bool isLoggingInMicrosoftAccount;
    public bool IsLoggingInMicrosoftAccount
    {
        get => isLoggingInMicrosoftAccount;
        set => this.RaiseAndSetIfChanged(ref isLoggingInMicrosoftAccount, value);
    }
}