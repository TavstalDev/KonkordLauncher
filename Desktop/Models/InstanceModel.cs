using System;
using System.Diagnostics;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Common.Models.InstanceConfig;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Desktop.Helpers;

namespace Tavstal.KonkordLauncher.Desktop.Models;

/// <summary>
/// Represents a model for a Minecraft instance, including its properties and behaviors.
/// </summary>
public partial class InstanceModel : ObservableObject
{
    /// <summary>
    /// Gets or sets the unique identifier of the instance.
    /// </summary>
    [ObservableProperty] private string _id;

    /// <summary>
    /// Gets or sets the name of the instance.
    /// </summary>
    [ObservableProperty] private string _name;

    /// <summary>
    /// Gets or sets the group to which the instance belongs.
    /// </summary>
    [ObservableProperty] private string? _group;

    /// <summary>
    /// Gets or sets the file path to the icon of the instance.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Icon))]
    private string _iconPath;

    /// <summary>
    /// Gets or sets the Minecraft version associated with the instance.
    /// </summary>
    [ObservableProperty] private string _minecraftVersion;

    /// <summary>
    /// Gets or sets the custom version of the instance, if any.
    /// </summary>
    [ObservableProperty] private string _customVersion;

    /// <summary>
    /// Gets or sets the profile type of the instance.
    /// </summary>
    [ObservableProperty] private EProfileType _type;

    /// <summary>
    /// Gets or sets the kind of Minecraft associated with the instance.
    /// </summary>
    [ObservableProperty] private EMinecraftKind _kind;

    /// <summary>
    /// Gets or sets the custom game directory for the instance, if specified.
    /// </summary>
    [ObservableProperty] private string? _gameDirectory;

    /// <summary>
    /// Gets or sets the configuration of the instance.
    /// </summary>
    [ObservableProperty] private InstanceConfig _configModel;

    /// <summary>
    /// Gets or sets the process associated with the running game.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGameRunning))]
    private Process? _gameProcess;

    /// <summary>
    /// Gets or sets a value indicating whether the game is currently running.
    /// </summary>
    [ObservableProperty] private bool _isGameRunning;

    /// <summary>
    /// Gets the icon of the instance as a bitmap. If the icon path is not set, a default icon is used.
    /// </summary>
    public Bitmap? Icon => string.IsNullOrEmpty(IconPath)
        ? ImageHelper.LoadFromResource(new Uri("avares://Desktop/Assets/Icons/dirt.png"))
        : new Bitmap(IconPath);

    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceModel"/> class.
    /// </summary>
    public InstanceModel() {}

    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceModel"/> class using the specified common instance model.
    /// </summary>
    /// <param name="instance">The common instance model to initialize from.</param>
    public InstanceModel(Common.Models.Instance instance)
    {
        this.Id = instance.Id;
        this.Name = instance.Name;
        this.Group = instance.Group;
        this.IconPath = instance.IconPath;
        this.MinecraftVersion = instance.MinecraftVersion;
        this.CustomVersion = instance.CustomVersion;
        this.Type = instance.Type;
        this.Kind = instance.Kind;
        this.GameDirectory = instance.GameDirectory;
        this.ConfigModel = instance.Config;
    }

    /// <summary>
    /// Attaches event handlers to the game process to handle its exit and disposal events.
    /// </summary>
    public void AttachProcessEvent()
    {
        if (GameProcess == null)
            return;

        GameProcess.Exited += (sender, args) =>
        {
            IsGameRunning = false;
            GameProcess = null;
        };

        GameProcess.Disposed += (sender, args) =>
        {
            IsGameRunning = false;
            GameProcess = null;
        };
    }
}