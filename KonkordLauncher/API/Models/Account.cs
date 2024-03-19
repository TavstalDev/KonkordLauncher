using KonkordLauncher.API.Enums;
using System.Text.Json.Serialization;
using System;

namespace KonkordLauncher.API.Models
{
    [Serializable]
    public class Account
    {
        [JsonPropertyName("userId")]
        public string UserId {  get; set; }
        [JsonPropertyName("uuid")]
        public string UUID { get; set; }
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }
        [JsonPropertyName("type")]
        public EAccountType Type { get; set; }
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }

        public Account() { }

        public Account(string userId, string uUID, string displayName, EAccountType type, string accessToken, string refreshToken)
        {
            UserId = userId;
            UUID = uUID;
            DisplayName = displayName;
            Type = type;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }
    }
}
