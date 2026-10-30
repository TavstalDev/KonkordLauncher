using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Pages.CreateInstance;

public partial class ModpackPage : UserControl
{
    public ModpackPage()
    {
        InitializeComponent();
    }

    private void ScrollViewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (DataContext is not CreateInstanceViewModel viewModel)
            return;

        if (!viewModel.Modpack.AllowScrollbarRefresh)
            return;

        if (sender is ScrollViewer scrollViewer)
        {
            var verticalOffset = scrollViewer.Offset.Y;
            var maxVerticalOffset = scrollViewer.Extent.Height - scrollViewer.Viewport.Height;

            if (maxVerticalOffset < 0 || Math.Abs(verticalOffset - maxVerticalOffset) < 0.1)
                Dispatcher.Invoke(async () => await viewModel.Modpack.RefreshModpacksAsync());
        }
    }

    private void ModPackCategory_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CreateInstanceViewModel viewModel)
            return;

        if (!viewModel.Modpack.AllowScrollbarRefresh)
            return;

        Dispatcher.Invoke(async () => await viewModel.Modpack.RefreshModpacksAsync(true));
    }

    private void ModPackFilter_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not CreateInstanceViewModel viewModel)
            return;

        if (!viewModel.Modpack.AllowScrollbarRefresh)
            return;

        Dispatcher.Invoke(async () => await viewModel.Modpack.RefreshModpacksAsync(true));
    }
}

