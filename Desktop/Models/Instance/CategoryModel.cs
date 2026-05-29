using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Desktop.Models.Instance;

public partial class CategoryModel : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;
    
    [ObservableProperty, NotifyPropertyChangedFor(nameof(DisplayName))]
    public partial string TranslationKey { get; set; } = string.Empty;
    
    [ObservableProperty]
    public partial bool IsChecked { get; set; }
    
    public string DisplayName => string.IsNullOrEmpty(TranslationKey) ? Name : Program.ServiceProvider.GetRequiredService<ITranslationService>().Translate(TranslationKey);
}