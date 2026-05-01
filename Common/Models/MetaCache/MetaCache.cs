using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Common.Models.MetaCache;

public class MetaCache
{
    [JsonProperty("id"), JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonProperty("name"), JsonPropertyName("name")]
    public EMetaCacheType Type { get; set; }
    
    [JsonProperty("etag"), JsonPropertyName("etag")]
    public string? ETag { get; set; }
    
    [JsonProperty("path"), JsonPropertyName("path")]
    public string Path { get; set; }
    
    [JsonProperty("valid_until"), JsonPropertyName("valid_until")]
    public DateTime ValidUntil { get; set; }
    
    [JsonProperty("version"), JsonPropertyName("version")]
    public DateTime LastModifiedAt { get; set; }
    
    public bool IsValid() => DateTime.Now < ValidUntil;
}