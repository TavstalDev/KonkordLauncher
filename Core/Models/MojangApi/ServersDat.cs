
using NbtLib;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi;

/// <summary>
/// Represents the data structure for storing a list of Minecraft servers.
/// </summary>
public class ServersDat
{
    /// <summary>
    /// Gets or sets the list of Minecraft servers.
    /// </summary>
    [NbtProperty(PropertyName="servers")]
    [JsonProperty("servers")]
    public List<MinecraftServer> Servers { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServersDat"/> class
    /// with an empty list of servers.
    /// </summary>
    public ServersDat()
    {
        Servers = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServersDat"/> class
    /// with the specified list of servers.
    /// </summary>
    /// <param name="servers">The list of Minecraft servers.</param>
    public ServersDat(List<MinecraftServer> servers)
    {
        Servers = servers;
    }
}