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
        
        {"startup.title", "Starting Konkord Launcher"},
        
        {"startup.progress.initializing", "Initializing..."},
        {"startup.validation.dataFolder", "Validating data folder..."},
        {"startup.validation.dataFolderFailed", "Failed to validate data folder."},
        {"startup.validation.settings", "Validating settings..."},
        {"startup.validation.settingsFailed", "Failed to validate settings."},
        {"startup.validation.translations", "Validating translations..."},
        {"startup.validation.translationsFailed", "Failed to validate translations."},
        {"startup.validation.accounts", "Validating accounts..."},
        {"startup.validation.accountsFailed", "Failed to validate accounts."},
        {"startup.validation.skins", "Fetching skins..."},
        {"startup.validation.manifests", "Validating manifests..."},
        {"startup.validation.manifestsFailed", "Failed to validate manifests."},
        {"startup.validation.manifests.download", "Downloading {0} manifest {1}%"},
        {"startup.validation.java", "Validating Java installations..."},
        {"startup.validation.javaFailed", "Java is not installed. Please install Java 8 or minecraft will not run."},
        {"startup.validation.java.download", "Downloading Java {0} {1}%..."},
        
        {"startup.validation.java.exec.failedTitle", "Make Java Executable"},
        {"startup.validation.java.exec.failedMessage", "Failed to make '{0}' executable. Please make it executable manually."},
        
        {"startup.progress.checking", "Checking for updates..."},
        {"startup.progress.updating", "Updating..."},
        {"startup.progress.restarting", "Restarting..."},
        
        {"startup.update.fail", "Update Failed"},
        {"startup.update.failMessage", "Failed to launch updater."},
        
        {"startup.validation.github", "Validating GitHub cache..."},
        {"startup.validation.github.failed", "Failed to validate GitHub cache."},
        
        #endregion

        #region Main
        {"main.title", "Konkord Launcher"},
        {"main.subtitle", "Ready for Adventure?"},
        {"main.window.minimize", "Minimize"},
        {"main.window.maximize", "Maximize / Restore"},
        {"main.window.close", "Close"},
        
        {"main.sidebar.play", "Play"},
        {"main.sidebar.patch", "Patch Notes"},
        {"main.sidebar.accounts", "Accounts"},
        {"main.sidebar.accounts.loggedin", "LOGGED IN AS"},
        {"main.sidebar.accounts.guest", "Guest"},
        {"main.sidebar.settings", "Settings"},
        {"main.sidebar.about", "About"},
        {"main.sidebar.skins", "Skins"},
        
        {"main.sidebar.version.update.none", "Ready to launch. No update available."},
        {"main.sidebar.version.update.available", "Ready to launch. Update available."},

        {"main.page.play.title", "Instances"},
        {"main.page.play.empty", "No instances found. Create one to start playing."},
        {"main.page.play.uncategorized", "Uncategorized"},
        
        {"main.page.patch.title", "Patch Notes"},
        {"main.page.patch.empty", "No patches available."},
        {"main.page.patch.read", "Read More"},
        
        {"main.page.accounts.title", "Accounts"},
        {"main.page.accounts.empty", "No accounts found. Add one to start playing."},
        
        {"main.page.skins.title", "Skins"},
        {"main.page.skins.preview", "PREVIEW"},
        {"main.page.skins.model", "MODEL"},
        {"main.page.skins.model.wide", "Wide"},
        {"main.page.skins.model.slim", "Slim"},
        {"main.page.skins.skins", "SKINS"},
        {"main.page.skins.capes", "CAPES"},
        
        {"main.page.settings.title", "Settings"},
        
        #endregion

        #region Common

        {"common.select.directory", "Select a directory..."},
        {"common.select.file", "Select a file..."},
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
        {"common.enable", "Enable"},
        {"common.disable", "Disable"},
        {"common.toggle", "Toggle"},
        {"common.duplicate", "Duplicate"},
        {"common.rename", "Rename"},
        {"common.copy", "Copy"},
        {"common.clear", "Clear"},
        {"common.none", "None"},
        {"common.categories", "Categories"},
        {"common.bottom", "Bottom"},
        {"common.open.link", "Open Link"},
        {"common.or", "OR"},
        {"common.loading", "Loading"},
        {"common.launch", "Launch"},
        {"common.stop", "Stop"},
        {"common.export", "Export"},
        
        {"common.time.pass.minute", "{0} minutes ago"},
        {"common.time.pass.hour", "{0} hours ago"},
        {"common.time.pass.day", "{0} days ago"},
        {"common.time.pass.month", "{0} months ago"},
        {"common.time.pass.year", "{0} years ago"},
        
        {"common.selector.directory", "Select a Directory"},
        {"common.selector.file", "Select a File"},
        
        {"common.open.directory", "Open Directory"},
        
        {"common.dialog.rename", "Rename Dialog"},
        
        #endregion
        
        #region Instance
        
        {"instance.duplicate.title", "Instance Already Exists"},
        {"instance.duplicate.message", "An instance with this name already exists. Please choose a different name."},
        
        {"instance.java.notfound.title", "No Java {0} Version Found"},
        {"instance.java.notfound.message", "The instance needs Java {0}. Please install it manually."},
        
        {"instance.install.title", "Downloading Game Files..."},
        
        {"instance.reading.version_json", "Reading version json..."},
        {"instance.downloading.version_json", "Downloading version json {1}%..."},
        {"instance.reading.version_jar", "Reading version jar..."},
        {"instance.downloading.version_jar", "Downloading version jar {1}%..."},
        {"instance.reading.asset_index_json", "Reading asset index json..."},
        {"instance.downloading.asset_index_json", "Downloading asset index json {1}%..."},
        {"instance.reading.client_mappings", "Reading client mappings..."},
        {"instance.downloading.client_mappings", "Downloading client mappings {1}%..."},
        {"instance.reading.logging", "Reading logging json..."},
        {"instance.downloading.logging", "Downloading logging json {1}%..."},
        {"instance.reading.assets", "Reading assets..."},
        {"instance.downloading.assets", "Downloading assets {0}%..."},
        {"instance.reading.libraries", "Reading libraries..."},
        {"instance.downloading.libraries", "Downloading {0} library {1}%..."},
        {"instance.reading.natives", "Reading natives..."},
        {"instance.downloading.natives", "Downloading {0} native {1}%..."},
        {"instance.reading.manifest", "Reading manifest..."},
        {"instance.downloading.loader", "Downloading {0} loader {1}%..."},
        {"instance.building", "Building {0} {1}%..."},
        {"instance.building.arguments", "Building arguments..."},
        {"instance.downloading.installer", "Downloading {0} installer {1}%..."},
        {"instance.extracting.installer", "Extracting {0} installer..."},
        {"instance.reading.universal", "Reading {0} universal..."},
        {"instance.downloading.universal", "Downloading {0} universal {1}%..."},
        
        {"instance.version.change", "Change version"},
        {"instance.update.check", "Check for updates"},
        {"instance.download.shaders", "Download shaders"},
        {"instance.download.resourcePacks", "Download resource packs"},
        {"instance.download.mods", "Download mods"},
        
        {"instance.view.logs", "View Logs"},
        {"instance.change.group", "Change Group"},
        {"instance.change.icon", "Change Icon"},
        {"instance.logs.title", "Latest Logs of"},
        
        {"instance.rename.title", "New name of the instance"},
        {"instance.rename.duplicate", "An instance with this name already exists."},
        {"instance.change.group.title", "New group of the instance"},
        {"instance.delete.title", "Are you sure?"},
        {"instance.delete.message", "This will delete the instance '{0}' and all its files. This action cannot be undone."},

        #region Create

        {"instance.create.title", "Create Instance"},
        
        {"instance.create.tab.custom", "Custom"},
        {"instance.create.tab.modpacks", "Modpacks"},
        {"instance.create.tab.import", "Import"},
        {"instance.create.name", "Instance Name"},
        {"instance.create.group", "Group"},
        {"instance.create.minecraftVersion", "Minecraft Version"},
        {"instance.create.table.version", "Version"},
        {"instance.create.table.released", "Released"},
        {"instance.create.table.type", "Type"},
        {"instance.create.releases", "Releases"},
        {"instance.create.snapshots", "Snapshots"},
        {"instance.create.betas", "Betas"},
        {"instance.create.alphas", "Alphas"},
        {"instance.create.experiments", "Experiments" },
        {"instance.create.modloader", "Mod Loader"},
        {"instance.create.button", "Create Instance"},
        {"instance.create.subtitle.platform", "Platform"},
        
        {"instance.create.category.adventure", "Adventure"},
        {"instance.create.category.challenging", "Challenging"},
        {"instance.create.category.combat", "Combat"},
        {"instance.create.category.kitchenSink", "Kitchen Sink"},
        {"instance.create.category.lightweight", "Lightweight"},
        {"instance.create.category.magic", "Magic"},
        {"instance.create.category.multiplayer", "Multiplayer"},
        {"instance.create.category.optimization", "Optimization"},
        {"instance.create.category.quests", "Quests"},
        {"instance.create.category.technology", "Technology"},
        
        {"instance.create.modpack.version", "Pack Version"},
        {"instance.create.modpack.select", "Select a modpack"},
        
        {"instance.create.import.description", "Import an instance from a file or URL."},
        {"instance.create.import.source", "Import Source"},
        {"instance.create.import.source.file", "From File"},
        {"instance.create.import.source.url", "From Url"},
        {"instance.create.import.select.instanceFile", "Select Instance File"},
        {"instance.create.import.formats", "Supported formats: .zip, .mrpack, .json"},
        {"instance.create.import.select.instanceUrl", "Instance URL"},
        
        #endregion

        #region Edit

        {"instance.edit.window", "Edit Instance"},
        {"instance.edit.title", "Editing Instance:"},
        
        {"instance.edit.downloadPacks", "Download Packs"},
        {"instance.edit.downloadMods", "Download Mods"},
        {"instance.edit.copySeed", "Copy Seed"},
        {"instance.edit.joinServer", "Join"},
        
        {"instance.edit.tab.logs", "Logs"},
        {"instance.edit.tab.mods", "Mods"},
        {"instance.edit.tab.resourcePacks", "Resource Packs"},
        {"instance.edit.tab.shaderPacks", "Shader Packs"},
        {"instance.edit.tab.worlds", "Worlds"},
        {"instance.edit.tab.servers", "Servers"},
        {"instance.edit.tab.screenshots", "Screenshots"},
        {"instance.edit.tab.settings", "Settings"},
        
        {"instance.edit.latestLog", "Latest Log"},
        
        {"instance.edit.subTitle.window", "Window"},
        {"instance.edit.subTitle.console", "Console"},
        {"instance.edit.subTitle.performance", "Performance"},
        
        {"instance.edit.settings.console.showStart", "Show console while minecraft is running"},
        {"instance.edit.settings.console.closeExit", "Close console when minecraft quits"},
        {"instance.edit.settings.console.showCrash", "Show console when minecraft crashes"},
        
        {"instance.edit.table.enabled", "Enabled"},
        {"instance.edit.table.image", "Image"},
        {"instance.edit.table.name", "Name"},
        {"instance.edit.table.version", "Version"},
        {"instance.edit.table.lastModified", "Last Modified"},
        {"instance.edit.table.type", "Type"},
        {"instance.edit.table.size", "Size"},
        {"instance.edit.table.path", "Path"},
        {"instance.edit.table.provider", "Provider"},
        {"instance.edit.table.lastPlayed", "Last Played"},
        {"instance.edit.table.gameMode", "GameMode"},
        {"instance.edit.table.ipAddress", "IP Address"},
        
        #endregion
        
        #endregion
        
        #region Settings
        
        {"settings.tab.java", "Java"},
        {"settings.tab.minecraft", "Minecraft"},
        {"settings.tab.launcher", "Launcher"},
        {"settings.tab.game", "Game"},
        {"settings.tab.customCommands", "Custom Commands"},
        {"settings.tab.environment", "Environment"},
        {"settings.tab.misc", "Misc"},
        
        {"settings.launcher.autoUpdate", "Enable automatic updates"},
        {"settings.launcher.updateInterval", "Update interval (hours)"},
        {"settings.launcher.language", "Language"},
        {"settings.launcher.theme", "Theme"},
        {"settings.launcher.subtitle.general", "GENERAL"},
        {"settings.launcher.subtitle.directories", "DIRECTORIES"},
        
        {"settings.launcher.dir.assets", "Assets"},
        {"settings.launcher.dir.cache", "Cache"},
        {"settings.launcher.dir.instances", "Instances"},
        {"settings.launcher.dir.icons", "Icons"},
        {"settings.launcher.dir.java", "Java (local)"},
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
        {"settings.java.jvmArgs", "JVM ARGUMENTS"},

        {"settings.misc.subtitle.customCommands", "CUSTOM COMMANDS"},
        {"settings.misc.prelaunchCommand", "Pre-launch"},
        {"settings.misc.prelaunchCommand.description", "Runs before the instance starts. The instance will not begin until this command has completed."},
        {"settings.misc.wrapperCommand", "Wrapper"},
        {"settings.misc.wrapperCommand.description", "Wraps the Java Virtual Machine. The %command% placeholder is replaced with the Java executable path and all the Minecraft arguments. %command% is optional."},
        {"settings.misc.postExitCommand", "Post-exit"},
        {"settings.misc.postExitCommand.description", "Runs after the instance has stopped."},
        {"settings.misc.subtitle.nativeLibraries", "NATIVE LIBRARIES"},
        {"settings.misc.customGLFW", "Use custom GLFW library"},
        {"settings.misc.pathGLFW", "GLFW library path"},
        {"settings.misc.customOpenAL", "Use custom OpenAL library"},
        {"settings.misc.pathOpenAL", "OpenAL library path"},
        {"settings.misc.subtitle.performance", "PERFORMANCE"},
        {"settings.misc.enableMangoHud", "Enable MangoHUD"},
        {"settings.misc.enableGameMode", "Enable Feral GameMode"},
        {"settings.misc.useDedicatedGpu", "Use dedicated GPU"},
        
        {"settings.environment.enable", "Enable Environment Variables"},
        {"settings.environment.name", "Name"},
        {"settings.environment.value", "Value"},
        
        {"settings.misc.subtitle.preferences", "Preferences"},
        {"settings.misc.overrideAccount", "Override default account"},
        {"settings.misc.account", "Account"},
        {"settings.misc.serverQuickPlay", "Set server address to join on launch"},
        {"settings.misc.serverAddress", "Server address"},
        
        #endregion

        #region About

        {"about.tab.about", "About"},
        {"about.tab.license", "License"},
        {"about.tab.credits", "Credits"},
        
        {"about.description", "Konkord Launcher is an open-source, lightweight, and highly customizable Minecraft launcher designed for power users and modding enthusiasts. It provides advanced control over game instances, resource management, and versioning."},
        {"about.version", "Version:"},
        {"about.buildDate", "Build date:"},
        {"about.branch", "Branch:" },
        
        {"about.license.title", "License"},
        
        {"about.credits.projectMaintainers", "Project Maintainers"},
        {"about.credits.contributors", "Contributors"},
        {"about.credits.translators", "Translators"},
        {"about.credits.testers", "Testers"},
        {"about.credits.specialThanks", "Special Thanks"},
        {"about.credits.thirdParty", "Third Party"},

        #endregion
        
        #region Auth

        {"auth.title", "Add New Account"},
        {"auth.tab.microsoft", "Microsoft"},
        {"auth.tab.offline", "Offline"},
        {"auth.tab.custom", "Custom"},
        {"auth.logging_in", "Logging in..."},
        
        {"auth.microsoft.description", "Sign in with your Microsoft Account to access official Minecraft servers and features."},
        {"auth.microsoft.login", "Sign in with Microsoft"},
        {"auth.microsoft.note", "This will open a browser window for authentication."},
        {"auth.microsoft.browser", "Login in browser"},
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

        {"auth.code.creating", "Creating device code..."},
        {"auth.code.note", "Open https://microsoft.com/link or scan the QR image and enter the above code."},
        
        {"account.login.failed", "Login Failed"},
        {"account.login.microsoft.failed", "Failed to login to Microsoft account. Please try again later."},
        {"account.login.microsoft.null", "Failed to retrieve Microsoft account information. Please try again later."},
        {"account.duplicate", "Account Already Exists"},
        {"account.duplicate.microsoft", "You have already added this Microsoft account."},
        {"account.duplicate.offline", "An account with this username already exists. Please choose a different username."},
        {"account.empty.name", "Name Required"},
        {"account.empty.name.desc", "Please enter a username for the account."},
        
        {"account.none.title", "No Account Selected"},
        {"account.none.message", "Please select or create an account to use"},
        
        #endregion

        #region Java

        {"java.title", "Java Selector"},
        {"java.select", "Select Java Version"},
        {"java.table.major", "Major"},
        {"java.table.version", "Version"},
        {"java.table.architecture", "Architecture"},
        {"java.table.path", "Path"},

        #endregion

        #region Updater

        {"updater.title", "Updater"},
        
        {"updater.downloading", "Downloading update {0}%..."},
        {"updater.extracting", "Extracting update..."},
        {"updater.applying", "Applying update..."},
        {"updater.finalizing", "Finalizing update..."},
        {"updater.completed", "Update completed! Restarting..."},

        #endregion
        
        {"iconSelector.title", "Select a Icon"}
    };

    /// <summary>
    /// Gets the dictionary of default translations.
    /// </summary>
    public static Dictionary<string, string> Translations => _translations;
}