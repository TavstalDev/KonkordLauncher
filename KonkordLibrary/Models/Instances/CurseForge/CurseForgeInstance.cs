using KonkordLibrary.Enums;
using KonkordLibrary.Helpers;
using KonkordLibrary.Models.Launcher;
using Newtonsoft.Json;
using System.IO;
using System.Text.Json.Serialization;

namespace KonkordLibrary.Models.Instances.CurseForge
{
    public class CurseForgeInstance
    {
        [JsonProperty("minecraft"), JsonPropertyName("minecraft")]
        public CurseMinecraft Minecraft { get; set; }
        [JsonProperty("manifestType"), JsonPropertyName("manifestType")]
        public string ManifestType { get; set; }
        [JsonProperty("manifestVersion"), JsonPropertyName("manifestVersion")]
        public int ManifestVersion { get; set; }
        [JsonProperty("name"), JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonProperty("version"), JsonPropertyName("version")]
        public string Version { get; set; }
        [JsonProperty("author"), JsonPropertyName("author")]
        public string Author { get; set; }
        [JsonProperty("files"), JsonPropertyName("files")]
        public List<CurseFile> Files { get; set; }
        [JsonProperty("overrides"), JsonPropertyName("overrides")]
        public string Overrides { get; set; }

        public CurseForgeInstance(CurseMinecraft minecraft, string manifestType, int manifestVersion, string name, string version, string author, List<CurseFile> files, string overrides)
        {
            Minecraft = minecraft;
            ManifestType = manifestType;
            ManifestVersion = manifestVersion;
            Name = name;
            Version = version;
            Author = author;
            Files = files;
            Overrides = overrides;
        }

        public Profile GetProfile()
        {
            string gameDir = Path.Combine(IOHelper.InstancesDir, Name);

            EProfileKind kind = EProfileKind.VANILLA;
            string versionId = Minecraft.Version;
            if (Minecraft.ModLoaders != null)
            {
                CurseModLoader? curseModLoader = Minecraft.ModLoaders.FirstOrDefault(x => x.IsPrimary);
                if (curseModLoader != null)
                {
                    if (curseModLoader.Id.Contains("neoforge"))
                    {
                        kind = EProfileKind.FORGE;
                        versionId = curseModLoader.Id.Replace("neoforge", "").Replace("-", "");
                    }
                    else if (curseModLoader.Id.Contains("fabric"))
                    {
                        kind = EProfileKind.FABRIC;
                        versionId = curseModLoader.Id.Replace("fabric", "").Replace("-", "");
                    }
                    else if (curseModLoader.Id.Contains("quilt"))
                    {
                        kind = EProfileKind.QUILT;
                        versionId = curseModLoader.Id.Replace("quilt", "").Replace("-", "");
                    }
                    else if (curseModLoader.Id.Contains("forge"))
                    {
                        kind = EProfileKind.FORGE;
                        versionId = curseModLoader.Id.Replace("forge", "").Replace("-", "");
                    }
                }
            }

            return new Profile()
            {
                Name = Name,
                Icon = ProfileIcon.Icons.ElementAt(MathHelper.Next(0, ProfileIcon.Icons.Count - 1)).Path,
                GameDirectory = gameDir,
                JavaPath = string.Empty,
                JVMArgs = Profile.GetDefaultJVMArgs(),
                Kind = kind,
                Memory = -1,
                Resolution = null,
                Type = Enums.EProfileType.CUSTOM,
                VersionId = versionId,
                VersionVanillaId = Minecraft.Version,
                LauncherVisibility = Enums.ELaucnherVisibility.HIDE_AND_REOPEN_ON_GAME_CLOSE
            };
        }
    }
}
