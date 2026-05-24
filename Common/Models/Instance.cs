
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Enums;

namespace Tavstal.KonkordLauncher.Common.Models;

[Serializable]
public class Instance
{
    [JsonProperty("id")]
    public string Id { get; set; }
    
    [JsonProperty("name")]
    public string Name { get; set; }
    
    [JsonProperty("group")]
    public string? Group { get; set; }
    
    [JsonProperty("iconPath")]
    public string IconPath { get; set; }
    
    [JsonProperty("minecraftVersion")]
    public string MinecraftVersion { get; set; }
    
    [JsonProperty("customVersion")]
    public string CustomVersion { get; set; }
    
    [JsonProperty("type")]
    public EProfileType Type { get; set; }
    
    [JsonProperty("kind")]
    public EMinecraftKind Kind { get; set; }
    
    [JsonProperty("gameDirectory")]
    public string? GameDirectory { get; set; }
    
    [JsonProperty("settings")]
    public InstanceConfig.InstanceConfig Config { get; set; }

    public Instance()
    {
        Id = Guid.NewGuid().ToString();
    }

    public string GetResourceConfigPath() => Path.Combine(GameDirectory!, "resources.json");

    public static string GetDefaultJVMArgs()
    {
        return "-XX:+UnlockExperimentalVMOptions -XX:+UseG1GC -XX:G1NewSizePercent=20 -XX:G1ReservePercent=20 -XX:MaxGCPauseMillis=50 -XX:G1HeapRegionSize=16M -Djava.net.preferIPv4Stack=true";
    }
}