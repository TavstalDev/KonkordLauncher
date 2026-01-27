using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Encryption;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.User;

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
    /// <remarks>
    /// This property is marked as obsolete. Use the <see cref="AccessToken"/> property instead.
    /// </remarks>
    [Obsolete("Use AccessToken property instead. This property should not be used directly.")]
    [JsonPropertyName("accessToken"), JsonProperty("accessToken")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string EncryptedAccessToken { get; set; }
    
    /// <summary>
    /// Stores the encrypted refresh token for the account.
    /// </summary>
    /// <remarks>
    /// This property is marked as obsolete. Use the <see cref="RefreshToken"/> property instead.
    /// </remarks>
    [Obsolete("Use RefreshToken property instead. This property should not be used directly.")]
    [JsonPropertyName("refreshToken"), JsonProperty("refreshToken")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string EncryptedRefreshToken { get; set; }
    
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
    /// Gets or sets the decrypted refresh token for the account.
    /// The token is encrypted when set and decrypted when retrieved.
    /// </summary>
    [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public string RefreshToken
    {
#pragma warning disable CS0618 // Type or member is obsolete
        get => EncryptionUtility.Decrypt(EncryptedRefreshToken);
        set => EncryptedRefreshToken = EncryptionUtility.Encrypt(value);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    /// <summary>
    /// Gets or sets the expiration date of the access token.
    /// </summary>
    [JsonPropertyName("accessTokenExpDate"), JsonProperty("accessTokenExpDate")]
    public DateTime AccessTokenExpireDate { get; set; }
    
    [JsonProperty("mojangProfile"), JsonPropertyName("mojangProfile")]
    public MojangProfile? MojangProfile { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Account"/> class with default values.
    /// </summary>
    public Account() {}

    /// <summary>
    /// Initializes a new instance of the <see cref="Account"/> class with the specified details.
    /// </summary>
    /// <param name="id">The unique identifier of the account.</param>
    /// <param name="uuid">The universally unique identifier (UUID) of the account.</param>
    /// <param name="displayName">The display name of the account.</param>
    /// <param name="type">The type of the account (e.g., Mojang, Microsoft).</param>
    /// <param name="encryptedAccessToken">The encrypted access token for the account.</param>
    /// <param name="encryptedRefreshToken">The encrypted refresh token for the account.</param>
    /// <param name="accessTokenExpDate">The expiration date of the access token.</param>
    /// <param name="mojangProfile">The Mojang profile associated with the account, if any.</param>
    public Account(string id, string uuid, string displayName, EAccountType type, string encryptedAccessToken, string encryptedRefreshToken,
        DateTime accessTokenExpDate, MojangProfile? mojangProfile)
    {
        Id = id;
        Uuid = uuid;
        DisplayName = displayName;
        Type = type;
#pragma warning disable CS0618 // Type or member is obsolete
        EncryptedAccessToken = EncryptionUtility.Reprotect(encryptedAccessToken);
        EncryptedRefreshToken = EncryptionUtility.Reprotect(encryptedRefreshToken);
#pragma warning restore CS0618 // Type or member is obsolete
        AccessTokenExpireDate = accessTokenExpDate;
        MojangProfile = mojangProfile;
    }
    
    /// <summary>
    /// Gets a value indicating whether the account's access token can expire.
    /// Returns true if the account type is not OFFLINE; otherwise, false.
    /// </summary>
    public bool CanExpire => Type != EAccountType.OFFLINE;
    
    /// <summary>
    /// Gets or sets a value indicating whether the account is currently selected.
    /// </summary>
    [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public bool IsSelected { get; set; }
}