using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views;

/// <summary>
/// Represents the window for creating a new instance in the Konkord Launcher.
/// </summary>
public partial class CreateInstanceWindow : KonkordWindow<CreateInstanceViewModel>
{
    private Button _selectedTabBtn;
    private Button _selectedImportTypeBtn;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateInstanceWindow"/> class.
    /// Sets up the data context, initializes components, and registers reactive handlers.
    /// </summary>
    public CreateInstanceWindow()
    {
        InitializeComponent();

#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif
        
        DataContext = new CreateInstanceViewModel();
        _selectedTabBtn = CustomTabBtn;
        _selectedImportTypeBtn = ImportFromFileTabBtn;
        
        this.WhenActivated(disposables =>
        {
            DataContext.MinimizeWindowInteraction.RegisterHandler(action =>
            {
                WindowState = WindowState.Minimized;
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.MaximizeWindowInteraction.RegisterHandler(action =>
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.UpdateSelectedTabButton.RegisterHandler(action =>
            {
                HandleTabChange(action.Input);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            });
            DataContext.UpdateSelectedImportTypeButton.RegisterHandler(action =>
            {
                HandleImportTypeChange(action.Input);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            });
            DataContext.ShowAlertDialog.RegisterHandler(async action =>
            {
                AlertWindow alertWindow = new(action.Input.Title, action.Input.Message, action.Input.Type);
                await alertWindow.ShowDialog(this);
                action.SetOutput(Unit.Default);
            }).DisposeWith(disposables);
            DataContext.ShowIconSelector.RegisterHandler(async action =>
            {
                IconSelectorWindow window = new();
                var result = await window.ShowDialog<string?>(this);
                action.SetOutput(result);
            }).DisposeWith(disposables);
            DataContext.ShowFileSelector.RegisterHandler(async action =>
            {
                string title = TranslationManager.Translate("common.select.file");
                string? result = await OpenFilePickerAsync(title, ".zip, .mrpack, .json", ["*.zip", "*.mrpack", "*.json"]);
                action.SetOutput(result);
            }).DisposeWith(disposables);
        });
    }

    #region Events
    /// <summary>
    /// Called when the window is opened.
    /// Updates the Rich Presence status to indicate that an instance is being created.
    /// </summary>
    /// <param name="e">The event arguments for the opened event.</param>
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        App.UpdateRPC("Creating instance...");
    }

    /// <summary>
    /// Called when the window is closed.
    /// Updates the Rich Presence status to indicate that the user is browsing instances.
    /// </summary>
    /// <param name="e">The event arguments for the closed event.</param>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        App.UpdateRPC("Browsing instances...");
    }
    
    #endregion

    /// <summary>
    /// Handles switching the visible/create-instance tab and updates the visual state of the tab buttons.
    /// </summary>
    /// <param name="tab">The tab to switch to.</param>
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

    /// <summary>
    /// Handles switching the import type selection (file vs. URL) and updates related view model state and visual state.
    /// </summary>
    /// <param name="index">The index representing the chosen import source (0 = file, 1 = url).</param>
    private void HandleImportTypeChange(int index)
    {
        if (DataContext is not { } viewModel)
            return;
        
        if (viewModel.SelectedImportSourceIndex  == index)
            return;
        
        viewModel.SelectedImportSourceIndex = index;
        viewModel.HasImportPath = false;
        viewModel.ImportPath = null;
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

    private void ScrollViewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (DataContext is not { } viewModel)
            return;
        
        if (!viewModel.ModpackAllowScrollbarRefresh)
            return;
        
        if (sender is ScrollViewer scrollViewer)
        {
            double verticalOffset = scrollViewer.Offset.Y;
            double maxVerticalOffset = scrollViewer.Extent.Height - scrollViewer.Viewport.Height;

            if (maxVerticalOffset < 0 || Math.Abs(verticalOffset - maxVerticalOffset) < 0.1)
                Dispatcher.UIThread.Invoke(async () => await viewModel.RefreshModpacksAsync());
        }
    }

    private void ModPackCategory_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not { } viewModel)
            return;
        
        if (!viewModel.ModpackAllowScrollbarRefresh)
            return;
        
        Dispatcher.UIThread.Invoke(async () =>  await viewModel.RefreshModpacksAsync(true));
    }

    private void ModPackFilter_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not { } viewModel)
            return;
        
        if (!viewModel.ModpackAllowScrollbarRefresh)
            return;
        
        Dispatcher.UIThread.Invoke(async () =>  await viewModel.RefreshModpacksAsync(true));
    }
}