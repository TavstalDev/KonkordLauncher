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
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

/// <summary>
///     A dialog window for browsing and downloading resources, such as mods, for an instance.
/// </summary>
public partial class ResourceDownloadWindow : KonkordWindow<ResourceDownloadViewModel>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ResourceDownloadWindow" /> class for the designer.
    /// </summary>
    public ResourceDownloadWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ResourceDownloadWindow" /> class
    ///     with the given view model.
    /// </summary>
    /// <param name="viewModel">The view model that holds the window data.</param>
    /// <param name="bitmapService">The service used to load bitmaps.</param>
    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public ResourceDownloadWindow(ResourceDownloadViewModel viewModel, IBitmapService bitmapService) : base(bitmapService)
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;

        DataContext = viewModel;

        this.WhenActivated(disposables =>
        {
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close(action.Input);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
        });
    }

    /// <summary>
    ///     Refreshes the resource list from the first page when the filter changes.
    /// </summary>
    private void Filter_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not { } viewModel)
            return;

        if (!viewModel.AllowScrollbarRefresh)
            return;

        Dispatcher.UIThread.Invoke(async () => await viewModel.RefreshResourcesAsync(true));
    }

    /// <summary>
    ///     Refreshes the resource list from the first page when a category is checked or unchecked.
    /// </summary>
    private void Category_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not { } viewModel)
            return;

        if (!viewModel.AllowScrollbarRefresh || !viewModel.IsMod)
            return;

        Dispatcher.UIThread.Invoke(async () => await viewModel.RefreshResourcesAsync(true));
    }

    /// <summary>
    ///     Loads more resources when the user scrolls to the bottom.
    /// </summary>
    private void ScrollViewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (DataContext is not { } viewModel)
            return;

        if (!viewModel.AllowScrollbarRefresh)
            return;

        if (sender is ScrollViewer scrollViewer)
        {
            var verticalOffset = scrollViewer.Offset.Y;
            var maxVerticalOffset = scrollViewer.Extent.Height - scrollViewer.Viewport.Height;

            if (maxVerticalOffset < 0 || Math.Abs(verticalOffset - maxVerticalOffset) < 0.1)
                Dispatcher.UIThread.Invoke(async () => await viewModel.RefreshResourcesAsync());
        }
    }
}
