using System.Text.Json.Serialization;
using Tavstal.KonkordLauncher.Core.Models.Microsoft;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Fabric;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge.Legacy;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge.Modern;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.NeoForge;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta.Library;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Requests;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.User;
using ForgeVersionMetaModern = Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge.Modern.ForgeVersionMeta;
using ForgeVersionMetaLegacy = Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge.Legacy.ForgeVersionMeta;

namespace Tavstal.KonkordLauncher.Core.Models.Json;

/// <summary>
/// Represents a custom JSON serializer context for serializing various game-related objects.
/// </summary>
[JsonSerializable(typeof(VersionManifest))]
[JsonSerializable(typeof(FabricVersionMeta))]
[JsonSerializable(typeof(ForgeProfile))]
[JsonSerializable(typeof(ForgeVersionMetaLegacy), TypeInfoPropertyName = "ForgeVersionMetaLegacy")]
[JsonSerializable(typeof(ForgeVersionMetaModern), TypeInfoPropertyName = "ForgeVersionMetaModern")]
[JsonSerializable(typeof(ForgeVersionProfile))]
[JsonSerializable(typeof(List<ForgeManifest>))]
[JsonSerializable(typeof(List<NeoForgeManifest>))]
[JsonSerializable(typeof(List<Rule>))]
[JsonSerializable(typeof(DeviceCodeResult))]
[JsonSerializable(typeof(XboxTokenRequestBody))]
[JsonSerializable(typeof(XboxXstsRequestBody))]
[JsonSerializable(typeof(MinecraftAccessRequestBody))]
[JsonSerializable(typeof(OwnershipData))]
[JsonSerializable(typeof(MojangProfile))]
[JsonSerializable(typeof(VersionMeta))]
[JsonSerializable(typeof(ShowCapeRequestBody))]
[JsonSerializable(typeof(ChangeSkinRequestBody))]
[JsonSourceGenerationOptions(WriteIndented = true, IgnoreReadOnlyProperties = true, IgnoreReadOnlyFields = true )]
public partial class CoreJsonContext : JsonSerializerContext
{
    
}