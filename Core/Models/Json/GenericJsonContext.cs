using System.Text.Json.Serialization;

namespace Tavstal.KonkordLauncher.Core.Models.Json;

[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSourceGenerationOptions(WriteIndented = true, IgnoreReadOnlyProperties = true, IgnoreReadOnlyFields = true )]
public partial class GenericJsonContext : JsonSerializerContext
{
    
}