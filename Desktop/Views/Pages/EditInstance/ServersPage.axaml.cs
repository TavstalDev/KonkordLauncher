using Avalonia.Controls;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Pages.EditInstance;

public partial class ServersPage : UserControl
{
    public ServersPage()
    {
        InitializeComponent();
    }

    private void ServersDataGrid_OnRowEditEnded(object? sender, DataGridRowEditEndedEventArgs e)
    {
        if (DataContext is not EditInstanceViewModel vm)
            return;

        var row = e.Row;
        if (row.DataContext is not ServerModel)
            return;

        vm.Servers.SaveServers();
    }
}

