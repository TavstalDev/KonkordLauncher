using Avalonia.Controls;
using Avalonia.Interactivity;
using Tavstal.BeaconSDK.Models.Accounts;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs.InstanceConfig;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Pages.EditInstance;

public partial class SettingsPage : UserControl
{
    private Button _selectedSettingsTab;

    public SettingsPage()
    {
        InitializeComponent();
        _selectedSettingsTab = JavaSettingsBtn;
    }

    private void OverridenAccount_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not EditInstanceViewModel vm)
            return;

        if (sender is ComboBox { SelectedItem: Account selectedAccount })
            vm.Settings.InstanceConfig.Misc.AccountId = selectedAccount.Id;
    }

    private void EnvironmentDataGrid_OnRowEditEnded(object? sender, DataGridRowEditEndedEventArgs e)
    {
        if (DataContext is not EditInstanceViewModel vm)
            return;

        if (e.Row.DataContext is not EnvironmentVariable environmentItem)
            return;

        vm.Settings.InstanceConfig.Environment[e.Row.Index] = environmentItem;
    }

    private void AddEnvironmentRow_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not EditInstanceViewModel vm)
            return;
        vm.Settings.InstanceConfig.Environment.Add(new EnvironmentVariable("ENV_VAR", "env_value"));
    }

    private void RemoveEnvironmentRow_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not EditInstanceViewModel vm)
            return;

        if (vm.Settings.SelectedEnvironmentVariableIndex is null or < 0)
            return;

        var index = vm.Settings.SelectedEnvironmentVariableIndex.Value;
        if (index >= vm.Settings.InstanceConfig.Environment.Count)
            return;

        vm.Settings.InstanceConfig.Environment.RemoveAt(index);
    }

    private void ClearEnvironmentTable_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not EditInstanceViewModel vm)
            return;
        vm.Settings.InstanceConfig.Environment.Clear();
    }

    private void JavaSettingsBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        HandleSettingsTabSwitch(EInstanceSettingsTab.JAVA);
    }

    private void GameSettingsBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        HandleSettingsTabSwitch(EInstanceSettingsTab.GAME);
    }

    private void CustomCommandSettingsBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        HandleSettingsTabSwitch(EInstanceSettingsTab.CUSTOM_COMMAND);
    }

    private void EnvironmentSettingsBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        HandleSettingsTabSwitch(EInstanceSettingsTab.ENVIRONMENT);
    }

    private void MiscSettingsBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        HandleSettingsTabSwitch(EInstanceSettingsTab.MISC);
    }

    private void HandleSettingsTabSwitch(EInstanceSettingsTab tab)
    {
        if (DataContext is not EditInstanceViewModel viewModel)
            return;

        if (viewModel.InstanceSettingsTab == tab)
            return;

        viewModel.InstanceSettingsTab = tab;
        _selectedSettingsTab.Classes.Remove("SettingsTabBtnActive");

        _selectedSettingsTab = tab switch
        {
            EInstanceSettingsTab.JAVA => JavaSettingsBtn,
            EInstanceSettingsTab.GAME => GameSettingsBtn,
            EInstanceSettingsTab.CUSTOM_COMMAND => CustomCommandSettingsBtn,
            EInstanceSettingsTab.ENVIRONMENT => EnvironmentSettingsBtn,
            EInstanceSettingsTab.MISC => MiscSettingsBtn,
            _ => _selectedSettingsTab
        };

        _selectedSettingsTab.Classes.Add("SettingsTabBtnActive");
    }
}

