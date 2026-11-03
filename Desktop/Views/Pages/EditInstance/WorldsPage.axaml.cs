using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Pages.EditInstance;

public partial class WorldsPage : UserControl
{
    private EditInstanceViewModel? _subscribedVm;

    public WorldsPage()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (_subscribedVm != null)
        {
            _subscribedVm.OnWorldRenameRequested -= OnWorldRenameRequested;
            _subscribedVm = null;
        }

        if (DataContext is EditInstanceViewModel newVm)
        {
            _subscribedVm = newVm;
            _subscribedVm.OnWorldRenameRequested += OnWorldRenameRequested;
        }
    }

    private void OnWorldRenameRequested(object? sender, EventArgs e)
    {
        WorldsTable.BeginEdit();
    }

    private void WorldDataGrid_OnCellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        if (DataContext is not EditInstanceViewModel vm)
            return;

        var row = e.Row;
        if (row.DataContext is not WorldModel)
            return;

        vm.Worlds.SaveWorlds();
    }

    private void WorldDataGrid_OnRowEditEnded(object? sender, DataGridRowEditEndedEventArgs e)
    {
        if (DataContext is not EditInstanceViewModel vm)
            return;

        var row = e.Row;
        if (row.DataContext is not WorldModel)
            return;

        vm.Worlds.SaveWorlds();
    }
}

