using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Encryption;
using Tavstal.KonkordLauncher.Core.Enums;

namespace Tavstal.KonkordLauncher.Core.Models;

/// <summary>
/// Represents an account with user details, authentication tokens, and account type.
/// </summary>
[Serializable]
public class Account
{
    /// <summary>
    /// Gets or sets the user ID associated with the account.
    /// </summary>
    [JsonPropertyName("userId"), JsonProperty("userId")]
    public string UserId { get; set; }

    /// <summary>
    /// Gets or sets the universally unique identifier (UUID) of the account.
    /// </summary>
    [JsonPropertyName("uuid"), JsonProperty("uuid")]
    public string UUID { get; set; }

    /// <summary>
    /// Gets or sets the display name of the account.
    /// </summary>
    [JsonPropertyName("displayName"), JsonProperty("displayName")]
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the type of the account (e.g., Mojang, Microsoft).
    /// </summary>
    [JsonPropertyName("type"), JsonProperty("type")]
    public EAccountType Type { get; set; }

    /// <summary>
    /// Stores the encrypted access token for the account.
    /// </summary>
    [JsonPropertyName("accessToken"), JsonProperty("accessToken")]
    private string _encryptedAccessToken;

    /// <summary>
    /// Gets or sets the decrypted access token for the account.
    /// The token is encrypted when set and decrypted when retrieved.
    /// </summary>
    [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public string AccessToken
    {
        get => EncryptionUtility.Decrypt(_encryptedAccessToken);
        set => _encryptedAccessToken = EncryptionUtility.Encrypt(value);
    }

    /// <summary>
    /// Gets or sets the expiration date of the access token.
    /// </summary>
    [JsonPropertyName("accessTokenExpDate"), JsonProperty("accessTokenExpDate")]
    public DateTime AccessTokenExpireDate { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Account"/> class.
    /// </summary>
    public Account()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Account"/> class with the specified parameters.
    /// </summary>
    /// <param name="userId">The user ID associated with the account.</param>
    /// <param name="uUID">The UUID of the account.</param>
    /// <param name="displayName">The display name of the account.</param>
    /// <param name="type">The type of the account.</param>
    /// <param name="accessToken">The encrypted access token for the account.</param>
    /// <param name="accessTokenExpDate">The expiration date of the access token.</param>
    public Account(string userId, string uUID, string displayName, EAccountType type, string accessToken,
        DateTime accessTokenExpDate)
    {
        UserId = userId;
        UUID = uUID;
        DisplayName = displayName;
        Type = type;
        _encryptedAccessToken = accessToken;
        AccessTokenExpireDate = accessTokenExpDate;
    }
}