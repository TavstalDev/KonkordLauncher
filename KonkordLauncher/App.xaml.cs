using System;
using System.Reflection;
using System.Windows;

namespace Tavstal.KonkordLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly System.Version _version = Assembly.GetExecutingAssembly().GetName().Version;
        private static readonly DateTime _buildDate = new DateTime(2000, 1, 1).AddDays(_version.Build).AddSeconds(_version.Revision * 2);
        public static System.Version Version { get { return _version; } }
        public static DateTime BuildDate { get { return _buildDate; } }
    }

}
