using System.Diagnostics;
using System.Text.Json.Serialization;
using Microsoft.Win32;
using Newtonsoft.Json;

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

        private Dictionary<string, string> GetInstalledJavaVersions(RegistryKey javaKey)
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
