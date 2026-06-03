using Avalonia.Media.Imaging;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Common.Models;

public class BitmapEntry
{
    public string? Key { get; private set; }
    
    public Bitmap? Value { get; private set; }

    public BitmapEntry(string? key, Bitmap? value)
    {
        Key = key;
        Value = value;
    }
    
    public void SetValue(string? key, Bitmap? value)
    {
        Key = key;
        Value = value;
    }
    
    public bool IsEmpty()
    {
        return Value == null || string.IsNullOrEmpty(Key);
    }

    public void Dispose(IBitmapService service)
    {
        if (Key != null)
            service.Release(Key);
        Key = null;
        Value = null;
    }
}