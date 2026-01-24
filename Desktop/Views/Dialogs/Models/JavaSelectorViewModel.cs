using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Helpers;
using JavaVersionModel = Tavstal.KonkordLauncher.Desktop.Models.JavaVersionModel;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

/// <summary>
/// ViewModel for selecting a Java version. Provides properties and methods to manage and display Java versions.
/// </summary>
public partial class JavaSelectorViewModel : ObservableObject
{
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasSelectedJavaVersion))] private JavaVersionModel? _selectedJavaVersion;
    [ObservableProperty] private ObservableCollection<JavaVersionModel> _versions;
    
    /// <summary>
    /// Indicates whether a Java version is currently selected.
    /// </summary>
    public bool HasSelectedJavaVersion => SelectedJavaVersion != null;
    
    public Interaction<Unit, Unit> MinimizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> MaximizeWindowInteraction { get; } = new();
    public Interaction<JavaVersionModel?, Unit> CloseWindowInteraction { get; } = new();
    
    public JavaSelectorViewModel(string? customJavaDirectory = null)
    {
        Versions = [];
        SelectedJavaVersion = null;
        
        // Load available Java versions
        var versions = JavaHelper.LocateJavaInstallations(customJavaDirectory);
        foreach (var version in versions)
            Versions.Add(new JavaVersionModel(version));
    }
    
    /// <summary>
    /// Handles the selection action by closing the parent window and passing the selected Java version.
    /// </summary>
    [RelayCommand]
    public async Task SelectedBtn() => await CloseWindowInteraction.Handle(SelectedJavaVersion);

    /// <summary>
    /// Handles the cancel action by closing the parent window without passing a selected Java version.
    /// </summary>
    [RelayCommand]
    public async Task CancelBtn() => await CloseWindowInteraction.Handle(null);
    
    #region Window Commands
    [RelayCommand]
    public async Task MinimizeWindow()
    {
        await MinimizeWindowInteraction.Handle(Unit.Default);
    }

    [RelayCommand]
    public async Task MaximizeWindow()
    {
        await MaximizeWindowInteraction.Handle(Unit.Default);
    }

    [RelayCommand]
    public async Task CloseWindow()
    {
        await CloseWindowInteraction.Handle(null);
    }
    #endregion
}