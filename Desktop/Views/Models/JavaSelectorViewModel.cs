using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Common.Helpers;
using JavaVersionModel = Tavstal.KonkordLauncher.Desktop.Models.JavaVersionModel;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

/// <summary>
/// ViewModel for selecting a Java version. Provides properties and methods to manage and display Java versions.
/// </summary>
public partial class JavaSelectorViewModel : ObservableObject
{
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasSelectedJavaVersion))] private JavaVersionModel? _selectedJavaVersion;
    [ObservableProperty] private ObservableCollection<JavaVersionModel> _versions;
    [ObservableProperty] private string _tableMajorText = "Major";
    [ObservableProperty] private string _tableVersionText = "Version";
    [ObservableProperty] private string _tableArchitectureText = "Architecture";
    [ObservableProperty] private string _tablePathText = "Path";
    
    /// <summary>
    /// Indicates whether a Java version is currently selected.
    /// </summary>
    public bool HasSelectedJavaVersion => SelectedJavaVersion != null;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="JavaSelectorViewModel"/> class.
    /// </summary>
    /// <param name="customJavaDirectory">
    /// An optional custom directory to search for Java installations. If null, the default directories are used.
    /// </param>
    public JavaSelectorViewModel(string? customJavaDirectory = null)
    {
        Versions = [];
        SelectedJavaVersion = null;
        
        // Load available Java versions
        var versions = JavaHelper.LocateJavaInstallations(customJavaDirectory);
        foreach (var version in versions)
            Versions.Add(new JavaVersionModel(version));
    }
}