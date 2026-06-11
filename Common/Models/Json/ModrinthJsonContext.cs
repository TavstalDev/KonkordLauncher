using System.Text.Json.Serialization;
using Modrinth.Models;
using Tavstal.KonkordLauncher.Common.Models.Package.Modrinth;
using Version = Modrinth.Models.Version;

namespace Tavstal.KonkordLauncher.Common.Models.Json;

/// <summary>
/// Provides a compile-time, source-generated JSON serialization context for the Modrinth API models.
/// </summary>
[JsonSerializable(typeof(SearchResponse))]
[JsonSerializable(typeof(Project))]
[JsonSerializable(typeof(Project[]))]
[JsonSerializable(typeof(Version))]
[JsonSerializable(typeof(Version[]))]
[JsonSerializable(typeof(List<string[]>))]
[JsonSerializable(typeof(PackageFile))]
[JsonSerializable(typeof(ModrinthPackageIndex))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower, WriteIndented = true, IgnoreReadOnlyFields = true, IgnoreReadOnlyProperties = true)]
public partial class ModrinthJsonContext : JsonSerializerContext;