using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace KonkordLibrary.Models
{
    public class AccountData
    {
        [JsonPropertyName("accounts"), JsonProperty("accounts")]
        public Dictionary<string, Account> Accounts { get; set; }
        [JsonPropertyName("selectedAccountId"), JsonProperty("selectedAccountId")]
        public string SelectedAccountId { get; set; }
        [JsonPropertyName("mojangClientToken"), JsonProperty("mojangClientToken")]
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
