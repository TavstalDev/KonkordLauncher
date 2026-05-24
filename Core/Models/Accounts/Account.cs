using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Encryption;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.User;

namespace Tavstal.KonkordLauncher.Core.Models.Accounts;

/// <summary>
/// Represents an account with properties such as ID, UUID, display name, account type, 
/// access token, refresh token, expiration date, skins, and Mojang profile.
/// </summary>
[Serializable]
public class Account
{
    /// <summary>
    /// Gets or sets the unique identifier of the account.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the UUID of the account.
    /// </summary>
    [JsonProperty("uuid")]
    public string Uuid { get; set; }

    /// <summary>
    /// Gets or sets the display name of the account.
    /// </summary>
    [JsonProperty("displayName")]
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the type of the account.
    /// </summary>
    [JsonProperty("type")]
    public EAccountType Type { get; set; }

    /// <summary>
    /// Gets or sets the encrypted access token. 
    /// This property is obsolete and should not be used directly.
    /// </summary>
    [Obsolete("Use GetAccessToken() instead. This property should not be used directly.")]
    [JsonProperty("accessToken")]
    private string EncryptedAccessToken { get; set; }

    /// <summary>
    /// Gets or sets the encrypted refresh token. 
    /// This property is obsolete and should not be used directly.
    /// </summary>
    [Obsolete("Use GetRefreshToken() instead. This property should not be used directly.")]
    [JsonProperty("refreshToken")]
    private string EncryptedRefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the expiration date of the access token.
    /// </summary>
    [JsonProperty("accessTokenExpDate")]
    public DateTime AccessTokenExpireDate { get; set; }

    /// <summary>
    /// Gets or sets the list of skins associated with the account.
    /// </summary>
    [JsonProperty("skins")]
    public List<AccountSkin> Skins { get; set; } = new();

    /// <summary>
    /// Gets or sets the Mojang profile associated with the account.
    /// </summary>
    [JsonProperty("mojangProfile")]
    public MojangProfile? MojangProfile { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the account is selected.
    /// </summary>
    [JsonIgnore]
    public bool IsSelected { get; set; }

    /// <summary>
    /// Gets a value indicating whether the account can expire.
    /// </summary>
    public bool CanExpire => Type != EAccountType.OFFLINE;

    /// <summary>
    /// Initializes a new instance of the <see cref="Account"/> class.
    /// </summary>
    public Account() {}

    private string _accessTokenCache = string.Empty;

    /// <summary>
    /// Retrieves the decrypted access token. If the token is cached, it returns the cached value.
    /// </summary>
    /// <returns>The decrypted access token.</returns>
    public string GetAccessToken()
    {
        if (!string.IsNullOrEmpty(_accessTokenCache))
            return _accessTokenCache;
        if (!EncryptionUtility.IsDataProtectorSet)
            return EncryptedAccessToken;
        _accessTokenCache = EncryptionUtility.Decrypt(EncryptedAccessToken);
        return _accessTokenCache;
    }

    /// <summary>
    /// Sets the access token and encrypts it for storage.
    /// </summary>
    /// <param name="accessToken">The access token to set.</param>
    public void SetAccessToken(string accessToken)
    {
        _accessTokenCache = accessToken;
        EncryptedAccessToken = EncryptionUtility.Reprotect(accessToken);
    }

    private string _refreshTokenCache = string.Empty;

    /// <summary>
    /// Retrieves the decrypted refresh token. If the token is cached, it returns the cached value.
    /// </summary>
    /// <returns>The decrypted refresh token.</returns>
    public string GetRefreshToken()
    {
        if (!string.IsNullOrEmpty(_refreshTokenCache))
            return _refreshTokenCache;
        if (!EncryptionUtility.IsDataProtectorSet)
            return EncryptedRefreshToken;
        _refreshTokenCache = EncryptionUtility.Decrypt(EncryptedRefreshToken);
        return _refreshTokenCache;
    }

    /// <summary>
    /// Sets the refresh token and encrypts it for storage.
    /// </summary>
    /// <param name="refreshToken">The refresh token to set.</param>
    public void SetRefreshToken(string refreshToken)
    {
        _refreshTokenCache = refreshToken;
        EncryptedRefreshToken = EncryptionUtility.Reprotect(refreshToken);
    }
}