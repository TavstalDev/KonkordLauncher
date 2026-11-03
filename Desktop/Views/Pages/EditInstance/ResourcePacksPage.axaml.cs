using Avalonia.Controls;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Pages.EditInstance;

public partial class ResourcePacksPage : UserControl
{
    public ResourcePacksPage()
    {
        InitializeComponent();
    }

    private void ResourcePacksDataGrid_OnCellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        if (DataContext is not EditInstanceViewModel vm)
            return;

        var row = e.Row;
        if (row.DataContext is not ResourceBaseModel)
            return;

        vm.ResourcePacks.SaveResourcePacks();
    }
}

