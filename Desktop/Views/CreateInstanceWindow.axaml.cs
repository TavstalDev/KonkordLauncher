using Avalonia;
using Avalonia.Controls;

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
        
    }
}