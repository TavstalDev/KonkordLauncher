using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Common.Models.Config;

public class MinecraftConfig
{
    [JsonProperty("startMaximized"), JsonPropertyName("startMaximized")]
    public bool StartMaximized { get; set; }
    
    [JsonProperty("windowWidth"), JsonPropertyName("windowWidth")]
    public uint WindowWidth { get; set; }
    
    [JsonProperty("windowHeight"), JsonPropertyName("windowHeight")]
    public uint WindowHeight { get; set; }
    
    [JsonProperty("closeLauncherOnGameStart"), JsonPropertyName("closeLauncherOnGameStart")]
    public bool CloseLauncherOnGameStart { get; set; }
    
    [JsonProperty("closeLauncherOnGameExit"), JsonPropertyName("closeLauncherOnGameExit")]
    public bool CloseLauncherOnGameExit { get; set; }

    public MinecraftConfig()
    {
        StartMaximized = false;
        WindowWidth = 1280;
        WindowHeight = 720;
        CloseLauncherOnGameStart = false;
        CloseLauncherOnGameExit = false;
    }

    public MinecraftConfig(bool startMaximized, uint windowWidth, uint windowHeight, bool closeLauncherOnGameStart, bool closeLauncherOnGameExit)
    {
        StartMaximized = startMaximized;
        WindowWidth = windowWidth;
        WindowHeight = windowHeight;
        CloseLauncherOnGameStart = closeLauncherOnGameStart;
        CloseLauncherOnGameExit = closeLauncherOnGameExit;
    }
}