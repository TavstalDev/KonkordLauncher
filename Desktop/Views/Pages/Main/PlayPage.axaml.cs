using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Tavstal.KonkordLauncher.Desktop.Views.Pages.Main;

public partial class PlayPage : UserControl
{
    public PlayPage()
    {
        InitializeComponent();
    }

    private void InstanceFlyoutBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        var flyout = (MenuFlyout)Resources["InstanceMenu"]!;
        foreach (var item in flyout.Items.OfType<MenuItem>())
            item.CommandParameter = btn.DataContext;
        flyout.ShowAt(btn);
    }

    private void InstanceMenu_Opening(object? sender, EventArgs e)
    {
        if (sender is MenuFlyout { Target: not null } flyout)
        {
            foreach (var item in flyout.Items)
            {
                if (item is StyledElement element)
                    element.DataContext = flyout.Target.DataContext;
            }
        }
    }
}

