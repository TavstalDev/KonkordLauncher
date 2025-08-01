namespace Tavstal.KonkordLauncher.Common.Translation;

/// <summary>
/// Provides default translations for the application.
/// </summary>
public static class DefaultTranslationProvider
{
    /// <summary>
    /// A dictionary containing default translation key-value pairs.
    /// </summary>
    private static readonly Dictionary<string, string> _translations = new()
    {
        #region Startup
        
        {"startup.title", "Konkord Launcher"},
        {"startup.subtitle", "A modern Minecraft launcher"},
        {"startup.progress.checking", "Checking for updates..."},
        {"startup.progress.updating", "Updating..."},
        {"startup.progress.initializing", "Initializing..."},
        {"startup.progress.loading", "Loading..."},
        {"startup.progress.loadingAssets", "Loading assets..."},
        {"startup.progress.loadingLibraries", "Loading libraries..."},
        {"startup.progress.loadingInstances", "Loading instances..."},
        
        #endregion
    };

    /// <summary>
    /// Gets the dictionary of default translations.
    /// </summary>
    public static Dictionary<string, string> Translations => _translations;
}