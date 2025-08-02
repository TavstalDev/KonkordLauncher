using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Common.Models;

namespace Tavstal.KonkordLauncher.Desktop.Models.Config.Launcher;

public partial class LauncherConfigModel : ObservableObject
{
    [ObservableProperty] private bool _enableAutomaticUpdates;

    [ObservableProperty] private uint _updateInterval;

    [ObservableProperty] private string _language;

    [ObservableProperty] private EThemeType _theme;

    [ObservableProperty] private string _assetsDirectoryPath;

    [ObservableProperty] private string _cacheDirectoryPath;

    [ObservableProperty] private string _iconsDirectoryPath;

    [ObservableProperty] private string _instancesDirectoryPath;

    [ObservableProperty] private string _librariesDirectoryPath;

    [ObservableProperty] private string _manifestsDirectoryPath;

    [ObservableProperty] private string _translationsDirectoryPath;

    [ObservableProperty] private string _versionsDirectoryPath;

    public LauncherConfigModel() {}
    
    public LauncherConfigModel(bool enableAutomaticUpdates, uint updateInterval, string language, EThemeType theme, string assetsDirectoryPath, string cacheDirectoryPath, string iconsDirectoryPath, string instancesDirectoryPath, string librariesDirectoryPath, string manifestsDirectoryPath, string translationsDirectoryPath, string versionsDirectoryPath)
    {
        _enableAutomaticUpdates = enableAutomaticUpdates;
        _updateInterval = updateInterval;
        _language = language;
        _theme = theme;
        _assetsDirectoryPath = assetsDirectoryPath;
        _cacheDirectoryPath = cacheDirectoryPath;
        _iconsDirectoryPath = iconsDirectoryPath;
        _instancesDirectoryPath = instancesDirectoryPath;
        _librariesDirectoryPath = librariesDirectoryPath;
        _manifestsDirectoryPath = manifestsDirectoryPath;
        _translationsDirectoryPath = translationsDirectoryPath;
        _versionsDirectoryPath = versionsDirectoryPath;
    }
}