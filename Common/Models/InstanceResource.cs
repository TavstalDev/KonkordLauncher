using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Enums;

namespace Tavstal.KonkordLauncher.Common.Models;

public class InstanceResource
{
    [JsonProperty("projectId")]
    public required string ProjectId { get; set; }
    
    [JsonProperty("instanceId")]
    public required string Name { get; set; }
    
    [JsonProperty("url")]
    public required string Url { get; set; }
    
    [JsonProperty("iconPath")]
    public string? IconPath { get; set; }
    
    [JsonProperty("path")]
    public required string Path { get; set; }
    
    [JsonProperty("client")]
    public string? Client { get; set; }
    
    [JsonProperty("server")]
    public string? Server { get; set; }
    
    [JsonProperty("sha1")]
    public string? Sha1 { get; set; }
    
    [JsonProperty("sha256")]
    public string? Sha512 { get; set; }
    
    [JsonProperty("platform")]
    public EPlatformType Platform { get; set; }
    
    [JsonProperty("type")]
    public EResourceType Type { get; set; }
    
    [JsonProperty("fileSize")]
    public long FileSize { get; set; }
}