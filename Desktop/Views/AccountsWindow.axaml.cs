using System;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views;

public partial class AccountsWindow : Window
{
    public AccountsWindow()
    {
        InitializeComponent();

#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif

        this.DataContext = new AccountsViewModel();
    }
    

    #region Event Handlers
    
    private void MicrosoftLogin_OnClick(object? sender, RoutedEventArgs e)
    {
        // TODO
    }

    private void OpenLink_OnClick(object? sender, RoutedEventArgs e)
    {
        // TODO
    }
    
    private void CopyLink_OnClick(object? sender, RoutedEventArgs e)
    {
        // TODO
    }

    private void CancelLogin_OnClick(object? sender, RoutedEventArgs e)
    {
        // TODO
    }
    
    /// <summary>
    /// Handles the text changed event for the offline username input field. 
    /// Ensures that the input contains only alphanumeric characters and underscores.
    /// If invalid characters are detected, they are removed, and the caret position is adjusted accordingly.
    /// </summary>
    /// <param name="sender">The source of the event, expected to be a TextBox.</param>
    /// <param name="e">The event data.</param>
    private void OfflineUsername_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox)
            return;
        
        if (textBox.Text == null)
            return;

        string allowed = Regex.Replace(textBox.Text, @"[^A-Za-z0-9_]", "");
        if (textBox.Text != allowed)
        {
            int caret = textBox.CaretIndex;
            textBox.Text = allowed;
            textBox.CaretIndex = Math.Min(caret - 1, allowed.Length);
        }
    }
    
    /// <summary>
    /// Handles the click event for offline login. Validates the username, checks for duplicate accounts,
    /// and creates a new offline account if valid. Displays appropriate alerts for errors or success.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OfflineLogin_OnClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(OfflineUsernameInput.Text))
        {
            AlertWindow window = new AlertWindow(
                "Name Required",
                "Please enter a username for the account.",
                EAlertType.Warning
            );
            window.ShowDialog(this);
            return;
        }

        string uuid = GameHelper.GetOfflinePlayerUUID(OfflineUsernameInput.Text);
        AccountData accountData = LauncherHelper.GetAccountData();
        var account = accountData.Accounts.FirstOrDefault(x => x.Uuid == uuid);
        if (account != null)
        {
            AlertWindow window = new AlertWindow(
                "Account Already Exists",
                "An account with this username already exists. Please choose a different username.",
                EAlertType.Error
                );
            window.ShowDialog(this);
            return;
        }
        
        var id = Guid.NewGuid().ToString();
        if (string.IsNullOrEmpty(accountData.SelectedAccountId))
            accountData.SelectedAccountId = id;

        account = new Account(id, uuid, OfflineUsernameInput.Text, EAccountType.OFFLINE, "no_token_needed",
            DateTime.Now);
        accountData.Accounts.Add(account);
        Console.WriteLine($"Encrypted token value: '{account.EncryptedAccessToken}'"); 
        Console.WriteLine($"Token value: '{account.AccessToken}'"); 
        JsonHelper.WriteJsonFile(PathHelper.LauncherAccountsPath, accountData);
        App.InvokeAccountsChanged();
        this.Close();
    }
    #endregion
}