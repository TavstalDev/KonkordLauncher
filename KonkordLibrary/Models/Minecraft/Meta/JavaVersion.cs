using Microsoft.Win32;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace KonkordLibrary.Models.Minecraft.Meta
{
    public class JavaVersion
    {
        [JsonPropertyName("component"), JsonProperty("component")]
        public string Component {  get; set; }
        [JsonPropertyName("majorVersion"), JsonProperty("majorVersion")]
        public int MajorVersion { get; set; }

        public JavaVersion(string component, int majorVersion)
        {
            Component = component;
            MajorVersion = majorVersion;
        }

        public JavaVersion() { }

        /// <summary>
        /// Finds the Java version installed on the system.
        /// </summary>
        /// <param name="path">The path to the Java version found, if any.</param>
        public void FindOnSystem(out string? path)
        {
            path = null;
            // Registry key for 64-bit Java installations
            RegistryKey localMachine64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            RegistryKey? javaKey64 = localMachine64.OpenSubKey(@"SOFTWARE\JavaSoft\Java Runtime Environment");

            if (javaKey64 == null)
                return;   

            Dictionary<string, string> versions = GetInstalledJavaVersions(javaKey64);

            foreach (var version in versions.ToList())
            {
                string[] raw = version.Key.Split('.');
                if (raw[1] == MajorVersion.ToString())
                {
                    path = version.Value;
                    break;
                }
            }

            return;
        }

        /// <summary>
        /// Finds the specified major version of Java installed on the system.
        /// </summary>
        /// <param name="majorVersion">The major version of Java to find.</param>
        /// <param name="path">The path to the Java version found, if any.</param>
        public static void FindOnSystem(int majorVersion, out string? path)
        {
            path = null;
            // Registry key for 64-bit Java installations
            RegistryKey localMachine64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            RegistryKey? javaKey64 = localMachine64.OpenSubKey(@"SOFTWARE\JavaSoft\Java Runtime Environment");

            if (javaKey64 == null)
                return;

            Dictionary<string, string> versions = GetInstalledJavaVersions(javaKey64);

            foreach (var version in versions.ToList())
            {
                string[] raw = version.Key.Split('.');
                if (raw[1] == majorVersion.ToString())
                {
                    path = version.Value;
                    break;
                }
            }

            return;
        }

        /// <summary>
        /// Gets the installed Java versions from the specified registry key.
        /// </summary>
        /// <param name="javaKey">The registry key containing the Java versions.</param>
        /// <returns>
        /// A dictionary containing the installed Java versions, where the key is the version and the value is the installation path.
        /// </returns>
        private static Dictionary<string, string> GetInstalledJavaVersions(RegistryKey javaKey)
        {
            Dictionary<string, string> local = new Dictionary<string, string>();
            if (javaKey != null)
            {
                string[] versionSubKeys = javaKey.GetSubKeyNames();
                foreach (string version in versionSubKeys)
                {
                    using (RegistryKey? key = javaKey.OpenSubKey(version))
                    {
                        string? javaPath = key?.GetValue("JavaHome")?.ToString();
                        if (!string.IsNullOrEmpty(javaPath))
                        {
                            local.Add(version, javaPath);
                        }
                    }
                }
            }
            return local;
        }
    }
}
