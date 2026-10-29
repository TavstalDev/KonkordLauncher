using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Primitives;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
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

    private Button _selectedSideBarButton;

    public MainWindow()
    {
        InitializeComponent();
        _selectedSideBarButton = PlaySideBtn;


    }

    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public MainWindow(MainViewModel viewModel,
        IDiscordPresenceService discordPresenceService, ITopLevelProvider topLevelProvider,
        IDialogService dialogService, IBitmapService bitmapService) : base(bitmapService)
    {
        InitializeComponent();
        _selectedSideBarButton = PlaySideBtn;

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

    #endregion
}
