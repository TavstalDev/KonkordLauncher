using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;
using JavaVersionModel = Tavstal.KonkordLauncher.Desktop.Models.Domain.JavaVersionModel;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

/// <summary>
///     ViewModel for selecting a Java version. Provides properties and methods to manage and display Java versions.
/// </summary>
public partial class JavaSelectorViewModel : ObservableObject
{
    private readonly IJavaService _javaService = null!;
    private readonly ILauncherStore _launcherStore = null!;

    /// <summary>
    ///     Initializes a new instance of the <see cref="JavaSelectorViewModel" /> class.
    /// </summary>
    public JavaSelectorViewModel()
    {
        Versions = [];
        SelectedJavaVersion = null;
    }

    public JavaSelectorViewModel(ILauncherStore launcherStore, IJavaService javaService)
    {
        Versions = [];
        SelectedJavaVersion = null;

        if (Design.IsDesignMode)
            return;

        _launcherStore = launcherStore;
        _javaService = javaService;

        _ = InitAsync();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedJavaVersion))]
    public partial JavaVersionModel? SelectedJavaVersion { get; set; }

    [ObservableProperty] public partial ObservableCollection<JavaVersionModel> Versions { get; set; }

    /// <summary>
    ///     Indicates whether a Java version is currently selected.
    /// </summary>
    public bool HasSelectedJavaVersion => SelectedJavaVersion != null;

    /// <summary>
    ///     Represents the interaction for closing a window with a model of Java version.
    /// </summary>
    public Interaction<JavaVersionModel?, Unit> CloseWindowInteraction { get; } = new();

    /// <summary>
    ///     Loads the current launcher configuration, discovers installed Java runtimes in the configured
    ///     Java directory, and populates <see cref="Versions" /> with the detected installations.
    /// </summary>
    private async Task InitAsync()
    {
        var settings = await _launcherStore.GetSettingsAsync();

        var versions = await _javaService.LocateJavaInstallationsAsync(settings.Launcher.JavaDirectoryPath);
        foreach (var version in versions)
            Versions.Add(new JavaVersionModel(version));
    }

    /// <summary>
    ///     Handles the selection action by closing the parent window and passing the selected Java version.
    /// </summary>
    [RelayCommand]
    private async Task SelectedBtn()
    {
        await CloseWindowInteraction.Handle(SelectedJavaVersion);
    }

    /// <summary>
    ///     Handles the cancel action by closing the parent window without passing a selected Java version.
    /// </summary>
    [RelayCommand]
    private async Task CancelBtn()
    {
        await CloseWindowInteraction.Handle(null);
    }

    /// <summary>
    ///     Closes the current window.
    /// </summary>
    [RelayCommand]
    private async Task CloseWindow()
    {
        await CloseWindowInteraction.Handle(null);
    }
}
