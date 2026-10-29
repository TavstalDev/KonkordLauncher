using Avalonia.Controls;
using Avalonia.Interactivity;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Pages.Main;

public partial class SettingsPage : UserControl
{
    private Button _selectedSettingsTabButton;

    public SettingsPage()
    {
        InitializeComponent();
        _selectedSettingsTabButton = LauncherSettingsBtn;
    }

    private void Language_OnSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
            return;

        if (sender is not ComboBox { SelectedItem: Language selectedLanguage })
            return;

        viewModel.Config.CoreConfig.Launcher.Language = selectedLanguage.TwoLetterCode;
    }

    private void LauncherSettingsBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        HandleSettingsTabChange(ESettingsTab.LAUNCHER);
    }

    private void MinecraftSettingsBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        HandleSettingsTabChange(ESettingsTab.MINECRAFT);
    }

    private void JavaSettingsBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        HandleSettingsTabChange(ESettingsTab.JAVA);
    }

    private void MiscSettingsBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        HandleSettingsTabChange(ESettingsTab.MISC);
    }

    private void HandleSettingsTabChange(ESettingsTab tabType)
    {
        if (DataContext is not MainViewModel viewModel)
            return;

        if (viewModel.CurrentSettingsTab == tabType)
            return;

        viewModel.CurrentSettingsTab = tabType;
        _selectedSettingsTabButton.Classes.Remove("SettingsTabBtnActive");
        _selectedSettingsTabButton = tabType switch
        {
            ESettingsTab.LAUNCHER => LauncherSettingsBtn,
            ESettingsTab.MINECRAFT => MinecraftSettingsBtn,
            ESettingsTab.JAVA => JavaSettingsBtn,
            ESettingsTab.MISC => MiscSettingsBtn,
            _ => _selectedSettingsTabButton
        };
        _selectedSettingsTabButton.Classes.Add("SettingsTabBtnActive");
    }
}

