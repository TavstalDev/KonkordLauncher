using System;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Desktop.Models.Domain;

/// <summary>
/// Lightweight view-model / domain model for a Minecraft cape used by the desktop UI.
/// Encapsulates the cape identifier, optional alias, a loaded <see cref="Bitmap"/> for display,
/// and a selection flag. Implements <see cref="IDisposable"/> to release the loaded bitmap resource.
/// </summary>
public class CapeDataModel : IDisposable
{
    /// <summary>
    /// Gets or sets the unique identifier for the cape (corresponds to the Mojang/Cape id).
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Gets or sets the optional alias for the cape (friendly name or shorthand).
    /// </summary>
    public string Alias { get; set; }
    
    /// <summary>
    /// Gets or sets the loaded bitmap for the cape texture.
    /// May be <c>null</c> if loading failed or has not been performed.
    /// </summary>
    public BitmapEntry Image { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether this cape is currently selected in the UI.
    /// </summary>
    public bool IsSelected { get; set; }
    
    /// <summary>
    /// Parameterless constructor required for some serializers and UI frameworks.
    /// Creates an empty model; callers should populate properties as needed.
    /// </summary>
    public CapeDataModel() { }
    
    /// <summary>
    /// Initializes a new instance of <see cref="CapeDataModel"/> with the specified values.
    /// </summary>
    /// <param name="id">The cape identifier.</param>
    /// <param name="alias">The cape alias (may be empty).</param>
    /// <param name="image">The loaded <see cref="BitmapEntry"/> image for display (may be <c>null</c>).</param>
    /// <param name="isSelected">Whether the cape should be initially selected.</param>
    public CapeDataModel(string id, string alias, BitmapEntry image, bool isSelected)
    {
        Id = id;
        Alias = alias;
        Image = image;
        IsSelected = isSelected;
    }
    
    /// <summary>
    /// Disposes managed resources held by this model.
    /// Specifically disposes the <see cref="Image"/> bitmap if present.
    /// Calling <see cref="Dispose"/> multiple times is safe.
    /// </summary>
    public void Dispose()
    {
        if (Image.IsEmpty())
            return;
        
        var services = Program.ServiceProvider;
        var bitmapService = services.GetRequiredService<IBitmapService>();
        bitmapService.Release(Image.Key!);
    }
}