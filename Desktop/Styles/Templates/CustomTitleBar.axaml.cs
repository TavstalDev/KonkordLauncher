using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Tavstal.KonkordLauncher.Desktop.Styles.Templates;

/// <summary>
/// Custom title bar control for templated windows in the Konkord launcher.
/// Provides window control functionality including minimize, maximize, close, and drag-to-move operations.
/// </summary>
public class CustomTitleBar : TemplatedControl
{
    /// <summary>
    /// Wires up event handlers for title bar controls when the template is applied.
    /// Connects the drag area for window moving and buttons for window state management.
    /// </summary>
    /// <param name="e">The template applied event arguments containing the name scope of template elements.</param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var dragArea = e.NameScope.Find<Border>("PART_DragArea");
        if (dragArea != null)
            dragArea.PointerPressed += DragStart_PointerPressed;
        
        // Minimize Button
        var minBtn = e.NameScope.Find<Button>("PART_MinimizeButton");
        if (minBtn is not null)
            minBtn.Click += MinBtnOnClick;

        // Maximize Button
        var maxBtn = e.NameScope.Find<Button>("PART_MaximizeButton");
        if (maxBtn is not null)
            maxBtn.Click += MaxBtnOnClick;

        // Close Button
        var closeBtn = e.NameScope.Find<Button>("PART_CloseButton");
        if (closeBtn is not null)
            closeBtn.Click += CloseBtnOnClick;
    }

    /// <summary>
    /// Handles the minimize button click event.
    /// Sets the parent window's state to Minimized.
    /// </summary>
    /// <param name="sender">The minimize button control that triggered the event.</param>
    /// <param name="e">The routed event arguments.</param>
    private void MinBtnOnClick(object? sender, RoutedEventArgs e)
    {
        var window = this.FindAncestorOfType<Window>();
        window?.WindowState = WindowState.Minimized;
    }

    /// <summary>
    /// Handles the maximize button click event.
    /// Toggles the parent window between maximized and normal states.
    /// </summary>
    /// <param name="sender">The maximize button control that triggered the event.</param>
    /// <param name="e">The routed event arguments.</param>
    private void MaxBtnOnClick(object? sender, RoutedEventArgs e)
    {
        var window = this.FindAncestorOfType<Window>();
        window?.WindowState = window.WindowState == WindowState.Maximized 
                ? WindowState.Normal 
                : WindowState.Maximized;
    }

    /// <summary>
    /// Handles the close button click event.
    /// Closes the parent window.
    /// </summary>
    /// <param name="sender">The close button control that triggered the event.</param>
    /// <param name="e">The routed event arguments.</param>
    private void CloseBtnOnClick(object? sender, RoutedEventArgs e)
    {
        var window = this.FindAncestorOfType<Window>();
        window?.Close();
    }

    /// <summary>
    /// Handles pointer press events on the drag area to enable window dragging.
    /// Initiates a drag operation that allows the user to move the window by its title bar.
    /// </summary>
    /// <param name="sender">The drag area Border control that triggered the event.</param>
    /// <param name="e">The pointer press event arguments containing pointer information.</param>
    private void DragStart_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var window = this.FindAncestorOfType<Window>();
        window?.BeginMoveDrag(e);
    }
}