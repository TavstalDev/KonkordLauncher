using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Common.Models.MetaCache;

public class MetaCache
{
    [JsonProperty("id"), JsonPropertyName("id")]
    public required string Id { get; set; }
    
    [JsonProperty("type"), JsonPropertyName("type")]
    public required EMetaCacheType Type { get; set; }
    
    [JsonProperty("path"), JsonPropertyName("path")]
    public required string Path { get; set; }
    
    [JsonProperty("valid_until"), JsonPropertyName("valid_until")]
    public required DateTime ValidUntil { get; set; }
    
    [JsonProperty("last_modified_at"), JsonPropertyName("last_modified_at")]
    public DateTime? LastModifiedAt { get; set; }
    
    public bool IsValid() => DateTime.UtcNow < ValidUntil && File.Exists(Path);
}