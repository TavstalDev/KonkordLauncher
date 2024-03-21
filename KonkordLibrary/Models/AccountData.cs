using System.Text.Json.Serialization;

namespace KonkordLibrary.Models
{
    public class AccountData
    {
        [JsonPropertyName("accounts")]
        public Dictionary<string, Account> Accounts { get; set; }
        [JsonPropertyName("selectedAccountId")]
        public string SelectedAccountId { get; set; }
        [JsonPropertyName("mojangClientToken")]
        public string MojanClientToken { get; set; }

        public AccountData() { }

        public AccountData(Dictionary<string, Account> accounts, string selectedAccountId, string mojanClientToken)
        {
            Accounts = accounts;
            SelectedAccountId = selectedAccountId;
            MojanClientToken = mojanClientToken;
        }
    }
}
