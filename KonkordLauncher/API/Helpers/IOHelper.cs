using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KonkordLauncher.API.Helpers
{
    public static class IOHelper
    {
        private static readonly string _appDataRoamingFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string AppDataRoamingFolder { get {  return _appDataRoamingFolder; } }

        private static readonly string _mainDirectory = Path.Combine(_appDataRoamingFolder, ".konkordlauncher");
        public static string MainDirectory { get { return _mainDirectory; } }

        private static readonly string _instancesFolder = Path.Combine(_mainDirectory, "Instances");
        public static string InstancesFolder { get { return _instancesFolder; } }

        private static readonly string _translationsFolder = Path.Combine(_mainDirectory, "Translations");
        public static string TranslationsFolder { get { return _translationsFolder; } }

        private static readonly string _versionsFolder = Path.Combine(_mainDirectory, "Versions");
        public static string VersionsFolder { get { return _versionsFolder; } }

        public static bool ValidateDataFolder()
        {
            try
            {
                if (!Directory.Exists(MainDirectory))
                {
                    Directory.CreateDirectory(MainDirectory);
                    Directory.CreateDirectory(InstancesFolder);
                    Directory.CreateDirectory(TranslationsFolder);
                    Directory.CreateDirectory(VersionsFolder);
                }

                return true;
            }
            catch (Exception ex)
            {
                
                return false;
            }
        }
    }
}
