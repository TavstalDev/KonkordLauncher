using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Common.Models;

namespace Tavstal.KonkordLauncher.Desktop.Models.Config.Launcher;

/// <summary>
/// Represents the configuration model for the launcher, including update settings, language, theme, and directory paths.
/// </summary>
public partial class LauncherConfigModel : ObservableObject
{
    /// <summary>
    /// Gets or sets a value indicating whether automatic updates are enabled.
    /// </summary>
    [ObservableProperty]
    public partial bool EnableAutomaticUpdates { get; set; }

    /// <summary>
    /// Gets or sets the update interval in minutes.
    /// </summary>
    [ObservableProperty]
    public partial uint UpdateInterval { get; set; }

    /// <summary>
    /// Gets or sets the language of the launcher.
    /// </summary>
    [ObservableProperty]
    public partial string Language { get; set; }

    /// <summary>
    /// Gets or sets the theme type of the launcher.
    /// </summary>
    [ObservableProperty]
    public partial EThemeType Theme { get; set; }

    /// <summary>
    /// Gets or sets the path to the assets directory.
    /// </summary>
    [ObservableProperty]
    public partial string AssetsDirectoryPath { get; set; }

    /// <summary>
    /// Gets or sets the path to the cache directory.
    /// </summary>
    [ObservableProperty]
    public partial string CacheDirectoryPath { get; set; }

    /// <summary>
    /// Gets or sets the path to the icons directory.
    /// </summary>
    [ObservableProperty]
    public partial string IconsDirectoryPath { get; set; }

    /// <summary>
    /// Gets or sets the path to the instances directory.
    /// </summary>
    [ObservableProperty]
    public partial string InstancesDirectoryPath { get; set; }

    /// <summary>
    /// Gets or sets the path to the Java directory.
    /// </summary>
    [ObservableProperty]
    public partial string JavaDirectoryPath { get; set; }

    /// <summary>
    /// Gets or sets the path to the libraries directory.
    /// </summary>
    [ObservableProperty]
    public partial string LibrariesDirectoryPath { get; set; }

    /// <summary>
    /// Gets or sets the path to the manifests directory.
    /// </summary>
    [ObservableProperty]
    public partial string ManifestsDirectoryPath { get; set; }

    /// <summary>
    /// Gets or sets the path to the translations directory.
    /// </summary>
    [ObservableProperty]
    public partial string TranslationsDirectoryPath { get; set; }

    /// <summary>
    /// Gets or sets the path to the versions directory.
    /// </summary>
    [ObservableProperty]
    public partial string VersionsDirectoryPath { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LauncherConfigModel"/> class with default values.
    /// </summary>
    public LauncherConfigModel() {}

    /// <summary>
    /// Initializes a new instance of the <see cref="LauncherConfigModel"/> class with specified values.
    /// </summary>
    /// <param name="enableAutomaticUpdates">Whether automatic updates are enabled.</param>
    /// <param name="updateInterval">The update interval in minutes.</param>
    /// <param name="language">The language of the launcher.</param>
    /// <param name="theme">The theme type of the launcher.</param>
    /// <param name="assetsDirectoryPath">The path to the assets directory.</param>
    /// <param name="cacheDirectoryPath">The path to the cache directory.</param>
    /// <param name="iconsDirectoryPath">The path to the icons directory.</param>
    /// <param name="instancesDirectoryPath">The path to the instances directory.</param>
    /// <param name="javaDirectoryPath">The path to the Java directory.</param>
    /// <param name="librariesDirectoryPath">The path to the libraries directory.</param>
    /// <param name="manifestsDirectoryPath">The path to the manifests directory.</param>
    /// <param name="translationsDirectoryPath">The path to the translations directory.</param>
    /// <param name="versionsDirectoryPath">The path to the versions directory.</param>
    public LauncherConfigModel(bool enableAutomaticUpdates, uint updateInterval, string language, EThemeType theme, string assetsDirectoryPath, string cacheDirectoryPath, string iconsDirectoryPath, string instancesDirectoryPath, string javaDirectoryPath, string librariesDirectoryPath, string manifestsDirectoryPath, string translationsDirectoryPath, string versionsDirectoryPath)
    {
        EnableAutomaticUpdates = enableAutomaticUpdates;
        UpdateInterval = updateInterval;
        Language = language;
        Theme = theme;
        AssetsDirectoryPath = assetsDirectoryPath;
        CacheDirectoryPath = cacheDirectoryPath;
        IconsDirectoryPath = iconsDirectoryPath;
        InstancesDirectoryPath = instancesDirectoryPath;
        JavaDirectoryPath = javaDirectoryPath;
        LibrariesDirectoryPath = librariesDirectoryPath;
        ManifestsDirectoryPath = manifestsDirectoryPath;
        TranslationsDirectoryPath = translationsDirectoryPath;
        VersionsDirectoryPath = versionsDirectoryPath;
    }
}