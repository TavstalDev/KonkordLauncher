using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.Primitives;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

/// <summary>
///     A dialog window that shows an alert message.
/// </summary>
public partial class AlertWindow : KonkordWindow<AlertViewModel>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AlertWindow" /> class for the designer.
    /// </summary>
    public AlertWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            DataContext = new AlertViewModel("Design Time Title", "This is a design time message.", EAlertType.Info);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AlertWindow" /> class with the given content.
    /// </summary>
    /// <param name="title">The title of the alert.</param>
    /// <param name="message">The message shown in the alert.</param>
    /// <param name="type">The alert type, which sets its look and behavior.</param>
    /// <param name="bitmapService">The service used to load bitmaps.</param>
    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public AlertWindow(string title, string message, EAlertType type, IBitmapService bitmapService) : base(bitmapService)
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = new AlertViewModel("Design Time Title", "This is a design time message.", EAlertType.Info);
            return;
        }

        // Sets the DataContext to an instance of AlertViewModel with the provided parameters.
        DataContext = new AlertViewModel(title, message, type);
        this.WhenActivated(disposables =>
        {
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.ClickYesInteraction.RegisterHandler(action =>
            {
                Close(true);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
        });
    }

    /// <summary>
    ///     Handles the window loaded event and applies the alert color to the icon based on the current
    ///     alert type.
    /// </summary>
    /// <param name="e">The event data for the loaded event.</param>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (Design.IsDesignMode)
            return;

        if (DataContext == null)
            return;

        // Retrieves the color resource associated with the alert type and applies it to the icon.
        if (this.FindResource(DataContext.GetIconColor) is SolidColorBrush brush)
            AlertIcon.Foreground = brush;
    }
}
