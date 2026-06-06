
using System.Text.Json.Serialization;

using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;

namespace Tavstal.KonkordLauncher.Core.Models.ModLoaders.Fabric;

/// <summary>
/// Metadata model for a Fabric-specific version descriptor. This mirrors the JSON structure
/// returned by Fabric version metadata files and is used when combining Fabric's metadata
/// with the underlying Mojang version metadata to construct a runnable version.
/// </summary>
public class FabricVersionMeta
{
    /// <summary>
    /// Gets or sets the collection of argument metadata used when launching the game.
    /// This typically includes JVM and game argument definitions parsed from the underlying
    /// Mojang version metadata and any Fabric-specific additions.
    /// </summary>
    [JsonPropertyName("arguments")]
    public ArgumentMeta Arguments { get; set; }

    /// <summary>
    /// Gets or sets the version identifier for this Fabric version meta (e.g. "1.18.2+build.1").
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the ID of the Mojang version this Fabric meta inherits from.
    /// When present, the runner should merge or fall back to the inherited Mojang version's metadata.
    /// </summary>
    [JsonPropertyName("inheritsFrom")]
    public string InheritsFrom { get; set; }

    /// <summary>
    /// Gets or sets the list of libraries required by this Fabric version. These are Fabric-specific
    /// or additional libraries that must be present on the classpath alongside the vanilla libraries.
    /// </summary>
    [JsonPropertyName("libraries")]
    public List<FabricLibrary> Libraries { get; set; }

    /// <summary>
    /// Gets or sets the main class to use when launching the game for this Fabric version.
    /// Fabric often requires a specific main class to boot its loader; use this value when
    /// constructing the launch arguments if present.
    /// </summary>
    [JsonPropertyName("mainClass")]
    public string MainClass { get; set; }

    /// <summary>
    /// Gets or sets the type of this version metadata (commonly "release", "snapshot", or fabric-specific types).
    /// Consumers may use this to differentiate between stable and experimental metadata.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; }
}