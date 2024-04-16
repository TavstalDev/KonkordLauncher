using Tavstal.KonkordLibrary.Enums;
using Tavstal.KonkordLibrary.Helpers;
using Newtonsoft.Json;
using System.IO;
using System.Text.Json.Serialization;

namespace Tavstal.KonkordLibrary.Models.Launcher
{
    [Serializable]
    public class Profile
    {
        [JsonPropertyName("name"), JsonProperty("name")]
        public string Name { get; set; }
        [JsonPropertyName("icon"), JsonProperty("icon")]
        public string Icon { get; set; }
        [JsonPropertyName("versionId"), JsonProperty("versionId")]
        public string VersionId { get; set; }
        [JsonPropertyName("versionVanillaId"), JsonProperty("versionVanillaId")]
        public string VersionVanillaId { get; set; }
        [JsonPropertyName("type"), JsonProperty("type")]
        public EProfileType Type { get; set; }
        [JsonPropertyName("kind"), JsonProperty("kind")]
        public EProfileKind Kind { get; set; }
        [JsonPropertyName("resolution"), JsonProperty("resolution")]
        public Resolution? Resolution { get; set; }
        [JsonPropertyName("gameDirectory"), JsonProperty("gameDirectory")]
        public string GameDirectory { get; set; }
        [JsonPropertyName("javaPath"), JsonProperty("javaPath")]
        public string JavaPath { get; set; }
        [JsonPropertyName("jvmArgs"), JsonProperty("jvmArgs")]
        public string JVMArgs { get; set; }
        [JsonPropertyName("memory"), JsonProperty("memory")]
        public int Memory { get; set; }
        [JsonPropertyName("launcherVisibility"), JsonProperty("launcherVisibility")]
        public ELaucnherVisibility LauncherVisibility { get; set; }

        public Profile() { }

        public Profile(string name, string icon, string versionId, string versionVanillaId, EProfileType type, EProfileKind kind, Resolution resolution, string gameDirectory, string javaPath, string jVMArgs, int memory, ELaucnherVisibility launcherVisibility)
        {
            Name = name;
            Icon = icon;
            VersionId = versionId;
            VersionVanillaId = versionVanillaId;
            Type = type;
            Kind = kind;
            Resolution = resolution;
            GameDirectory = gameDirectory;
            JavaPath = javaPath;
            JVMArgs = jVMArgs;
            Memory = memory;
            LauncherVisibility = launcherVisibility;
        }

        public Profile(string name, string icon, string versionId, string versionVanillaId, EProfileType type, EProfileKind kind, Resolution? resolution, int memory, ELaucnherVisibility launcherVisibility)
        {
            Name = name;
            Icon = icon;
            VersionId = versionId;
            VersionVanillaId = versionVanillaId;
            Type = type;
            Kind = kind;
            Resolution = resolution;
            GameDirectory = Path.Combine(IOHelper.InstancesDir, name);
            JavaPath = string.Empty;
            JVMArgs = GetDefaultJVMArgs();
            Memory = memory;
            LauncherVisibility = launcherVisibility;
        }

        public string GetGameDirectory()
        {
            VersionDetails version = GameHelper.GetProfileVersionDetails(Kind, VersionId, VersionVanillaId, GameDirectory);
            return version.GameDir;
        }

        public static string GetDefaultJVMArgs()
        {
            return "-XX:+UnlockExperimentalVMOptions -XX:+UseG1GC -XX:G1NewSizePercent=20 -XX:G1ReservePercent=20 -XX:MaxGCPauseMillis=50 -XX:G1HeapRegionSize=16M -Djava.net.preferIPv4Stack=true";
        }
    }
}
