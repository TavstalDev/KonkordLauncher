using Avalonia.Controls;
using Avalonia.Interactivity;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Pages.Main;

public partial class AboutPage : UserControl
{
    private Button _selectedAboutTabButton;

    public AboutPage()
    {
        InitializeComponent();
        _selectedAboutTabButton = AboutInfoBtn;
    }

    private void AboutInfoBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        HandleAboutTabChange(EAboutTab.ABOUT);
    }

    private void CreditsInfoBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        HandleAboutTabChange(EAboutTab.CREDITS);
    }

    private void LicenseInfoBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        HandleAboutTabChange(EAboutTab.LICENSE);
    }

    private void HandleAboutTabChange(EAboutTab tabType)
    {
        if (DataContext is not MainViewModel viewModel)
            return;

        if (viewModel.CurrentAboutTab == tabType)
            return;

        viewModel.CurrentAboutTab = tabType;
        _selectedAboutTabButton.Classes.Remove("SettingsTabBtnActive");
        _selectedAboutTabButton = tabType switch
        {
            EAboutTab.ABOUT => AboutInfoBtn,
            EAboutTab.CREDITS => CreditsInfoBtn,
            EAboutTab.LICENSE => LicenseInfoBtn,
            _ => _selectedAboutTabButton
        };
        _selectedAboutTabButton.Classes.Add("SettingsTabBtnActive");
    }
}

