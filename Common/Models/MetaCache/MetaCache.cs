
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Common.Models.MetaCache;

public class MetaCache
{
    [JsonProperty("id")]
    public required string Id { get; set; }
    
    [JsonProperty("type")]
    public required EMetaCacheType Type { get; set; }
    
    [JsonProperty("path")]
    public required string Path { get; set; }
    
    [JsonProperty("valid_until")]
    public required DateTime ValidUntil { get; set; }
    
    [JsonProperty("last_modified_at")]
    public DateTime? LastModifiedAt { get; set; }
    
    public bool IsValid() => DateTime.UtcNow < ValidUntil && File.Exists(Path);
}