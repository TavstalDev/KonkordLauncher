using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Helpers.IO;

namespace Tavstal.KonkordLauncher.Desktop.Models.Instance;

/// <summary>
/// Represents a shader pack model with properties such as ID, name, path, size, and provider.
/// </summary>
public partial class ShaderPackModel : ObservableObject
{
    /// <summary>
    /// Gets the unique identifier for the shader pack.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Indicates whether the shader pack is enabled.
    /// </summary>
    [ObservableProperty] private bool _isEnabled;

    /// <summary>
    /// The name of the shader pack.
    /// </summary>
    [ObservableProperty] private string _name;

    /// <summary>
    /// The file path to the shader pack.
    /// </summary>
    [ObservableProperty] private string _path;

    /// <summary>
    /// The provider of the shader pack.
    /// </summary>
    [ObservableProperty] private string? _provider;

    /// <summary>
    /// The size of the shader pack in bytes.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedSize))]
    private long _size;

    /// <summary>
    /// Gets the formatted size of the shader pack as a human-readable string.
    /// </summary>
    public string FormattedSize => FileSystemHelper.GetFormatedSize(Size);

    /// <summary>
    /// Initializes a new instance of the <see cref="ShaderPackModel"/> class with the specified properties.
    /// </summary>
    /// <param name="isEnabled">Indicates whether the shader pack is enabled.</param>
    /// <param name="name">The name of the shader pack.</param>
    /// <param name="path">The file path to the shader pack.</param>
    /// <param name="provider">The provider of the shader pack.</param>
    /// <param name="size">The size of the shader pack in bytes.</param>
    public ShaderPackModel(bool isEnabled, string name, string path, string? provider, long size)
    {
        _isEnabled = isEnabled;
        _name = name;
        _path = path;
        _provider = provider;
        _size = size;
    }
}