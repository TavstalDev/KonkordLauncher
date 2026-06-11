using System.Text.Json.Serialization;
using Tavstal.KonkordLauncher.Common.Models.Package.CurseForge;

namespace Tavstal.KonkordLauncher.Common.Models.Json;

/// <summary>
/// Provides a compile-time, source-generated JSON serialization context for the CurseForge package manifest model.
/// </summary>
[JsonSerializable(typeof(CurseForgeManifest))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower, WriteIndented = true, IgnoreReadOnlyFields = true, IgnoreReadOnlyProperties = true)]
public partial class CurseForgeJsonContext : JsonSerializerContext;