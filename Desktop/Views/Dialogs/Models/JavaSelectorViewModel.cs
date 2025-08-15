using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Desktop.Models;
using JavaVersionModel = Tavstal.KonkordLauncher.Desktop.Models.JavaVersionModel;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

/// <summary>
/// ViewModel for selecting a Java version. Provides properties and methods to manage and display Java versions.
/// </summary>
public partial class JavaSelectorViewModel : KonkordObservableObject
{
    private JavaSelectorWindow? _parentWindow;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasSelectedJavaVersion))] private JavaVersionModel? _selectedJavaVersion;
    [ObservableProperty] private ObservableCollection<JavaVersionModel> _versions;
    
    /// <summary>
    /// Indicates whether a Java version is currently selected.
    /// </summary>
    public bool HasSelectedJavaVersion => SelectedJavaVersion != null;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="JavaSelectorViewModel"/> class.
    /// Sets up the parent window, initializes the Java versions collection, and loads available Java installations.
    /// </summary>
    /// <param name="parentWindow">The parent window associated with this view model.</param>
    /// <param name="customJavaDirectory">
    /// An optional custom directory to search for Java installations. If null, the default directories are used.
    /// </param>
    public JavaSelectorViewModel(JavaSelectorWindow parentWindow, string? customJavaDirectory = null)
    {
        _parentWindow = parentWindow;
        Versions = [];
        SelectedJavaVersion = null;
        
        // Load available Java versions
        var versions = JavaHelper.LocateJavaInstallations(customJavaDirectory);
        foreach (var version in versions)
            Versions.Add(new JavaVersionModel(version));
    }

    /// <summary>
    /// Releases resources associated with the <see cref="JavaSelectorViewModel"/>.
    /// Clears the collection of Java versions and resets the selected Java version to null.
    /// </summary>
    public override void FreeMemory()
    {
        Versions.Clear();
        Versions = [];
        SelectedJavaVersion = null;
        _parentWindow = null;
    }
    
    /// <summary>
    /// Handles the selection action by closing the parent window and passing the selected Java version.
    /// </summary>
    [RelayCommand]
    public void SelectedBtn() => _parentWindow?.Close(SelectedJavaVersion);

    /// <summary>
    /// Handles the cancel action by closing the parent window without passing a selected Java version.
    /// </summary>
    [RelayCommand]
    public void CancelBtn() => _parentWindow?.Close(null);
}