using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Tavstal.BeaconSDK.Enums;
using Tavstal.BeaconSDK.Helpers.Serialization;
using Tavstal.KonkordLauncher.Desktop.Serialization;

namespace Tavstal.KonkordLauncher.Desktop.Models.DTOs;

/// <summary>
///     Holds the data of a launcher instance.
/// </summary>
public class Instance
{
    /// <summary>
    ///     Gets or sets the unique ID of the instance. A new GUID is created by default.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    ///     Gets or sets the display name of the instance.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    ///     Gets or sets the optional group name of the instance.
    /// </summary>
    [JsonPropertyName("group")]
    public string? Group { get; set; }

    /// <summary>
    ///     Gets or sets the optional icon path of the instance.
    /// </summary>
    [JsonPropertyName("iconPath")]
    public string? IconPath { get; set; }

    /// <summary>
    ///     Gets or sets the Minecraft version of the instance.
    /// </summary>
    [JsonPropertyName("minecraftVersion")]
    public required string MinecraftVersion { get; set; }

    /// <summary>
    ///     Gets or sets the optional custom version label.
    /// </summary>
    [JsonPropertyName("customVersion")]
    public string? CustomVersion { get; set; }

    /// <summary>
    ///     Gets or sets the Minecraft edition of the instance.
    /// </summary>
    [JsonPropertyName("kind")]
    public EMinecraftKind Kind { get; set; }

    /// <summary>
    ///     Gets or sets the game folder of the instance.
    /// </summary>
    [JsonPropertyName("gameDirectory")]
    public string? GameDirectory { get; set; }

    /// <summary>
    ///     Gets or sets the instance settings.
    /// </summary>
    [JsonPropertyName("settings")]
    public required InstanceConfig.InstanceConfig Config { get; set; }

    /// <summary>
    ///     Gets the full path to the resource file of the instance.
    /// </summary>
    /// <returns>The path to <c>resources.json</c> in the game folder.</returns>
    public string GetResourceConfigPath()
    {
        return Path.Combine(GameDirectory!, "resources.json");
    }

    /// <summary>
    ///     Reads the resources of the instance from disk.
    /// </summary>
    /// <returns>The resources, or <see langword="null" /> if the file does not exist.</returns>
    public async Task<List<InstanceResource>?> GetInstanceResourcesAsync()
    {
        var path = GetResourceConfigPath();
        if (!File.Exists(path))
            return null;
        return await JsonHelper.ReadJsonFileAsync<List<InstanceResource>>(path,
            CommonJsonContext.Default.ListInstanceResource);
    }

    /// <summary>
    ///     Gets the default JVM arguments with recommended garbage collector settings.
    /// </summary>
    /// <returns>The default JVM arguments as one string.</returns>
    public static string GetDefaultJVMArgs()
    {
        return
            "-XX:+UnlockExperimentalVMOptions -XX:+UseG1GC -XX:G1NewSizePercent=20 -XX:G1ReservePercent=20 -XX:MaxGCPauseMillis=50 -XX:G1HeapRegionSize=16M -Djava.net.preferIPv4Stack=true";
    }
}
