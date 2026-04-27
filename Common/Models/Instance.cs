using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Enums;

namespace Tavstal.KonkordLauncher.Common.Models;

[Serializable]
public class Instance
{
    [JsonPropertyName("id"), JsonProperty("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("name"), JsonProperty("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("group"), JsonProperty("group")]
    public string? Group { get; set; }
    
    [JsonPropertyName("iconPath"), JsonProperty("iconPath")]
    public string IconPath { get; set; }
    
    [JsonPropertyName("minecraftVersion"), JsonProperty("minecraftVersion")]
    public string MinecraftVersion { get; set; }
    
    [JsonPropertyName("customVersion"), JsonProperty("customVersion")]
    public string CustomVersion { get; set; }
    
    [JsonPropertyName("type"), JsonProperty("type")]
    public EProfileType Type { get; set; }
    
    [JsonPropertyName("kind"), JsonProperty("kind")]
    public EMinecraftKind Kind { get; set; }
    
    [JsonProperty("gameDirectory"), JsonPropertyName("gameDirectory")]
    public string? GameDirectory { get; set; }
    
    [JsonProperty("settings"), JsonPropertyName("settings")]
    public InstanceConfig.InstanceConfig Config { get; set; }

    public Instance()
    {
        Id = Guid.NewGuid().ToString();
    }

    public static string GetDefaultJVMArgs()
    {
        return "-XX:+UnlockExperimentalVMOptions -XX:+UseG1GC -XX:G1NewSizePercent=20 -XX:G1ReservePercent=20 -XX:MaxGCPauseMillis=50 -XX:G1HeapRegionSize=16M -Djava.net.preferIPv4Stack=true";
    }
}