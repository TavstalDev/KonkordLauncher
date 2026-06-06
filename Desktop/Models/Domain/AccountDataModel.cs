using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Common.Models;

namespace Tavstal.KonkordLauncher.Desktop.Models.Domain;

/// <summary>
/// Represents the data model for account management, including selected account ID and a collection of accounts.
/// </summary>
public partial class AccountDataModel : ObservableObject
{
    /// <summary>
    /// The ID of the currently selected account.
    /// </summary>
    [ObservableProperty]
    public partial string? SelectedAccountId { get; set; }

    private ObservableCollection<AccountModel> _accounts;

    /// <summary>
    /// Gets or sets the collection of accounts. Updates the collection changed event handler when set.
    /// </summary>
    public ObservableCollection<AccountModel> Accounts
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
        SelectedAccountId = null;
        _accounts.CollectionChanged += OnAccountsCollectionChanged;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountDataModel"/> class from an existing <see cref="AccountData"/> object.
    /// </summary>
    /// <param name="data">The account data to initialize from.</param>
    public AccountDataModel(AccountData data)
    {
        SelectedAccountId = data.SelectedAccountId;
        _accounts = [];
        foreach (var account in data.Accounts)
        {
            _accounts.Add(new AccountModel(account, account.Id == data.SelectedAccountId));
        }
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