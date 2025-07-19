using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Common.Models;

[Serializable]
public class LauncherSettings
{
    [JsonPropertyName("version"), JsonProperty("version")]
    public int Version { get; set; }
    [JsonPropertyName("language"), JsonProperty("language")]
    public string Language { get; set; }
    [JsonPropertyName("selectedProfile"), JsonProperty("selectedProfile")]
    public string SelectedProfile { get; set; }
    [JsonPropertyName("profile"), JsonProperty("profile")]
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
                    Icon = ProfileIcon.Icons.Find(x => x.Name == "Grass")?.Path,
                    Type = Core.Enums.EProfileType.LATEST_RELEASE,
                    Kind = Core.Enums.EMinecraftKind.VANILLA,
                    VersionId = string.Empty,
                    VersionVanillaId = string.Empty,
                    GameDirectory = string.Empty,
                    JavaPath = string.Empty,
                    LauncherVisibility = Core.Enums.ELaucnherVisibility.HIDE_AND_REOPEN_ON_GAME_CLOSE,
                    Memory = -1,
                    Resolution = null,
                    JVMArgs = Profile.GetDefaultJVMArgs()
                }
            },
            { Guid.NewGuid().ToString(), new Profile()
                {
                    Name = "Latest Snapshot",
                    Icon = ProfileIcon.Icons.Find(x => x.Name == "Dirt")?.Path,
                    Type = Core.Enums.EProfileType.LATEST_SNAPSHOT,
                    Kind = Core.Enums.EMinecraftKind.VANILLA,
                    VersionId = string.Empty,
                    VersionVanillaId = string.Empty,
                    GameDirectory = string.Empty,
                    JavaPath = string.Empty,
                    LauncherVisibility = Core.Enums.ELaucnherVisibility.HIDE_AND_REOPEN_ON_GAME_CLOSE,
                    Memory= -1,
                    Resolution = null,
                    JVMArgs = Profile.GetDefaultJVMArgs()
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