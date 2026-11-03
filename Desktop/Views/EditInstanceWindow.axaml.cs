using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Primitives;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Services;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views;

/// <summary>
///     The window used to view and change the settings of a game instance.
/// </summary>
public partial class EditInstanceWindow : KonkordWindow<EditInstanceViewModel>
{
    private readonly IDiscordPresenceService _discordPresenceService = null!;
    private readonly IEventService _eventService = null!;
    private readonly string _instanceId = null!;
    private Button _selectedInstanceTab;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EditInstanceWindow" /> class.
    /// </summary>
    public EditInstanceWindow()
    {
        InitializeComponent();
        _selectedInstanceTab = LogsTabBtn;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="EditInstanceWindow" /> class with the
    ///     given view model and services.
    /// </summary>
    /// <param name="viewModel">The view model that controls the window.</param>
    /// <param name="eventService">The service used to raise application events.</param>
    /// <param name="discordPresenceService">The service used to update the Discord presence.</param>
    /// <param name="bitmapService">The service used to load and release bitmaps.</param>
    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public EditInstanceWindow(EditInstanceViewModel viewModel,
        IEventService eventService, IDiscordPresenceService discordPresenceService,
        IBitmapService bitmapService) : base(bitmapService)
    {
        InitializeComponent();
        _selectedInstanceTab = LogsTabBtn;

        if (Design.IsDesignMode)
            return;

        _eventService = eventService;
        _discordPresenceService = discordPresenceService;
        _instanceId = viewModel.Instance.Id;
        DataContext = viewModel;

        this.WhenActivated(disposables =>
        {
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.TabSwitchInteraction.RegisterHandler(action =>
            {
                HandleTabSwitch(action.Input);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
        });
    }

    /// <summary>
    ///     Sets the Discord presence to show that an instance is being edited.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        _discordPresenceService.Update("Editing instance...");
    }

    /// <summary>
    ///     Resets the Discord presence and tells listeners that the instance changed.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _discordPresenceService.Update("Browsing instances...");
        _eventService.RaiseInstanceUpdated(_instanceId);
    }

    private void HandleTabSwitch(EEditInstanceTab tab)
    {
        if (DataContext is not { } viewModel)
            return;

        if (viewModel.EditInstanceTab == tab)
            return;

        viewModel.EditInstanceTab = tab;
        _selectedInstanceTab.Classes.Remove("SettingsTabBtnActive");

        _selectedInstanceTab = tab switch
        {
            EEditInstanceTab.LOGS => LogsTabBtn,
            EEditInstanceTab.MODS => ModsTabBtn,
            EEditInstanceTab.RESOURCE_PACKS => ResourcePacksTabBtn,
            EEditInstanceTab.SHADER_PACKS => ShaderPacksTabBtn,
            EEditInstanceTab.WORLDS => WorldsTabBtn,
            EEditInstanceTab.SERVERS => ServersTabBtn,
            EEditInstanceTab.SCREENSHOTS => ScreenshotsTabBtn,
            EEditInstanceTab.SETTINGS => SettingsTabBtn,
            _ => _selectedInstanceTab
        };

        _selectedInstanceTab.Classes.Add("SettingsTabBtnActive");
    }
}
