using Tavstal.KonkordLauncher.Core.Models.Accounts;

namespace Tavstal.KonkordLauncher.Desktop.Models.Domain;

/// <summary>
/// Represents an account model for UI binding, extending the base <see cref="Account"/> class with selection state.
/// </summary>
public class AccountModel : Account
{
    /// <summary>
    /// Gets or sets whether this account is currently selected in the UI.
    /// </summary>
    public bool IsSelected { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AccountModel"/> class by copying data from an existing <see cref="Account"/>.
    /// </summary>
    /// <param name="account">The source account to copy data from.</param>
    /// <param name="isSelected">Whether this account should be marked as selected.</param>
    public AccountModel(Account account, bool isSelected)
    {
        Id = account.Id;
        Uuid = account.Uuid;
        DisplayName = account.DisplayName;
        Type = account.Type;
        AccessTokenExpireDate = account.AccessTokenExpireDate;
        Skins = account.Skins;
        MojangProfile = account.MojangProfile;
        SetAccessToken(account.GetAccessToken());
        SetRefreshToken(account.GetRefreshToken());
        IsSelected = isSelected;
    }
}