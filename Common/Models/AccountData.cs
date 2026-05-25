
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
    [JsonProperty("selectedAccountId")]
    public string SelectedAccountId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of accounts
    /// </summary>
    [JsonProperty("accounts")]
    public List<Account> Accounts { get; set; } = [];
}