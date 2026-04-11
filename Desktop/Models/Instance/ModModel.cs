using System;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Helpers.IO;

namespace Tavstal.KonkordLauncher.Desktop.Models.Instance;

/// <summary>
/// Represents a mod model with properties such as ID, name, path, version, size, and provider.
/// </summary>
public partial class ModModel : ObservableObject
{
    /// <summary>
    /// Gets the unique identifier for the mod.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Indicates whether the mod is enabled.
    /// </summary>
    [ObservableProperty] private bool _isEnabled;

    /// <summary>
    /// The name of the mod.
    /// </summary>
    [ObservableProperty] private string _name;

    /// <summary>
    /// The file path to the mod.
    /// </summary>
    [ObservableProperty] private string _path;

    /// <summary>
    /// The icon associated with the mod.
    /// </summary>
    [ObservableProperty] private Bitmap? _icon;

    /// <summary>
    /// The provider of the mod.
    /// </summary>
    [ObservableProperty] private string? _provider;

    /// <summary>
    /// The version of the mod.
    /// </summary>
    [ObservableProperty] private string _version;

    /// <summary>
    /// The size of the mod in bytes.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedSize))]
    private long _size;

    /// <summary>
    /// Gets the formatted size of the mod as a human-readable string.
    /// </summary>
    public string FormattedSize => FileSystemHelper.GetFormatedSize(Size);

    /// <summary>
    /// Initializes a new instance of the <see cref="ModModel"/> class with the specified properties.
    /// </summary>
    /// <param name="isEnabled">Indicates whether the mod is enabled.</param>
    /// <param name="name">The name of the mod.</param>
    /// <param name="path">The file path to the mod.</param>
    /// <param name="icon">The icon associated with the mod.</param>
    /// <param name="provider">The provider of the mod.</param>
    /// <param name="version">The version of the mod.</param>
    /// <param name="size">The size of the mod in bytes.</param>
    public ModModel(bool isEnabled, string name, string path, Bitmap? icon, string? provider, string version, long size)
    {
        _isEnabled = isEnabled;
        _name = name;
        _path = path;
        _icon = icon;
        _provider = provider;
        _version = version;
        _size = size;
    }
}