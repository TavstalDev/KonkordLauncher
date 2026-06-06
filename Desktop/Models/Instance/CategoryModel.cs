using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Desktop.Models.Instance;

/// <summary>
/// Represents a selectable category with a display name that supports translation.
/// </summary>
public partial class CategoryModel : ObservableObject
{
    /// <summary>
    /// Gets or sets the category name.
    /// </summary>
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the translation key for the category name. Notifies <see cref="DisplayName"/> on change.
    /// </summary>
    [ObservableProperty, NotifyPropertyChangedFor(nameof(DisplayName))]
    public partial string TranslationKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets whether this category is checked/selected.
    /// </summary>
    [ObservableProperty]
    public partial bool IsChecked { get; set; }
    
    /// <summary>
    /// Gets the localized display name, falling back to <see cref="Name"/> if no translation key is set.
    /// </summary>
    public string DisplayName => string.IsNullOrEmpty(TranslationKey) ? Name : Program.ServiceProvider.GetRequiredService<ITranslationService>().Translate(TranslationKey);
}