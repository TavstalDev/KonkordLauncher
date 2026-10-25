using System.Collections.Generic;
using System.Text.Json.Serialization;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs.Config;

namespace Tavstal.KonkordLauncher.Desktop.Serialization;

/// <summary>
/// Source generation context for JSON serialization/deserialization of common models.
/// </summary>
[JsonSerializable(typeof(CoreConfig))]
[JsonSerializable(typeof(AccountData))]
[JsonSerializable(typeof(Instance))]
[JsonSerializable(typeof(List<Instance>))]
[JsonSerializable(typeof(InstanceResource))]
[JsonSerializable(typeof(List<InstanceResource>))]
[JsonSerializable(typeof(PatchNote))]
[JsonSerializable(typeof(List<PatchNote>))]
[JsonSerializable(typeof(Models.DTOs.MetaCache.MetaCache))]
[JsonSerializable(typeof(List<Models.DTOs.MetaCache.MetaCache>))]
[JsonSerializable(typeof(Models.DTOs.MetaCache.MetaCache[]))]
[JsonSourceGenerationOptions(WriteIndented = true, IgnoreReadOnlyProperties = true, IgnoreReadOnlyFields = true)]
public partial class CommonJsonContext : JsonSerializerContext;