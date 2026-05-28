using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.KonkordLauncher.Desktop.Models.Instance;

public partial class CategoryModel : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;
    
    [ObservableProperty]
    public partial string TranslationKey { get; set; } = string.Empty;
    
    [ObservableProperty]
    public partial bool IsChecked { get; set; }
    
    public string DisplayName => string.IsNullOrEmpty(TranslationKey) ? Name : TranslationManager.Translate(TranslationKey);
}