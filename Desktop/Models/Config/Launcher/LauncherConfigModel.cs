using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;

namespace Tavstal.KonkordLauncher.Desktop.Models.Config.Launcher;

/// <summary>
///     Holds the launcher settings, such as updates, language, theme, and folder paths.
/// </summary>
public partial class LauncherConfigModel : ObservableObject
{
    /// <summary>
    ///     Creates a new instance with default settings.
    /// </summary>
    public LauncherConfigModel() { }

    /// <summary>
    ///     Creates a new instance with the given settings.
    /// </summary>
    /// <param name="enableAutomaticUpdates">Whether automatic updates are enabled.</param>
    /// <param name="updateInterval">The update interval in minutes.</param>
    /// <param name="language">The language code.</param>
    /// <param name="theme">The theme.</param>
    /// <param name="assetsDirectoryPath">The path to the assets folder.</param>
    /// <param name="cacheDirectoryPath">The path to the cache folder.</param>
    /// <param name="iconsDirectoryPath">The path to the icons folder.</param>
    /// <param name="instancesDirectoryPath">The path to the instances folder.</param>
    /// <param name="javaDirectoryPath">The path to the Java folder.</param>
    /// <param name="librariesDirectoryPath">The path to the libraries folder.</param>
    /// <param name="manifestsDirectoryPath">The path to the manifests folder.</param>
    /// <param name="translationsDirectoryPath">The path to the translations folder.</param>
    /// <param name="versionsDirectoryPath">The path to the versions folder.</param>
    public LauncherConfigModel(bool enableAutomaticUpdates, uint updateInterval, string language, EThemeType theme,
        string assetsDirectoryPath, string cacheDirectoryPath, string iconsDirectoryPath, string instancesDirectoryPath,
        string javaDirectoryPath, string librariesDirectoryPath, string manifestsDirectoryPath,
        string translationsDirectoryPath, string versionsDirectoryPath)
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

    /// <summary>
    ///     Gets or sets a value that indicates whether automatic updates are enabled.
    /// </summary>
    [ObservableProperty]
    public partial bool EnableAutomaticUpdates { get; set; }

    /// <summary>
    ///     Gets or sets the update interval in minutes.
    /// </summary>
    [ObservableProperty]
    public partial uint UpdateInterval { get; set; }

    /// <summary>
    ///     Gets or sets the language code.
    /// </summary>
    [ObservableProperty]
    public partial string Language { get; set; }

    /// <summary>
    ///     Gets or sets the theme.
    /// </summary>
    [ObservableProperty]
    public partial EThemeType Theme { get; set; }

    /// <summary>
    ///     Gets or sets the path to the assets folder.
    /// </summary>
    [ObservableProperty]
    public partial string AssetsDirectoryPath { get; set; }

    /// <summary>
    ///     Gets or sets the path to the cache folder.
    /// </summary>
    [ObservableProperty]
    public partial string CacheDirectoryPath { get; set; }

    /// <summary>
    ///     Gets or sets the path to the icons folder.
    /// </summary>
    [ObservableProperty]
    public partial string IconsDirectoryPath { get; set; }

    /// <summary>
    ///     Gets or sets the path to the instances folder.
    /// </summary>
    [ObservableProperty]
    public partial string InstancesDirectoryPath { get; set; }

    /// <summary>
    ///     Gets or sets the path to the Java folder.
    /// </summary>
    [ObservableProperty]
    public partial string JavaDirectoryPath { get; set; }

    /// <summary>
    ///     Gets or sets the path to the libraries folder.
    /// </summary>
    [ObservableProperty]
    public partial string LibrariesDirectoryPath { get; set; }

    /// <summary>
    ///     Gets or sets the path to the manifests folder.
    /// </summary>
    [ObservableProperty]
    public partial string ManifestsDirectoryPath { get; set; }

    /// <summary>
    ///     Gets or sets the path to the translations folder.
    /// </summary>
    [ObservableProperty]
    public partial string TranslationsDirectoryPath { get; set; }

    /// <summary>
    ///     Gets or sets the path to the versions folder.
    /// </summary>
    [ObservableProperty]
    public partial string VersionsDirectoryPath { get; set; }
}
