using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Models;

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
    /// Gets or sets the dictionary of accounts, where the key is the account ID and the value is the account object.
    /// </summary>
    [JsonPropertyName("accounts"), JsonProperty("accounts")]
    public Dictionary<string, Account> Accounts { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountData"/> class.
    /// </summary>
    public AccountData() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountData"/> class with the specified accounts and selected account ID.
    /// </summary>
    /// <param name="accounts">The dictionary of accounts.</param>
    /// <param name="selectedAccountId">The ID of the selected account.</param>
    public AccountData(Dictionary<string, Account> accounts, string selectedAccountId)
    {
        Accounts = accounts;
        SelectedAccountId = selectedAccountId;
    }
}