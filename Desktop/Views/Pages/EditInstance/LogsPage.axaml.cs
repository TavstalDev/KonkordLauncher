using System;
using Avalonia;
using Avalonia.Controls;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Pages.EditInstance;

public partial class LogsPage : UserControl
{
    private EditInstanceViewModel? _subscribedVm;

    public LogsPage()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (_subscribedVm != null)
        {
            _subscribedVm.OnScrollLogsToEndRequested -= OnScrollLogsToEndRequested;
            _subscribedVm = null;
        }

        if (DataContext is EditInstanceViewModel newVm)
        {
            _subscribedVm = newVm;
            _subscribedVm.OnScrollLogsToEndRequested += OnScrollLogsToEndRequested;
        }
    }

    private void OnScrollLogsToEndRequested(object? sender, EventArgs e)
    {
        var scroll = LogsScrollViewer.Scroll;
        if (scroll == null)
            return;
        scroll.Offset = new Vector(0, scroll.Extent.Height);
    }
}

