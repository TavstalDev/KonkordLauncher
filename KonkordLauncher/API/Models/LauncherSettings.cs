﻿using KonkordLauncher.API.Helpers;
using System.IO;
using System.Text.Json.Serialization;

namespace KonkordLauncher.API.Models
{
    [Serializable]
    public class LauncherSettings
    {
        [JsonPropertyName("version")]
        public int Version { get; set; }
        [JsonPropertyName("language")]
        public string Language { get; set; }
        [JsonPropertyName("selectedProfile")]
        public string SelectedProfile { get; set; }
        [JsonPropertyName("profile")]
        public Dictionary<string, Profile> Profiles { get; set; }

        public LauncherSettings()
        {
            Version = 1;
            Language = "en";
            string latestRelease = Guid.NewGuid().ToString();
            SelectedProfile = latestRelease;
            Profiles = new Dictionary<string, Profile>()
            {
                { latestRelease, new Profile()
                    {
                        Name = "Latest Release",
                        Icon = ProfileIcon.Icons.Find(x => x.Name == "Grass").Path,
                        Type = Enums.EProfileType.LATEST_RELEASE,
                        Kind = Enums.EProfileKind.VANILLA,
                        VersionId = string.Empty,
                        GameDirectory = Path.Combine(IOHelper.InstancesDir, "LatestRelease"),
                        JavaPath = string.Empty,
                        LauncherVisibility = Enums.ELaucnherVisibility.HIDE_AND_REOPEN_ON_GAME_CLOSE,
                        Memory = -1,
                        Resolution = null,
                        JVMArgs = "-XX:+UnlockExperimentalVMOptions -XX:+UseG1GC -XX:G1NewSizePercent=20 -XX:G1ReservePercent=20 -XX:MaxGCPauseMillis=50 -XX:G1HeapRegionSize=16M -Djava.net.preferIPv4Stack=true"
                    } 
                }
            };
        }

        public LauncherSettings(int version, string language, Dictionary<string, Profile> profiles)
        {
            Version = version;
            Language = language;
            Profiles = profiles;
        }
    }
}