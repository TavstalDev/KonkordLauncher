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
    /// Gets or sets the unique identifier of the account.
    /// </summary>
    [JsonPropertyName("id"), JsonProperty("id")]
    public string Id { get; set; }
    
    /// <summary>
    /// Gets or sets the universally unique identifier (UUID) of the account.
    /// </summary>
    [JsonPropertyName("uuid"), JsonProperty("uuid")]
    public string Uuid { get; set; }

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
    [Obsolete("Use AccessToken property instead. This property should not be used directly.")]
    [JsonPropertyName("accessToken"), JsonProperty("accessToken")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string EncryptedAccessToken { get; set; }
    
    /// <summary>
    /// Gets or sets the decrypted access token for the account.
    /// The token is encrypted when set and decrypted when retrieved.
    /// </summary>
    [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public string AccessToken
    {
#pragma warning disable CS0618 // Type or member is obsolete
        get => EncryptionUtility.Decrypt(EncryptedAccessToken);
        set => EncryptedAccessToken = EncryptionUtility.Encrypt(value);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    /// <summary>
    /// Gets or sets the expiration date of the access token.
    /// </summary>
    [JsonPropertyName("accessTokenExpDate"), JsonProperty("accessTokenExpDate")]
    public DateTime AccessTokenExpireDate { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Account"/> class with default values.
    /// </summary>
    public Account() {}
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Account"/> class with the specified parameters.
    /// </summary>
    /// <param name="id">The ID of the account</param>
    /// <param name="uuid">The UUID of the account.</param>
    /// <param name="displayName">The display name of the account.</param>
    /// <param name="type">The type of the account.</param>
    /// <param name="accessToken">The access token for the account.</param>
    /// <param name="accessTokenExpDate">The expiration date of the access token.</param>
    public Account(string id, string uuid, string displayName, EAccountType type, string accessToken,
        DateTime accessTokenExpDate)
    {
        Id = id;
        Uuid = uuid;
        DisplayName = displayName;
        Type = type;
        AccessToken = accessToken;
        AccessTokenExpireDate = accessTokenExpDate;
    }
    
    public bool CanExpire => Type != EAccountType.OFFLINE;
}