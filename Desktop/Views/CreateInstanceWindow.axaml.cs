using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Primitives;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views;

public partial class CreateInstanceWindow : KonkordWindow<CreateInstanceViewModel>
{
    private readonly IDiscordPresenceService _discordPresenceService = null!;
    private Button _selectedImportTypeBtn;
    private Button _selectedTabBtn;

    public CreateInstanceWindow()
    {
        InitializeComponent();
        _selectedTabBtn = CustomTabBtn;
        _selectedImportTypeBtn = ImportFromFileTabBtn;
    }

    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public CreateInstanceWindow(CreateInstanceViewModel viewModel, ITranslationService translationService,
        IWindowFactory windowFactory, IDiscordPresenceService discordPresenceService)
    {
        InitializeComponent();
        _selectedTabBtn = CustomTabBtn;
        _selectedImportTypeBtn = ImportFromFileTabBtn;

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
            });
            DataContext.SwitchImportTabInteraction.RegisterHandler(action =>
            {
                HandleImportTypeChange(action.Input);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            });
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

    private void HandleImportTypeChange(int index)
    {
        if (DataContext is not { } viewModel)
            return;

        if (viewModel.Import.SelectedImportSourceIndex == index)
            return;

        viewModel.Import.SelectedImportSourceIndex = index;
        viewModel.Import.HasImportPath = false;
        viewModel.Import.ImportPath = null;
        _selectedImportTypeBtn.Classes.Remove("SuccessBtn");
        switch (index)
        {
            case 0:
            {
                _selectedImportTypeBtn = ImportFromFileTabBtn;
                break;
            }
            case 1:
            {
                _selectedImportTypeBtn = ImportFromUrlTabBtn;
                break;
            }
        }

        _selectedImportTypeBtn.Classes.Add("SuccessBtn");
    }

    #region Events

    /// <inheritdoc />
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        _discordPresenceService.Update("Creating instance...");
    }

    /// <inheritdoc />
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _discordPresenceService.Update("Browsing instances...");
    }

    private void ScrollViewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (DataContext is not { } viewModel)
            return;

        if (!viewModel.Modpack.AllowScrollbarRefresh)
            return;

        if (sender is ScrollViewer scrollViewer)
        {
            var verticalOffset = scrollViewer.Offset.Y;
            var maxVerticalOffset = scrollViewer.Extent.Height - scrollViewer.Viewport.Height;

            if (maxVerticalOffset < 0 || Math.Abs(verticalOffset - maxVerticalOffset) < 0.1)
                Dispatcher.UIThread.Invoke(async () => await viewModel.Modpack.RefreshModpacksAsync());
        }
    }

    private void ModPackCategory_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not { } viewModel)
            return;

        if (!viewModel.Modpack.AllowScrollbarRefresh)
            return;

        Dispatcher.UIThread.Invoke(async () => await viewModel.Modpack.RefreshModpacksAsync(true));
    }

    private void ModPackFilter_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not { } viewModel)
            return;

        if (!viewModel.Modpack.AllowScrollbarRefresh)
            return;

        Dispatcher.UIThread.Invoke(async () => await viewModel.Modpack.RefreshModpacksAsync(true));
    }

    #endregion
}
