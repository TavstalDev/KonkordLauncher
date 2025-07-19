using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Common.Models;

public class AccountData
{
    [JsonPropertyName("accounts"), JsonProperty("accounts")]
    public Dictionary<string, Account> Accounts { get; set; }
    [JsonPropertyName("selectedAccountId"), JsonProperty("selectedAccountId")]
    public string SelectedAccountId { get; set; }

    public AccountData() { }

    public AccountData(Dictionary<string, Account> accounts, string selectedAccountId)
    {
        Accounts = accounts;
        SelectedAccountId = selectedAccountId;
    }
}