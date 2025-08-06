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
        {"main.sidebar.patch", "Patch Notes"},
        {"main.sidebar.accounts", "Accounts"},
        {"main.sidebar.settings", "Settings"},
        {"main.sidebar.about", "About"},
        
        {"main.sidebar.version.update.none", "Ready to launch. No update available."},
        {"main.sidebar.version.update.available", "Ready to launch. Update available."},

        {"main.page.play.title", "Launch Game"},
        {"main.page.play.empty", "No instances found. Create one to start playing."},
        
        {"main.page.patch.title", "Patch Notes"},
        {"main.page.patch.empty", "No patches available."},
        {"main.page.patch.read", "Read More"},
        
        {"main.page.accounts.title", "Accounts"},
        {"main.page.accounts.empty", "No accounts found. Add one to start playing."},
        
        {"main.page.settings.title", "Settings"},
        
        #endregion

        #region Common

        {"common.select.directory", "Select a directory..."},
        {"common.ok", "OK"},
        {"common.cancel", "Cancel"},
        {"common.close", "Close"},
        {"common.save", "Save"},
        {"common.delete", "Delete"},
        {"common.edit", "Edit"},
        {"common.add", "Add"},
        {"common.remove", "Remove"},
        {"common.select", "Select"},
        {"common.search", "Search..."},
        
        {"common.selector.directory", "Select a Directory"},
        {"common.select.file", "Select a File"},
        
        #endregion
        
        #region Settings
        
        {"settings.tab.java", "Java"},
        {"settings.tab.minecraft", "Minecraft"},
        {"settings.tab.launcher", "Launcher"},
        {"settings.tab.misc", "Misc"},
        
        {"settings.launcher.autoUpdate", "Enable automatic updates"},
        {"settings.launcher.updateInterval", "Update interval (hours)"},
        {"settings.launcher.language", "Language"},
        {"settings.launcher.theme", "Theme"},
        {"settings.launcher.subtitle.directories", "Directories"},
        
        {"settings.launcher.dir.assets", "Assets"},
        {"settings.launcher.dir.cache", "Cache"},
        {"settings.launcher.dir.instances", "Instances"},
        {"settings.launcher.dir.icons", "Icons"},
        {"settings.launcher.dir.libraries", "Libraries"},
        {"settings.launcher.dir.manifests", "Manifests"},
        {"settings.launcher.dir.translations", "Translations"},
        {"settings.launcher.dir.versions", "Versions"},

        {"settings.minecraft.startMaximized", "Start Minecraft maximized"},
        {"settings.minecraft.windowWidth", "Window width"},
        {"settings.minecraft.windowHeight", "Window height"},
        {"settings.minecraft.closeOpen", "Close launcher after game opens"},
        {"settings.minecraft.closeExit", "Close launcher after game closes"},

        {"settings.java.minMemory", "Minimum memory"},
        {"settings.java.maxMemory", "Maximum memory"},
        {"settings.java.permaGen", "PermaGen"},
        {"settings.java.path", "Java path"},
        {"settings.java.jvmArgs", "JVM Arguments"},

        {"settings.misc.subtitle.customCommands", "Custom Commands"},
        {"settings.misc.prelaunchCommand", "Pre-launch"},
        {"settings.misc.wrapperCommand", "Wrapper"},
        {"settings.misc.postExitCommand", "Post-exit"},
        {"settings.misc.subtitle.nativeLibraries", "Native Libraries"},
        {"settings.misc.customGLFW", "Use custom GLFW library"},
        {"settings.misc.pathGLFW", "GLFW library path"},
        {"settings.misc.customOpenAL", "Use custom OpenAL library"},
        {"settings.misc.pathOpenAL", "OpenAL library path"},
        {"settings.misc.subtitle.performance", "Performance"},
        {"settings.misc.enableMangoHud", "Enable MangoHUD"},
        {"settings.misc.enableGameMode", "Enable Feral GameMode"},
        {"settings.misc.useDedicatedGpu", "Use dedicated GPU"},
        
        #endregion

        #region Auth

        {"auth.title", "Add New Account"},
        {"auth.tab.microsoft", "Microsoft"},
        {"auth.tab.offline", "Offline"},
        {"auth.tab.custom", "Custom"},
        
        {"auth.microsoft.description", "Sign in with your Microsoft Account to access official Minecraft servers and features."},
        {"auth.microsoft.login", "Sign in with Microsoft"},
        {"auth.microsoft.note", "This will open a browser window for authentication."},
        {"auth.offline.description", "Play with an offline account."},
        {"auth.offline.login", "Login as Offline User"},
        {"auth.offline.note", "Offline accounts can only connect to offline servers; online multiplayer is unavailable."},
        
        {"auth.listener.starting", "Waiting for authentication..."},
        {"auth.listener.failed", "Failed to start authentication listener."},
        {"auth.listener.success", "Authentication successful!"},
        {"auth.listener.error", "An error occurred during authentication."},
        {"auth.listener.cancelled", "Authentication was cancelled."},
        {"auth.listener.callback", "Received authentication callback."},
        {"auth.microsoft.authenticating", "Requesting Microsoft token..."},
        {"auth.xbox.authenticating", "Requesting Xbox token..."},
        {"auth.xbox.xsts", "Requesting XSTS token..."},
        {"auth.minecraft.authenticating", "Requesting Minecraft token..."},
        {"auth.minecraft.ownership", "Checking Minecraft ownership..."},
        {"auth.minecraft.profile", "Requesting Minecraft profile..."},

        #endregion

        #region Java

        {"java.title", "Java Selector"},
        {"java.select", "Select Java Version"},
        {"java.table.major", "Major"},
        {"java.table.version", "Version"},
        {"java.table.architecture", "Architecture"},
        {"java.table.path", "Path"},

        #endregion
        {"", ""},
    };

    /// <summary>
    /// Gets the dictionary of default translations.
    /// </summary>
    public static Dictionary<string, string> Translations => _translations;
}