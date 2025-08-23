using System.ComponentModel;
using Tavstal.KonkordLauncher.Common.Translation;

namespace Tavstal.KonkordLauncher.Common.Models.Translation;

/// <summary>
/// Provides a binding source for translations, enabling dynamic updates when the language changes.
/// Implements <see cref="INotifyPropertyChanged"/> to notify UI elements of changes.
/// </summary>
public class TranslationBindingSource : INotifyPropertyChanged
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="TranslationBindingSource"/> class.
    /// </summary>
    public static TranslationBindingSource Instance { get; } = new();

    /// <summary>
    /// Gets the translated string for the specified key.
    /// </summary>
    /// <param name="key">The key to translate.</param>
    /// <returns>The translated string corresponding to the key.</returns>
    public string this[string key] => TranslationManager.Translate(key);

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event to notify that the language has changed.
    /// Passes <c>null</c> as the property name to indicate that all bindings should refresh.
    /// </summary>
    public void RaiseLanguageChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }
}