using Tavstal.BeaconSDK.Models.Accounts;
using Tavstal.BeaconSDK.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Desktop.Models.Domain;

public class AccountModel : Account
{
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

    public bool IsSelected { get; set; }
}
