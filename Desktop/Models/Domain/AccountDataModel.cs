using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;

namespace Tavstal.KonkordLauncher.Desktop.Models.Domain;

/// <summary>
///     Holds the list of accounts and the ID of the selected account.
/// </summary>
public partial class AccountDataModel : ObservableObject
{
    private ObservableCollection<AccountModel> _accounts;

    /// <summary>
    ///     Creates an empty account model.
    /// </summary>
    public AccountDataModel()
    {
        _accounts = [];
        SelectedAccountId = null;
        _accounts.CollectionChanged += OnAccountsCollectionChanged;
    }

    /// <summary>
    ///     Creates an account model from existing account data.
    /// </summary>
    /// <param name="data">The account data to copy.</param>
    /// <param name="encryptionService">The service used to read the protected tokens.</param>
    public AccountDataModel(AccountData data, IEncryptionService encryptionService)
    {
        SelectedAccountId = data.SelectedAccountId;
        _accounts = [];
        foreach (var account in data.Accounts)
            _accounts.Add(new AccountModel(account, account.Id == data.SelectedAccountId, encryptionService));
        _accounts.CollectionChanged += OnAccountsCollectionChanged;
    }

    /// <summary>
    ///     Gets or sets the ID of the selected account.
    /// </summary>
    [ObservableProperty]
    public partial string? SelectedAccountId { get; set; }

    /// <summary>
    ///     Gets or sets the list of accounts.
    /// </summary>
    public ObservableCollection<AccountModel> Accounts
    {
        get => _accounts;
        set
        {
            _accounts.CollectionChanged -= OnAccountsCollectionChanged;
            if (SetProperty(ref _accounts, value)) _accounts.CollectionChanged += OnAccountsCollectionChanged;
        }
    }

    /// <summary>
    ///     Gets a value that indicates whether the list has any accounts.
    /// </summary>
    public bool HasAccounts => Accounts.Count > 0;

    /// <summary>
    ///     Updates <see cref="HasAccounts" /> when the account list changes.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OnAccountsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasAccounts));
    }
}
