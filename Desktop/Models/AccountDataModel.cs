using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Desktop.Models;

public partial class AccountDataModel : ObservableObject
{
    [ObservableProperty] private string? _selectedAccountId;
    
    [ObservableProperty] private ObservableCollection<Account> _accounts;
    
    public AccountDataModel()
    {
        _accounts = new ObservableCollection<Account>();
        _selectedAccountId = null;
    }

    public AccountDataModel(string? selectedAccountId, ObservableCollection<Account> accounts)
    {
        _selectedAccountId = selectedAccountId;
        _accounts = accounts;
    }

    public AccountDataModel(AccountData data)
    {
        _selectedAccountId = data.SelectedAccountId;
        _accounts = new ObservableCollection<Account>(data.Accounts);
    }
}