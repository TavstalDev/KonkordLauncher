using System;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace Tavstal.KonkordLauncher.Desktop.Models.Translation;

/// <summary>
/// A markup extension that provides a binding to a translated string based on a specified key.
/// </summary>
public class TranslateExtension : MarkupExtension
{
    /// <summary>
    /// Gets the translation key used to retrieve the translated string.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslateExtension"/> class with the specified translation key.
    /// </summary>
    /// <param name="key">The key to use for retrieving the translated string.</param>
    public TranslateExtension(string key) => Key = key;

    /// <summary>
    /// Provides the value for the markup extension, which is a binding to the translated string.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can be used to provide services for the markup extension.</param>
    /// <returns>A one-way binding to the translated string corresponding to the specified key.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new Binding
        {
            Source = TranslationBindingSource.Instance,
            Path = $"[{Key}]",
            Mode = BindingMode.OneWay
        };
    }
}