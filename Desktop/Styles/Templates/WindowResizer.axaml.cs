using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace Tavstal.KonkordLauncher.Desktop.Styles.Templates;

/// <summary>
/// A templated control that provides window resizing functionality through styled resize handles.
/// Automatically wires up resize handles for all eight window edges (cardinal and diagonal directions).
/// </summary>
public partial class WindowResizer : TemplatedControl
{
    /// <summary>
    /// Initializes event handlers for all window resize handles when the control template is applied.
    /// Locates and subscribes to pointer press events for all eight directional resize handles.
    /// </summary>
    /// <param name="e">The template applied event arguments containing the name scope of applied template elements.</param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        string[] edges = ["North", "South", "West", "East", "NorthWest", "NorthEast", "SouthWest", "SouthEast"];
            
        foreach (var edge in edges)
        {
            if (e.NameScope.Find<Border>($"PART_{edge}Handle") is { } handle)
                handle.PointerPressed += ResizeHandle_PointerPressed;
        }
    }

    /// <summary>
    /// Handles pointer press events on resize handle controls to initiate window resizing.
    /// Initiates a drag-to-resize operation on the parent window if it supports resizing.
    /// </summary>
    /// <param name="sender">The resize handle Border control that triggered the event. Must have a Tag containing a WindowEdge value.</param>
    /// <param name="e">The pointer press event arguments containing pointer information.</param>
    private void ResizeHandle_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border { Tag: string edgeStr })
        {
            var window = this.FindAncestorOfType<Window>();
            if (window is { CanResize: true })
            {
                if (Enum.TryParse<WindowEdge>(edgeStr, out var edge))
                    window.BeginResizeDrag(edge, e);
            }
        }
    }
}