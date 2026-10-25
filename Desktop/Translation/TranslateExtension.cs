using System;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace Tavstal.KonkordLauncher.Desktop.Translation;

public class TranslateExtension : MarkupExtension
{
    public TranslateExtension(string key)
    {
        Key = key;
    }

    public string Key { get; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new Binding { Source = TranslationBindingSource.Instance, Path = $"[{Key}]", Mode = BindingMode.OneWay };
    }
}
