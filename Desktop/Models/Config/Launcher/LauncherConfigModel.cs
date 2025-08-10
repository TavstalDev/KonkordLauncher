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
    [ObservableProperty] private bool _enableAutomaticUpdates;

    /// <summary>
    /// Gets or sets the update interval in minutes.
    /// </summary>
    [ObservableProperty] private uint _updateInterval;

    /// <summary>
    /// Gets or sets the language of the launcher.
    /// </summary>
    [ObservableProperty] private string _language;

    /// <summary>
    /// Gets or sets the theme type of the launcher.
    /// </summary>
    [ObservableProperty] private EThemeType _theme;

    /// <summary>
    /// Gets or sets the path to the assets directory.
    /// </summary>
    [ObservableProperty] private string _assetsDirectoryPath;

    /// <summary>
    /// Gets or sets the path to the cache directory.
    /// </summary>
    [ObservableProperty] private string _cacheDirectoryPath;

    /// <summary>
    /// Gets or sets the path to the icons directory.
    /// </summary>
    [ObservableProperty] private string _iconsDirectoryPath;

    /// <summary>
    /// Gets or sets the path to the instances directory.
    /// </summary>
    [ObservableProperty] private string _instancesDirectoryPath;

    /// <summary>
    /// Gets or sets the path to the Java directory.
    /// </summary>
    [ObservableProperty] private string _javaDirectoryPath;

    /// <summary>
    /// Gets or sets the path to the libraries directory.
    /// </summary>
    [ObservableProperty] private string _librariesDirectoryPath;

    /// <summary>
    /// Gets or sets the path to the manifests directory.
    /// </summary>
    [ObservableProperty] private string _manifestsDirectoryPath;

    /// <summary>
    /// Gets or sets the path to the translations directory.
    /// </summary>
    [ObservableProperty] private string _translationsDirectoryPath;

    /// <summary>
    /// Gets or sets the path to the versions directory.
    /// </summary>
    [ObservableProperty] private string _versionsDirectoryPath;

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
        _enableAutomaticUpdates = enableAutomaticUpdates;
        _updateInterval = updateInterval;
        _language = language;
        _theme = theme;
        _assetsDirectoryPath = assetsDirectoryPath;
        _cacheDirectoryPath = cacheDirectoryPath;
        _iconsDirectoryPath = iconsDirectoryPath;
        _instancesDirectoryPath = instancesDirectoryPath;
        _javaDirectoryPath = javaDirectoryPath;
        _librariesDirectoryPath = librariesDirectoryPath;
        _manifestsDirectoryPath = manifestsDirectoryPath;
        _translationsDirectoryPath = translationsDirectoryPath;
        _versionsDirectoryPath = versionsDirectoryPath;
    }
}