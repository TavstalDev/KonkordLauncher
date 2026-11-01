using System.Text.Json.Serialization;
using NbtLib;

namespace Tavstal.KonkordLauncher.Desktop.Models.DTOs;

/// <summary>
///     Holds the data of a Minecraft server entry.
/// </summary>
public class MinecraftServer
{
    /// <summary>
    ///     Creates a new instance with default values.
    /// </summary>
    public MinecraftServer()
    {
        Name = string.Empty;
        Ip = string.Empty;
        AcceptTextures = 1;
    }

    /// <summary>
    ///     Creates a new instance with the given values.
    /// </summary>
    /// <param name="name">The name of the server.</param>
    /// <param name="ip">The IP address of the server.</param>
    /// <param name="acceptTextures">The value that shows if the server accepts textures.</param>
    /// <param name="hideAddress">The value that shows if the server hides its address.</param>
    /// <param name="icon">The optional icon of the server.</param>
    public MinecraftServer(string name, string ip, byte acceptTextures, byte? hideAddress, string? icon)
    {
        Name = name;
        Ip = ip;
        AcceptTextures = acceptTextures;
        HideAddress = hideAddress;
        Icon = icon;
    }

    /// <summary>
    ///     Gets or sets the name of the server.
    /// </summary>
    [NbtProperty(PropertyName = "name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    ///     Gets or sets the IP address of the server.
    /// </summary>
    [NbtProperty(PropertyName = "ip")]
    [JsonPropertyName("ip")]
    public string Ip { get; set; }

    /// <summary>
    ///     Gets or sets the value that shows if the server accepts textures.
    /// </summary>
    [NbtProperty(PropertyName = "acceptTextures")]
    [JsonPropertyName("acceptTextures")]
    public byte AcceptTextures { get; set; }

    /// <summary>
    ///     Gets or sets the value that shows if the server hides its address.
    /// </summary>
    [NbtProperty(PropertyName = "hideAddress")]
    [JsonPropertyName("hideAddress")]
    public byte? HideAddress { get; set; }

    /// <summary>
    ///     Gets or sets the optional icon of the server.
    /// </summary>
    [NbtProperty(PropertyName = "icon")]
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }
}
