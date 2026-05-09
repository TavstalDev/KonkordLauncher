using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Models.Accounts;

namespace Tavstal.KonkordLauncher.Common.Models;

/// <summary>
/// Represents account data, including a collection of accounts and the ID of the selected account.
/// </summary>
public class AccountData
{
    /// <summary>
    /// Gets or sets the ID of the selected account.
    /// </summary>
    [JsonPropertyName("selectedAccountId"), JsonProperty("selectedAccountId")]
    public string SelectedAccountId { get; set; }

    /// <summary>
    /// Gets or sets the list of accounts
    /// </summary>
    [JsonPropertyName("accounts"), JsonProperty("accounts")]
    public List<Account> Accounts { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountData"/> class.
    /// </summary>
    public AccountData()
    {
        SelectedAccountId = string.Empty;
        Accounts = new List<Account>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountData"/> class with the specified accounts and selected account ID.
    /// </summary>
    /// <param name="accounts">The list of accounts.</param>
    /// <param name="selectedAccountId">The ID of the selected account.</param>
    public AccountData(List<Account> accounts, string selectedAccountId)
    {
        Accounts = accounts;
        SelectedAccountId = selectedAccountId;
    }
}