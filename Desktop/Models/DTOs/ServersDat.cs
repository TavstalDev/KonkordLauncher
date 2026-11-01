using System.Collections.Generic;
using System.Text.Json.Serialization;
using NbtLib;

namespace Tavstal.KonkordLauncher.Desktop.Models.DTOs;

/// <summary>
///     Holds the list of saved Minecraft servers.
/// </summary>
public class ServersDat
{
    /// <summary>
    ///     Creates a new instance with an empty server list.
    /// </summary>
    public ServersDat()
    {
        Servers = [];
    }

    /// <summary>
    ///     Creates a new instance with the given server list.
    /// </summary>
    /// <param name="servers">The list of servers.</param>
    public ServersDat(List<MinecraftServer> servers)
    {
        Servers = servers;
    }

    /// <summary>
    ///     Gets or sets the list of servers.
    /// </summary>
    [NbtProperty(PropertyName = "servers")]
    [JsonPropertyName("servers")]
    public List<MinecraftServer> Servers { get; set; }
}
