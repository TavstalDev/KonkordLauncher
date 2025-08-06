using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views;

public partial class CreateInstanceWindow : Window
{
    public CreateInstanceWindow()
    {
        InitializeComponent();

#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif

        this.DataContext = new CreateInstanceViewModel();
        
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CreateInstanceViewModel vm) return;
        
        // TODO
    }

    private void CustomNoModLoader_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CreateInstanceViewModel vm) return;
        
        vm.ModType = EMinecraftKind.VANILLA;
    }

    private void CustomNeoForge_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CreateInstanceViewModel vm) return;
        
        vm.ModType = EMinecraftKind.NEOFORGE;
    }

    private void CustomForge_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CreateInstanceViewModel vm) return;
        
        vm.ModType = EMinecraftKind.FORGE;
    }

    private void CustomFabric_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CreateInstanceViewModel vm) return;
        
        vm.ModType = EMinecraftKind.FABRIC;
    }

    private void CustomQuilt_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CreateInstanceViewModel vm) return;
        
        vm.ModType = EMinecraftKind.QUILT;
    }

    private async void IconSelector_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: Replace async void
        if (DataContext is not CreateInstanceViewModel vm) 
            return;
        
        IconSelectorWindow window = new();
        var result = await window.ShowDialog<IconDataModel>(this);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (result == null)
            return;
        vm.InstanceIcon = result.Image;
    }
}