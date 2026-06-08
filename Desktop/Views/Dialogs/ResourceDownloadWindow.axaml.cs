using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

/// <summary>
/// Represents the dialog window used to browse and download resources (for example, mods)
/// for a specific launcher instance.
/// </summary>
public partial class ResourceDownloadWindow : KonkordWindow<ResourceDownloadViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceDownloadWindow"/> class
    /// using default values. This overload primarily exists for designer/serialization scenarios.
    /// </summary>
    [RequiresUnreferencedCode( "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public ResourceDownloadWindow() : this(null!, EResourceType.MOD) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceDownloadWindow"/> class.
    /// </summary>
    /// <param name="instance">The target launcher instance the resource is associated with.</param>
    /// <param name="resourceType">The type of resource to display and download.</param>
    [RequiresUnreferencedCode( "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public ResourceDownloadWindow(Instance instance, EResourceType resourceType)
    {
        InitializeComponent();
        
        DataContext = new ResourceDownloadViewModel(this, instance, resourceType);

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
    /// Handles filter selection changes and refreshes the resource list from the first page.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">Selection change event data.</param>
    private void Filter_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not { } viewModel)
            return;
        
        if (!viewModel.AllowScrollbarRefresh)
            return;
        
        Dispatcher.UIThread.Invoke(async () =>  await viewModel.RefreshResourcesAsync(true));
    }

    /// <summary>
    /// Handles category checkbox state changes and refreshes the resource list from the first page.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">Routed event data.</param>
    private void Category_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not { } viewModel)
            return;
        
        if (!viewModel.AllowScrollbarRefresh || !viewModel.IsMod)
            return;
        
        Dispatcher.UIThread.Invoke(async () =>  await viewModel.RefreshResourcesAsync(true));
    }

    /// <summary>
    /// Handles scroll changes and triggers incremental loading when the user reaches the bottom.
    /// </summary>
    /// <param name="sender">The scroll viewer that raised the event.</param>
    /// <param name="e">Scroll change event data.</param>
    private void ScrollViewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (DataContext is not { } viewModel)
            return;
        
        if (!viewModel.AllowScrollbarRefresh)
            return;
        
        if (sender is ScrollViewer scrollViewer)
        {
            double verticalOffset = scrollViewer.Offset.Y;
            double maxVerticalOffset = scrollViewer.Extent.Height - scrollViewer.Viewport.Height;

            if (maxVerticalOffset < 0 || Math.Abs(verticalOffset - maxVerticalOffset) < 0.1)
                Dispatcher.UIThread.Invoke(async () => await viewModel.RefreshResourcesAsync());
        }
    }
}