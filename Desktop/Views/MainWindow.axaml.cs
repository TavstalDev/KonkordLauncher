using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Primitives;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Button = Avalonia.Controls.Button;
using MainViewModel = Tavstal.KonkordLauncher.Desktop.Views.Models.MainViewModel;

namespace Tavstal.KonkordLauncher.Desktop.Views;

// ReSharper disable once PartialTypeWithSinglePart
public partial class MainWindow : KonkordWindow<MainViewModel>
{
    private readonly IDiscordPresenceService _discordPresenceService = null!;
    private readonly ITopLevelProvider _topLevelProvider = null!;

    private Button _selectedAboutTabButton;
    private Button _selectedSettingsTabButton;
    private Button _selectedSideBarButton;

    public MainWindow()
    {
        InitializeComponent();
        _selectedSideBarButton = PlaySideBtn;
        _selectedSettingsTabButton = LauncherSettingsBtn;
        _selectedAboutTabButton = AboutInfoBtn;
    }

    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public MainWindow(MainViewModel viewModel,
        IDiscordPresenceService discordPresenceService, ITopLevelProvider topLevelProvider,
        IDialogService dialogService)
    {
        InitializeComponent();
        _selectedSideBarButton = PlaySideBtn;
        _selectedSettingsTabButton = LauncherSettingsBtn;
        _selectedAboutTabButton = AboutInfoBtn;

        if (Design.IsDesignMode)
            return;

        _discordPresenceService = discordPresenceService;
        _topLevelProvider = topLevelProvider;
        dialogService.SetMainWindow(this);

        DataContext = viewModel;
        this.WhenActivated(disposables =>
        {
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.SwitchSidebarBtnInteraction.RegisterHandler(action =>
            {
                HandleSidebarChange(action.Input);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.UpdateSettingsTabButtonInteraction.RegisterHandler(action =>
            {
                HandleSettingsTabChange(action.Input);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.SwitchAboutTabInteractionInteraction.RegisterHandler(action =>
            {
                HandleAboutTabChange(action.Input);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
        });
    }

    private void HandleSidebarChange(ESidebarType sidebarType)
    {
        if (DataContext is not { } viewModel)
            return;

        if (viewModel.CurrentPageIndex == sidebarType)
            return;

        viewModel.CurrentPageIndex = sidebarType;
        _selectedSideBarButton.Classes.Remove("SideBarActiveBtn");

        _selectedSideBarButton = sidebarType switch
        {
            ESidebarType.Play => PlaySideBtn,
            ESidebarType.Patch => NewsSideBtn,
            ESidebarType.Accounts => AccountsSideBtn,
            ESidebarType.Settings => SettingsSideBtn,
            ESidebarType.About => AboutSideBtn,
            ESidebarType.Skins => SkinsSideBtn,
            _ => _selectedSideBarButton
        };

        _selectedSideBarButton.Classes.Add("SideBarActiveBtn");
    }

    private void HandleSettingsTabChange(ESettingsTab tabType)
    {
        if (DataContext is not { } viewModel)
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

    private void HandleAboutTabChange(EAboutTab tabType)
    {
        if (DataContext is not { } viewModel)
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

    #region Events

    /// <inheritdoc />
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (Design.IsDesignMode)
            return;
        _discordPresenceService.Update("Browsing instances...");
        _topLevelProvider.SetActive(this);
        GC.Collect(2, GCCollectionMode.Forced, true, true);
    }

    /// <inheritdoc />
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        if (Design.IsDesignMode)
            return;
        _discordPresenceService.Clear();
        _topLevelProvider.ClearIfMatches(this);
        base.OnClosing(e);
    }

    private void Language_OnSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not { } viewModel)
            return;

        if (sender is not ComboBox { SelectedItem: Language selectedLanguage })
            return;

        viewModel.Config.CoreConfig.Launcher.Language = selectedLanguage.TwoLetterCode;
    }

    #endregion
}
