using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Desktop.Models.Domain;

/// <summary>
/// Represents the data model for account management, including selected account ID and a collection of accounts.
/// </summary>
public partial class AccountDataModel : ObservableObject
{
    /// <summary>
    /// The ID of the currently selected account.
    /// </summary>
    [ObservableProperty] private string? _selectedAccountId;

    private ObservableCollection<Account> _accounts;

    /// <summary>
    /// Gets or sets the collection of accounts. Updates the collection changed event handler when set.
    /// </summary>
    public ObservableCollection<Account> Accounts
    {
        get => _accounts;
        set
        {
            _accounts.CollectionChanged -= OnAccountsCollectionChanged;
            if (SetProperty(ref _accounts, value))
            {
                _accounts.CollectionChanged += OnAccountsCollectionChanged;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether there are any accounts available.
    /// </summary>
    public bool HasAccounts => Accounts.Count > 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountDataModel"/> class with default values.
    /// </summary>
    public AccountDataModel()
    {
        _accounts = [];
        _selectedAccountId = null;
        _accounts.CollectionChanged += OnAccountsCollectionChanged;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountDataModel"/> class with the specified selected account ID and accounts.
    /// </summary>
    /// <param name="selectedAccountId">The ID of the selected account.</param>
    /// <param name="accounts">The collection of accounts.</param>
    public AccountDataModel(string? selectedAccountId, ObservableCollection<Account> accounts)
    {
        _selectedAccountId = selectedAccountId;
        _accounts = accounts;
        _accounts.CollectionChanged += OnAccountsCollectionChanged;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountDataModel"/> class from an existing <see cref="AccountData"/> object.
    /// </summary>
    /// <param name="data">The account data to initialize from.</param>
    public AccountDataModel(AccountData data)
    {
        _selectedAccountId = data.SelectedAccountId;
        _accounts = new ObservableCollection<Account>(data.Accounts);
        _accounts.CollectionChanged += OnAccountsCollectionChanged;
    }

    /// <summary>
    /// Handles the collection changed event for the accounts collection. 
    /// Updates the <see cref="HasAccounts"/> property when the collection changes.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OnAccountsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasAccounts));
    }
}