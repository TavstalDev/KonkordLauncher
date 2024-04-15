using System.Windows;
using System.Windows.Controls;

namespace KonkordLibrary.Helpers
{
    public static class WindowHelper
    {
        /// <summary>
        /// Resizes a FrameworkElement based on the provided height and width multipliers.
        /// </summary>
        /// <param name="element">The FrameworkElement to resize.</param>
        /// <param name="heightMulti">The multiplier for the height.</param>
        /// <param name="widthMulti">The multiplier for the width.</param>
        public static void Resize(FrameworkElement element, double heightMulti, double widthMulti)
        {
            element.Height = element.Height * heightMulti;
            element.Width = element.Width * widthMulti;
            element.Margin = new Thickness
            {
                Bottom = element.Margin.Bottom * heightMulti,
                Left = element.Margin.Left * widthMulti,
                Right = element.Margin.Right * widthMulti,
                Top = element.Margin.Top * heightMulti,
            };
        }

        /// <summary>
        /// Resizes the font of a Control based on the provided height and width multipliers.
        /// </summary>
        /// <param name="element">The Control whose font to resize.</param>
        /// <param name="heightMulti">The multiplier for the height.</param>
        /// <param name="widthMulti">The multiplier for the width.</param>
        public static void ResizeFont(Control element, double heightMulti, double widthMulti)
        {
            element.Height = element.Height * heightMulti;
            element.Width = element.Width * widthMulti;
            element.Margin = new Thickness
            {
                Bottom = element.Margin.Bottom * heightMulti,
                Left = element.Margin.Left * widthMulti,
                Right = element.Margin.Right * widthMulti,
                Top = element.Margin.Top * heightMulti,
            };
            element.FontSize = element.FontSize * widthMulti;
        }
    }
}
