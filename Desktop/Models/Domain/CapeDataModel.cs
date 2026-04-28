using System;
using Avalonia.Media.Imaging;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.User;
using Tavstal.KonkordLauncher.Desktop.Helpers;

namespace Tavstal.KonkordLauncher.Desktop.Models.Domain;

public class CapeDataModel : IDisposable
{
    public string Id {  get; set; }
    
    public string Alias { get; set; }
    
    public Bitmap? Image {  get; set; }
    
    public bool IsSelected {  get; set; }
    
    public CapeDataModel() { }
    
    public CapeDataModel(string id, string alias, Bitmap? image, bool isSelected)
    {
        Id = id;
        Alias = alias;
        Image = image;
        IsSelected = isSelected;
    }
    
    public CapeDataModel(Cape cape, bool isSelected = false) : this(cape.Id, cape.Alias, ImageHelper.Load(cape.Url).Result, isSelected) { }

    public void Dispose()
    {
        Image?.Dispose();
    }
}