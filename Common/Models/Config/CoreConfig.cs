using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Common.Models.Config;

public class CoreConfig
{
    [JsonProperty("launcher"), JsonPropertyName("launcher")]
    public LauncherConfig Launcher { get; set; }
    
    [JsonProperty("java"), JsonPropertyName("java")]
    public JavaConfig Java { get; set; }
    
    [JsonProperty("minecraft"), JsonPropertyName("minecraft")]
    public MinecraftConfig Minecraft { get; set; }
    
    [JsonProperty("misc"), JsonPropertyName("misc")]
    public MiscConfig Misc { get; set; }
    
    public CoreConfig()
    {
        Launcher = new LauncherConfig();
        Java = new JavaConfig();
        Minecraft = new MinecraftConfig();
        Misc = new MiscConfig();
    }
    
    public CoreConfig(LauncherConfig launcher, JavaConfig java, MinecraftConfig minecraft, MiscConfig misc)
    {
        Launcher = launcher;
        Java = java;
        Minecraft = minecraft;
        Misc = misc;
    }
}