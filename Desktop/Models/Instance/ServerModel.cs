using System;
using Avalonia.Media.Imaging;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Desktop.Helpers;

namespace Tavstal.KonkordLauncher.Desktop.Models.Instance;

/// <summary>
/// Represents a server model that extends the <see cref="MinecraftServer"/> class.
/// Provides additional functionality to convert the server's icon (if available)
/// from a Base64 string to a bitmap image.
/// </summary>
public class ServerModel : MinecraftServer
{
    /// <summary>
    /// Gets the bitmap representation of the server's icon.
    /// If the icon is null, this property returns null.
    /// </summary>
    public Bitmap? Image => Icon == null ? ImageHelper.LoadFromResource(new Uri("avares://Desktop/Assets/Images/default_world.png")) : ImageHelper.Base64ToBitmap(Icon);

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerModel"/> class
    /// with the specified properties.
    /// </summary>
    /// <param name="name">The name of the Minecraft server.</param>
    /// <param name="ip">The IP address of the Minecraft server.</param>
    /// <param name="acceptTextures">Indicates whether the server accepts textures.</param>
    /// <param name="hideAddress">Indicates whether the server address is hidden.</param>
    /// <param name="icon">The optional icon of the Minecraft server.</param>
    public ServerModel(string name, string ip, byte acceptTextures, byte? hideAddress, string? icon) : base(name, ip, acceptTextures, hideAddress, icon)
    {
    }
}