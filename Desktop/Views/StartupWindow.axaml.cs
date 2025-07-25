using Avalonia;
using Avalonia.Controls;

namespace Tavstal.KonkordLauncher.Desktop.Views;

public partial class StartupWindow : Window
{
    public StartupWindow()
    {
        InitializeComponent();

#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif
        
    }
}