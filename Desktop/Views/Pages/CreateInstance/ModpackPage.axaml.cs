using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Pages.CreateInstance;

public partial class ModpackPage : UserControl
{
    private bool _refreshing;

    public ModpackPage()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ModpackList.AddHandler(ScrollViewer.ScrollChangedEvent, OnListScrolled);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        ModpackList.RemoveHandler(ScrollViewer.ScrollChangedEvent, OnListScrolled);
    }

    private void ModPackFilter_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not CreateInstanceViewModel viewModel)
            return;

        if (!viewModel.Modpack.AllowScrollbarRefresh)
            return;

        Dispatcher.Invoke(async () => await viewModel.Modpack.RefreshModpacksAsync(true));
    }

    private void OnListScrolled(object? sender, ScrollChangedEventArgs e)
    {
        if (_refreshing)
            return;

        if (DataContext is not CreateInstanceViewModel viewModel)
            return;

        if (!viewModel.Modpack.AllowScrollbarRefresh)
            return;

        if (e.Source is not ScrollViewer scrollViewer)
            return;

        var verticalOffset = scrollViewer.Offset.Y;
        var maxVerticalOffset = scrollViewer.Extent.Height - scrollViewer.Viewport.Height;

        if (maxVerticalOffset < 0 || Math.Abs(verticalOffset - maxVerticalOffset) < 0.1)
        {
            Dispatcher.Invoke(async () =>
            {
                try
                {
                    _refreshing = true;
                    await viewModel.Modpack.RefreshModpacksAsync();
                }
                finally
                {
                    _refreshing = false;
                }
            });
        }
    }
}

