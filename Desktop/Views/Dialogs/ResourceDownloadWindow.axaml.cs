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

public partial class ResourceDownloadWindow : KonkordWindow<ResourceDownloadViewModel>
{
    [RequiresUnreferencedCode( "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public ResourceDownloadWindow() : this(null!, EResourceType.MOD) { }
    
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

    private void Filter_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not { } viewModel)
            return;
        
        if (!viewModel.AllowScrollbarRefresh)
            return;
        
        Dispatcher.UIThread.Invoke(async () =>  await viewModel.RefreshResourcesAsync(true));
    }

    private void Category_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not { } viewModel)
            return;
        
        if (!viewModel.AllowScrollbarRefresh || !viewModel.IsMod)
            return;
        
        Dispatcher.UIThread.Invoke(async () =>  await viewModel.RefreshResourcesAsync(true));
    }

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