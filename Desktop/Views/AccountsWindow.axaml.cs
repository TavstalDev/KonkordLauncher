using System;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Tavstal.KonkordLauncher.Desktop.ViewModels;

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
    
    private void OfflineLogin_OnClick(object? sender, RoutedEventArgs e)
    {
        // TODO
    }
    #endregion
}