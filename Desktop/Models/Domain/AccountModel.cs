using Tavstal.BeaconSDK.Models.Accounts;
using Tavstal.BeaconSDK.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Desktop.Models.Domain;

/// <summary>
///     An account shown in the launcher, with a flag that marks it as selected.
/// </summary>
public class AccountModel : Account
{
    /// <summary>
    ///     Creates an account model from an existing account.
    /// </summary>
    /// <param name="account">The account to copy.</param>
    /// <param name="isSelected">Whether the account is selected.</param>
    /// <param name="encryptionService">The service used to read the protected tokens.</param>
    public AccountModel(Account account, bool isSelected, IEncryptionService encryptionService)
    {
        Id = account.Id;
        Uuid = account.Uuid;
        DisplayName = account.DisplayName;
        Type = account.Type;
        AccessTokenExpireDate = account.AccessTokenExpireDate;
        Skins = account.Skins;
        MojangProfile = account.MojangProfile;
        SetAccessToken(account.GetAccessToken(encryptionService), encryptionService);
        SetRefreshToken(account.GetRefreshToken(encryptionService), encryptionService);
        IsSelected = isSelected;
    }

    /// <summary>
    ///     Gets or sets a value that indicates whether the account is selected.
    /// </summary>
    public bool IsSelected { get; set; }
}
