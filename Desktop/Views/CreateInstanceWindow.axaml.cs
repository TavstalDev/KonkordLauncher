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
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views;

/// <summary>
///     The window used to create a new game instance.
/// </summary>
public partial class CreateInstanceWindow : KonkordWindow<CreateInstanceViewModel>
{
    private readonly IDiscordPresenceService _discordPresenceService = null!;
    private Button _selectedTabBtn;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CreateInstanceWindow" /> class.
    /// </summary>
    public CreateInstanceWindow()
    {
        InitializeComponent();
        _selectedTabBtn = CustomTabBtn;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CreateInstanceWindow" /> class with the
    ///     given view model and services.
    /// </summary>
    /// <param name="viewModel">The view model that controls the window.</param>
    /// <param name="discordPresenceService">The service used to update the Discord presence.</param>
    /// <param name="bitmapService">The service used to load and release bitmaps.</param>
    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public CreateInstanceWindow(CreateInstanceViewModel viewModel, IDiscordPresenceService discordPresenceService,
        IBitmapService bitmapService) : base(bitmapService)
    {
        InitializeComponent();
        _selectedTabBtn = CustomTabBtn;

        if (Design.IsDesignMode)
            return;

        DataContext = viewModel;
        _discordPresenceService = discordPresenceService;

        this.WhenActivated(disposables =>
        {
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.SwitchTabInteraction.RegisterHandler(action =>
            {
                HandleTabChange(action.Input);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
        });
    }

    private void HandleTabChange(ECreateInstanceTab tab)
    {
        if (DataContext is not { } viewModel)
            return;

        if (viewModel.SelectedTab == tab)
            return;

        viewModel.SelectedTab = tab;
        _selectedTabBtn.Classes.Remove("SettingsTabBtnActive");
        switch (tab)
        {
            case ECreateInstanceTab.CUSTOM:
            {
                _selectedTabBtn = CustomTabBtn;
                break;
            }
            case ECreateInstanceTab.MODPACK:
            {
                _selectedTabBtn = ModpackTabBtn;
                break;
            }
            case ECreateInstanceTab.IMPORT:
            {
                _selectedTabBtn = ImportTabBtn;
                break;
            }
        }

        _selectedTabBtn.Classes.Add("SettingsTabBtnActive");
    }

    #region Events

    /// <summary>
    ///     Sets the Discord presence to show that an instance is being created.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        _discordPresenceService.Update("Creating instance...");
    }

    /// <summary>
    ///     Resets the Discord presence to show that the user is browsing instances.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _discordPresenceService.Update("Browsing instances...");
    }

    #endregion
}
