using System.Text.Json.Serialization;
using NbtLib;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi;

/// <summary>
/// Represents a Minecraft server with properties for its name, IP address,
/// texture acceptance, address visibility, and optional icon.
/// </summary>
public class MinecraftServer
{
    /// <summary>
    /// Gets or sets the name of the Minecraft server.
    /// </summary>
    [NbtProperty(PropertyName="name")]
    [JsonProperty("name"), JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the IP address of the Minecraft server.
    /// </summary>
    [NbtProperty(PropertyName="ip")]
    [JsonProperty("ip"), JsonPropertyName("ip")]
    public string Ip { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the server accepts textures.
    /// </summary>
    [NbtProperty(PropertyName="acceptTextures")]
    [JsonProperty("acceptTextures"), JsonPropertyName("acceptTextures")]
    public byte AcceptTextures { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the server address is hidden.
    /// This property is nullable.
    /// </summary>
    [NbtProperty(PropertyName="hideAddress")]
    [JsonProperty("hideAddress"), JsonPropertyName("hideAddress")]
    public byte? HideAddress { get; set; }

    /// <summary>
    /// Gets or sets the optional icon of the Minecraft server.
    /// </summary>
    [NbtProperty(PropertyName="icon")]
    [JsonProperty("icon"), JsonPropertyName("icon")]
    public string? Icon { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinecraftServer"/> class
    /// with default values.
    /// </summary>
    public MinecraftServer()
    {
        Name = string.Empty;
        Ip = string.Empty;
        AcceptTextures = 1;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinecraftServer"/> class
    /// with the specified properties.
    /// </summary>
    /// <param name="name">The name of the Minecraft server.</param>
    /// <param name="ip">The IP address of the Minecraft server.</param>
    /// <param name="acceptTextures">Indicates whether the server accepts textures.</param>
    /// <param name="hideAddress">Indicates whether the server address is hidden.</param>
    /// <param name="icon">The optional icon of the Minecraft server.</param>
    public MinecraftServer(string name, string ip, byte acceptTextures, byte? hideAddress, string? icon)
    {
        Name = name;
        Ip = ip;
        AcceptTextures = acceptTextures;
        HideAddress = hideAddress;
        Icon = icon;
    }
}