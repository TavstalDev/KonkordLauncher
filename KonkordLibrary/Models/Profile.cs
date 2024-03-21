using KonkordLibrary.Enums;
using KonkordLibrary.Helpers;
using System.IO;
using System.Text.Json.Serialization;

namespace KonkordLibrary.Models
{
    [Serializable]
    public class Profile
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("icon")]
        public string Icon { get; set; }
        [JsonPropertyName("versionId")]
        public string VersionId { get; set; }
        [JsonPropertyName("type")]
        public EProfileType Type { get; set; }
        [JsonPropertyName("kind")]
        public EProfileKind Kind { get; set; }
        [JsonPropertyName("resolution")]
        public Resolution Resolution { get; set; }
        [JsonPropertyName("gameDirectory")]
        public string GameDirectory { get; set; }
        [JsonPropertyName("javaPath")]
        public string JavaPath { get; set; }
        [JsonPropertyName("jvmArgs")]
        public string JVMArgs { get; set; }
        [JsonPropertyName("memory")]
        public int Memory { get; set; }
        [JsonPropertyName("launcherVisibility")]
        public ELaucnherVisibility LauncherVisibility { get; set; }

        public Profile() { }

        public Profile(string name, string icon, string versionId, EProfileType type, EProfileKind kind, Resolution resolution, string gameDirectory, string javaPath, string jVMArgs, int memory, ELaucnherVisibility launcherVisibility)
        {
            Name = name;
            Icon = icon;
            VersionId = versionId;
            Type = type;
            Kind = kind;
            Resolution = resolution;
            GameDirectory = gameDirectory;
            JavaPath = javaPath;
            JVMArgs = jVMArgs;
            Memory = memory;
            LauncherVisibility = launcherVisibility;
        }

        public Profile(string name, string icon, string versionId, EProfileType type, EProfileKind kind, Resolution resolution, int memory, ELaucnherVisibility launcherVisibility)
        {
            Name = name;
            Icon = icon;
            VersionId = versionId;
            Type = type;
            Kind = kind;
            Resolution = resolution;
            GameDirectory = Path.Combine(IOHelper.InstancesDir, name);
            JavaPath = string.Empty;
            JVMArgs = "-XX:+UnlockExperimentalVMOptions -XX:+UseG1GC -XX:G1NewSizePercent=20 -XX:G1ReservePercent=20 -XX:MaxGCPauseMillis=50 -XX:G1HeapRegionSize=16M -Djava.net.preferIPv4Stack=true";
            Memory = memory;
            LauncherVisibility = launcherVisibility;
        }
    }
}
