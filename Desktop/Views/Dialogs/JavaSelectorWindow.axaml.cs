using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using JavaSelectorViewModel = Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models.JavaSelectorViewModel;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

/// <summary>
/// Represents a window for selecting a Java version.
/// </summary>
public partial class JavaSelectorWindow : KonkordWindow<JavaSelectorViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JavaSelectorWindow"/> class.
    /// Sets up the data context and handles language changes.
    /// </summary>
    public JavaSelectorWindow()
    {
        InitializeComponent();

#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif

        if (Design.IsDesignMode)
            DataContext = new JavaSelectorViewModel();
        else
        {
            var settings = LauncherHelper.GetLauncherSettings();
            DataContext = new JavaSelectorViewModel(settings.Launcher.JavaDirectoryPath);
            this.WhenActivated(disposables =>
            {
                DataContext.MinimizeWindowInteraction.RegisterHandler(action =>
                {
                    WindowState = WindowState.Minimized;
                    action.SetOutput(Unit.Default);
                    return Task.CompletedTask;
                }).DisposeWith(disposables);
                DataContext.MaximizeWindowInteraction.RegisterHandler(action =>
                {
                    WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                    action.SetOutput(Unit.Default);
                    return Task.CompletedTask;
                }).DisposeWith(disposables);
                DataContext.CloseWindowInteraction.RegisterHandler(action =>
                {
                    Close(action.Input);
                    action.SetOutput(Unit.Default);
                    return Task.CompletedTask;
                }).DisposeWith(disposables);
            });
        }
    }
}