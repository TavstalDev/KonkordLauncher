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
        
        {"startup.progress.initializing", "Initializing..."},
        {"startup.validation.dataFolder", "Validating data folder..."},
        {"startup.validation.dataFolderFailed", "Failed to validate data folder."},
        {"startup.validation.settings", "Validating settings..."},
        {"startup.validation.settingsFailed", "Failed to validate settings."},
        {"startup.validation.translations", "Validating translations..."},
        {"startup.validation.translationsFailed", "Failed to validate translations."},
        {"startup.validation.accounts", "Validating accounts..."},
        {"startup.validation.accountsFailed", "Failed to validate accounts."},
        {"startup.validation.manifests", "Validating manifests..."},
        {"startup.validation.manifestsFailed", "Failed to validate manifests."},
        {"startup.validation.java", "Validating Java installations..."},
        {"startup.validation.javaFailed", "Java is not installed. Please install Java 8 or minecraft will not run."},
        
        {"startup.progress.checking", "Checking for updates..."},
        {"startup.progress.updating", "Updating..."},
        {"startup.progress.restarting", "Restarting..."},
        
        #endregion

        #region Main
        {"main.title", "Konkord Launcher"},
        {"main.subtitle", "Ready for Adventure?"},
        
        {"main.sidebar.play", "Play"},
        {"main.sidebar.news", "News"},
        {"main.sidebar.accounts", "Accounts"},
        {"main.sidebar.settings", "Settings"},
        
        {"main.sidebar.version.update.none", "Ready to launch. No update available."},
        {"main.sidebar.version.update.available", "Ready to launch. Update available."},

        {"main.page.play.title", "Launch Game"},
        {"main.page.play.empty", "No instances found. Create one to start playing."},
        
        {"main.page.news.title", "News"},
        {"main.page.news.empty", "No news available."},
        {"main.page.news.read", "Read More"},
        
        {"main.page.accounts.title", "Accounts"},
        {"main.page.accounts.empty", "No accounts found. Add one to start playing."},
        
        {"main.page.settings.title", "Settings"},
        
        #endregion

        #region Settings
        
        {"settings.tab.java", "Java"},
        {"settings.tab.minecraft", "Minecraft"},
        {"settings.tab.launcher", "Launcher"},
        {"settings.tab.misc", "Misc"},
        
        

        #endregion
    };

    /// <summary>
    /// Gets the dictionary of default translations.
    /// </summary>
    public static Dictionary<string, string> Translations => _translations;
}