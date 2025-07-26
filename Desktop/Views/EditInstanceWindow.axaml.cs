using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.ViewModels;

namespace Tavstal.KonkordLauncher.Desktop.Views;

public partial class EditInstanceWindow : Window
{
    public EditInstanceWindow()
    {
        InitializeComponent();

#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif
        
        
    }
}