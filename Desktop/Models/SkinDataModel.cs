using System;
using Avalonia.Media.Imaging;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.User;
using Tavstal.KonkordLauncher.Desktop.Helpers;

namespace Tavstal.KonkordLauncher.Desktop.Models;

public class SkinDataModel : IDisposable
{
    public string Id { get; set; }
    
    public string Variant { get; set; }
    
    public Bitmap? Image { get; set; }
    
    public bool IsSelected { get; set; }
    
    public SkinDataModel() { }
    
    public SkinDataModel(string id, string variant, Bitmap? image, bool isSelected)
    {
        Id = id;
        Variant = variant;
        Image = image;
        IsSelected = isSelected;
    }
    
    public SkinDataModel(Skin skin, bool isSelected = false) : this(skin.Id, skin.Variant, ImageHelper.Load(skin.Url).Result, isSelected) { }

    public void Dispose()
    {
        Image?.Dispose();
    }
}