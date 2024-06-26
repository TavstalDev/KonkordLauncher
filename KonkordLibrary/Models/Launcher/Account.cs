﻿using Tavstal.KonkordLibrary.Enums;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLibrary.Models.Launcher
{
    [Serializable]
    public class Account
    {
        [JsonPropertyName("userId"), JsonProperty("userId")]
        public string UserId { get; set; }
        [JsonPropertyName("uuid"), JsonProperty("uuid")]
        public string UUID { get; set; }
        [JsonPropertyName("displayName"), JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonPropertyName("type"), JsonProperty("type")]
        public EAccountType Type { get; set; }
        [JsonPropertyName("accessToken"), JsonProperty("accessToken")]
        public string AccessToken { get; set; }
        [JsonPropertyName("accessTokenExpDate"), JsonProperty("accessTokenExpDate")]
        public DateTime AccessTokenExpireDate { get; set; }

        public Account() { }

        public Account(string userId, string uUID, string displayName, EAccountType type, string accessToken, DateTime accessTokenExpDate)
        {
            UserId = userId;
            UUID = uUID;
            DisplayName = displayName;
            Type = type;
            AccessToken = accessToken;
            AccessTokenExpireDate = accessTokenExpDate;
        }
    }
}
