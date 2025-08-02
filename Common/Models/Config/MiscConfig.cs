using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Common.Models.Config;

public class MiscConfig
{
    [JsonProperty("preLaunchCommand"), JsonPropertyName("preLaunchCommand")]
    public string PreLaunchCommand { get; set; }
    
    [JsonProperty("wrapperCommand"), JsonPropertyName("wrapperCommand")]
    public string WrapperCommand { get; set; }
    
    [JsonProperty("postExitCommand"), JsonPropertyName("postExitCommand")]
    public string PostExitCommand { get; set; }
    
    [JsonProperty("useCustomGlfw"), JsonPropertyName("useCustomGlfw")]
    public bool UseCustomGlfw { get; set; }
    
    [JsonProperty("customGlfwPath"), JsonPropertyName("customGlfwPath")]
    public string CustomGlfwPath { get; set; }
    
    [JsonProperty("useCustomOpenAl"), JsonPropertyName("useCustomOpenAl")]
    public bool UseCustomOpenAl { get; set; }
    
    [JsonProperty("customOpenAlPath"), JsonPropertyName("customOpenAlPath")]
    public string CustomOpenAlPath { get; set; }
    
    [JsonProperty("enableFeralGameMode"), JsonPropertyName("enableFeralGameMode")]
    public bool EnableFeralGameMode { get; set; }
    
    [JsonProperty("enableMangoHud"), JsonPropertyName("enableMangoHud")]
    public bool EnableMangoHud { get; set; }
    
    [JsonProperty("useDedicatedGpu"), JsonPropertyName("useDedicatedGpu")]
    public bool UseDedicatedGpu { get; set; }

    public MiscConfig()
    {
        PreLaunchCommand = string.Empty;
        WrapperCommand = string.Empty;
        PostExitCommand = string.Empty;
        UseCustomGlfw = false;
        CustomGlfwPath = string.Empty;
        UseCustomOpenAl = false;
        CustomOpenAlPath = string.Empty;
        EnableFeralGameMode = false;
        EnableMangoHud = false;
        UseDedicatedGpu = false;
    }
    
    public MiscConfig(string preLaunchCommand, string wrapperCommand, string postExitCommand, bool useCustomGlfw, string customGlfwPath, bool useCustomOpenAl, string customOpenAlPath, bool enableFeralGameMode, bool enableMangoHud, bool useDedicatedGpu)
    {
        PreLaunchCommand = preLaunchCommand;
        WrapperCommand = wrapperCommand;
        PostExitCommand = postExitCommand;
        UseCustomGlfw = useCustomGlfw;
        CustomGlfwPath = customGlfwPath;
        UseCustomOpenAl = useCustomOpenAl;
        CustomOpenAlPath = customOpenAlPath;
        EnableFeralGameMode = enableFeralGameMode;
        EnableMangoHud = enableMangoHud;
        UseDedicatedGpu = useDedicatedGpu;
    }
}