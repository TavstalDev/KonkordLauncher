using System;
using Avalonia.Media.Imaging;
using Tavstal.KonkordLauncher.Desktop.Helpers;

namespace Tavstal.KonkordLauncher.Desktop.Models;

/// <summary>
/// Represents a model for a Minecraft instance in the desktop application,
/// extending the common instance model with additional desktop-specific functionality.
/// </summary>
public class InstanceModel : Common.Models.Instance
{
    /// <summary>
    /// Gets the icon for the instance.
    /// If <see cref="IconPath"/> is null or empty, loads a default icon from resources.
    /// Otherwise, loads the icon from the specified path.
    /// </summary>
    public Bitmap? Icon => string.IsNullOrEmpty(IconPath) ? ImageHelper.LoadFromResource(new Uri("avares://Desktop/Assets/Icons/dirt.png")) : new Bitmap(IconPath);

    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceModel"/> class.
    /// </summary>
    public InstanceModel() {}

    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceModel"/> class
    /// by copying properties from an existing <see cref="Common.Models.Instance"/>.
    /// </summary>
    /// <param name="instance">The instance to copy properties from.</param>
    public InstanceModel(Common.Models.Instance instance)
    {
        this.Name = instance.Name;
        this.Group = instance.Group;
        this.IconPath = instance.IconPath;
        this.MinecraftVersion = instance.MinecraftVersion;
        this.CustomVersion = instance.CustomVersion;
        this.Type = instance.Type;
        this.Kind = instance.Kind;
        this.GameDirectory = instance.GameDirectory;
        this.Config = instance.Config;
    }
}