using System;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Helpers.IO;

namespace Tavstal.KonkordLauncher.Desktop.Models.Instance;

/// <summary>
/// Represents a resource pack model used in the application.
/// This model contains information about the resource pack, such as its name, size, provider, and icon.
/// </summary>
public partial class ResourcePackModel : ObservableObject
{
    public Guid Id { get; } = Guid.NewGuid();
    
    /// <summary>
    /// Indicates whether the resource pack is enabled.
    /// </summary>
    [ObservableProperty] private bool _isEnabled;

    /// <summary>
    /// The name of the resource pack.
    /// </summary>
    [ObservableProperty] private string _name;

    /// <summary>
    /// The file path of the resource pack.
    /// </summary>
    [ObservableProperty] private string _path;
    
    /// <summary>
    /// The icon associated with the resource pack.
    /// </summary>
    [ObservableProperty] private Bitmap? _icon;

    /// <summary>
    /// The provider of the resource pack, if available.
    /// </summary>
    [ObservableProperty] private string? _provider;

    /// <summary>
    /// The size of the resource pack in bytes.
    /// Updates the <see cref="FormattedSize"/> property when changed.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedSize))]
    private long _size;

    /// <summary>
    /// Gets the formatted size of the resource pack as a human-readable string.
    /// </summary>
    public string FormattedSize => FileSystemHelper.GetFormatedSize(Size);

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourcePackModel"/> class with the specified properties.
    /// </summary>
    /// <param name="isEnabled">Indicates whether the resource pack is enabled.</param>
    /// <param name="name">The name of the resource pack.</param>
    /// <param name="path">The file path of the resource pack.</param>
    /// <param name="icon">The icon associated with the resource pack.</param>
    /// <param name="provider">The provider of the resource pack, if available.</param>
    /// <param name="size">The size of the resource pack in bytes.</param>
    public ResourcePackModel(bool isEnabled, string name, string path, Bitmap? icon, string? provider, long size)
    {
        _isEnabled = isEnabled;
        _name = name;
        _path = path;
        _icon = icon;
        _provider = provider;
        _size = size;
    }
}