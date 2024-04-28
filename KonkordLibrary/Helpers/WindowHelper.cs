using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Tavstal.KonkordLibrary.Models.Launcher;

namespace Tavstal.KonkordLibrary.Helpers
{
    public static class WindowHelper
    {
        private static readonly System.Version _version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0, 0); // New Version() is used to fix null warning
        private static readonly DateTime _buildDate = new DateTime(2000, 1, 1).AddDays(_version.Build).AddSeconds(_version.Revision * 2);
        public static System.Version Version { get { return _version; } }
        public static DateTime BuildDate { get { return _buildDate; } }

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

        /// <summary>
        /// Finds a visual child of type T within the specified parent element.
        /// </summary>
        /// <typeparam name="T">The type of the visual child to find.</typeparam>
        /// <param name="parent">The parent element to search within.</param>
        /// <returns>
        /// The visual child of type T if found; otherwise, null.
        /// </returns>
        public static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child == null)
                    return default;

                if (child is T)
                    return (T)child;
                else
                {
                    T? childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return default;
        }

        /// <summary>
        /// Asynchronously retrieves an image source from the specified file path.
        /// </summary>
        /// <param name="filePath">The file path of the image.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains the image source.
        /// </returns>
        public static async Task<ImageSource> GetImageSourceAsync(string filePath)
        {
            return GetImageSource(await File.ReadAllBytesAsync(filePath));
        }

        /// <summary>
        /// Retrieves an image source from the specified byte array.
        /// </summary>
        /// <param name="bytes">The byte array containing image data.</param>
        /// <returns>
        /// The image source extracted from the byte array.
        /// </returns>
        public static ImageSource GetImageSource(byte[] bytes)
        {
            BitmapImage bi;
            using (var ms = new MemoryStream(bytes))
            {
                bi = new BitmapImage();
                bi.BeginInit();
                bi.CreateOptions = BitmapCreateOptions.None;
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.StreamSource = ms;
                bi.EndInit();
            }
            return bi;
        }

        /// <summary>
        /// Retrieves an image source from the specified URI.
        /// </summary>
        /// <param name="path">The URI of the image.</param>
        /// <returns>
        /// The image source extracted from the URI.
        /// </returns>
        public static ImageSource GetImageSourceFromUri(string path)
        {
            return new BitmapImage(new Uri(path.StartsWith("/assets") ? "pack://application:,,," + path : path));
        }
    }
}
