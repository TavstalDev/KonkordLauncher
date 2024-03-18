using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace KonkordLauncher.API.Helpers
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

        public static childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            if (obj == null)
                return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                {
                    return (childItem)child;
                }
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        public static void Resize(ref ListBox listBox, double defaultHeight, double defaultWidth, Thickness defaultMargin, double heightMulti, double widthMulti)
        {
            listBox.Resources["ListBorderHeight"] = defaultHeight * heightMulti;
            listBox.Resources["ListBorderWidth"] = defaultWidth * widthMulti;
            listBox.Resources["ListBorderMargin"] = new Thickness
            {
                Bottom = defaultMargin.Bottom * heightMulti,
                Left = defaultMargin.Left * widthMulti,
                Right = defaultMargin.Right * widthMulti,
                Top = defaultMargin.Top * heightMulti,
            };
        }

        public static void ResizeFont(ref ListBox listBox, double defaultFont, double defaultHeight, double defaultWidth, Thickness defaultMargin, double heightMulti, double widthMulti)
        {
            listBox.Resources["ListLabelHeight"] = defaultHeight * heightMulti;
            listBox.Resources["ListLabelWidth"] = defaultWidth * widthMulti;
            listBox.Resources["ListLabelMargin"] = new Thickness
            {
                Bottom = defaultMargin.Bottom * heightMulti,
                Left = defaultMargin.Left * widthMulti,
                Right = defaultMargin.Right * widthMulti,
                Top = defaultMargin.Top * heightMulti,
            };

            listBox.Resources["ListLabelFontSize"] = defaultFont * widthMulti;
        }
    }
}
