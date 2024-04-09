using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace KonkordLibrary.Models.Launcher
{
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
}
