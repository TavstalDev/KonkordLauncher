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
        
        {"startup.title", "Konkord Launcher - Starting..."},
        
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
        
        {"main.title", "Konkord Launcher - Main Window"},
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
        {"main.page.skins.alert.error", "Failed to upload skin. Please try again later."},
        {"main.page.skins.alert.cape.change", "Failed to change cape. Please try again later."},
        {"main.page.skins.alert.cape.unexpected", "Unexpected error happened while selecting the cape."},
        {"main.page.skins.alert.model.change", "Failed to change model. Please try again later."},
        
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
        {"common.error", "Error"},
        {"common.warning", "Warning"},
        {"common.success", "Success"},
        {"common.review", "Review"},
        
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
        
        {"instance.java.notfound.title", "No Java {0} Found"},
        {"instance.java.notfound.message", "The instance needs Java {0}. Please install it manually."},
        {"instance.java.missing.title", "Java {0} Missing"},
        {"instance.java.missing.message", "Do you want to install it ?"},
        {"instance.java.error.title", "Fail"},
        {"instance.java.error.message", "Failed to download java."},
        
        {"instance.install.title", "Downloading"},
        {"instance.install.description", "Downloading and installing files"},
        
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
        
        {"instance.download.file", "Downloading {0} {1}%..."},
        
        {"instance.view.logs", "View Logs"},
        {"instance.change.group", "Change Group"},
        {"instance.change.icon", "Change Icon"},
        {"instance.change.version", "Change Version"},
        {"instance.logs.title", "Latest Logs of"},
        
        {"instance.rename.title", "New name of the instance"},
        {"instance.rename.duplicate", "An instance with this name already exists."},
        {"instance.change.group.title", "New group of the instance"},
        {"instance.delete.title", "Are you sure?"},
        {"instance.delete.message", "This will delete the instance '{0}' and all its files. This action cannot be undone."},

        #region Create

        {"instance.create.title", "KonkordLauncher - New Instance"},
        {"instance.create.instance.title", "Create Instance"},
        {"instance.create.category.information.title", "INFORMATION"},
        
        {"instance.create.tab.custom", "Custom"},
        {"instance.create.tab.modpacks", "Modpacks"},
        {"instance.create.tab.import", "Import"},
        {"instance.create.name", "Instance Name"},
        {"instance.create.group", "Group"},
        {"instance.create.category.minecraft.version.title", "MINECRAFT VERSION"},
        {"instance.create.table.version", "Version"},
        {"instance.create.table.released", "Released"},
        {"instance.create.table.type", "Type"},
        {"instance.create.releases", "Releases"},
        {"instance.create.snapshots", "Snapshots"},
        {"instance.create.betas", "Betas"},
        {"instance.create.alphas", "Alphas"},
        {"instance.create.experiments", "Experiments" },
        {"instance.create.category.mod_loader.title", "MOD LOADER"},
        {"instance.create.button", "Create Instance"},
        
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
        {"instance.create.modpack.platform", "Platform"},
        {"instance.create.modpack.mod.loader", "Mod Loader"},
        {"instance.create.modpack.minecraft.version", "Minecraft Version"},
        {"instance.create.modpack.preview.select", "Select a modpack to see its preview."},
        
        {"instance.create.import.type", "IMPORT TYPE" },
        {"instance.create.import.description", "Import an instance from a file or URL."},
        {"instance.create.import.source", "Import Source"},
        {"instance.create.import.source.file", "From File"},
        {"instance.create.import.source.url", "From Url"},
        {"instance.create.import.select_type.title", "SELECT TYPE"},
        {"instance.create.import.select_file.title", "SELECT FILE"},
        {"instance.create.import.import_from_url.title", "IMPORT FROM URL"},
        {"instance.create.import.formats", "Supported formats: .zip, .mrpack, .json"},
        
        {"instance.create.import.preview.title", "INSTANCE INFO"},
        {"instance.create.import.preview.name", "Name:"},
        {"instance.create.import.preview.version", "Version:"},
        {"instance.create.import.preview.mod.loader", "Mod loader:"},
        
        {"instance.create.import.error.invalid_path.title", "Invalid Import Path"},
        {"instance.create.import.error.invalid_path.message", "The selected file does not exist or is not a valid format. Please select a valid .zip or .mrpack file."},
        {"instance.create.import.failed.title", "Import Failed"},
        {"instance.create.import.failed.message", "Failed to import the instance. Please check the file and try again."},
        {"instance.create.import.error.import_failed.title", "Import Error"},
        {"instance.create.import.error.import_failed.message", "An error occurred while importing from the URL. Please check the URL and try again."},
        
        #endregion

        #region Edit

        {"instance.edit.window", "KonkordLauncher - Edit Instance"},
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
        
        {"instance.edit.log.title", "KonkordLauncher - Logs"},
        {"instance.edit.latestLog", "LATEST LOG"},
        
        {"instance.edit.subTitle.window", "WINDOW"},
        {"instance.edit.subTitle.console", "CONSOLE"},
        {"instance.edit.subTitle.performance", "PERFORMANCE"},
        {"instance.edit.subTitle.mods", "MODS"},
        {"instance.edit.subTitle.resourcePacks", "RESOURCE PACKS"},
        {"instance.edit.subTitle.shaderPacks", "SHADER PACKS"},
        {"instance.edit.subTitle.worlds", "WORLDS"},
        {"instance.edit.subTitle.servers", "SERVERS"},
        {"instance.edit.subTitle.screenshots", "SCREENSHOTS"},
        {"instance.edit.subTitle.settings", "SETTINGS"},
        
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

        #region Exporting

        {"instance.export.window", "KonkordLauncher - Export Instance"},
        {"instance.export.title", "Export Instance"},
        {"instance.export.subtitle", "Create a distributable package with selected files and metadata."},
        {"instance.export.name", "Name"},
        {"instance.export.version", "Version"},
        {"instance.export.summary", "Summary"},
        {"instance.export.selection.title", "EXPORT SELECTION"},
        {"instance.export.progress", "Exporting..."},
        {"instance.export.alert.empty.name.or.version", "Instance name and version cannot be empty. Please provide valid values for both fields."},
        {"instance.export.alert.no.directory", "No directory selected. Please select a directory to export the instance."},
        {"instance.export.alert.file.exists", "File already exists. Please select a different directory or change the instance name/version to avoid conflicts."},
        {"instance.export.alert.error", "Failed to export instance. Please try again."},
        {"instance.export.alert.success", "Instance exported successfully to {0}."},

        #endregion

        #region Resources

        {"instance.resource.download.window", "Konkord Launcher - Resource Downloader"},
        {"instance.resource.download.title", "Download Resource"},
        {"instance.resource.download.description", "Select the resources that you want to download."},
        {"instance.resource.download.preview", "Select a resource to see its preview."},
        {"instance.resource.download.selected", "[Selected]"},
        {"instance.resource.download.installed", "[Installed]"},
        {"instance.resource.review.window", "Konkord Launcher - Resource Downloader Review"},
        {"instance.resource.review.title", "Review Selection"},
        {"instance.resource.review.description", "You are about to download the following content:"},
        {"instance.resource.review.file", "File:"},
        {"instance.resource.review.version", "Version:"},
        {"instance.resource.review.platform", "Platform:"},
        {"instance.resources.download.complete", "Download Complete"},
        {"instance.resources.download.complete.description", "The selected resources have been downloaded and added to the instance."},

        #endregion
        
        {"instance.version.selector.window", "Konkord Launcher - Version Selector"},
        {"instance.version.selector.title", "Version Selector"},
        {"instance.version.selector.description", "Change the version of the instance."},
        
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
        
        {"settings.environment.subtitle", "ENVIRONMENT"},
        {"settings.environment.enable", "Enable Environment Variables"},
        {"settings.environment.name", "Name"},
        {"settings.environment.value", "Value"},
        
        {"settings.misc.subtitle.preferences", "PREFERENCES"},
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

        {"auth.title", "KonkordLauncher - New Account"},
        {"auth.add.account", "Add New Account"},
        {"auth.tab.microsoft", "Microsoft"},
        {"auth.tab.offline", "Offline"},
        {"auth.tab.custom", "Custom"},
        {"auth.logging_in", "Logging in..."},
        {"auth.creating.title", "Creating New Account"},
        
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

        {"java.title", "KonkordLauncher - Java Selector"},
        {"java.select", "Select Java Version"},
        {"java.select.subtitle", "Select a Java version to use for launching an instance."},
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

        #region Modrinth

        { "modrinth.category.adventure", "Adventure" },
        { "modrinth.category.cursed", "Cursed" },
        { "modrinth.category.decoration", "Decoration" },
        { "modrinth.category.economy", "Economy" },
        { "modrinth.category.equipment", "Equipment" },
        { "modrinth.category.food", "Food" },
        { "modrinth.category.game_mechanics", "Game mechanics" },
        { "modrinth.category.library", "Library" },
        { "modrinth.category.magic", "Magic" },
        { "modrinth.category.management", "Management" },
        { "modrinth.category.minigame", "Minigame" },
        { "modrinth.category.mobs", "Mobs" },
        { "modrinth.category.optimization", "Optimization" },
        { "modrinth.category.social", "Social" },
        { "modrinth.category.storage", "Storage" },
        { "modrinth.category.technology", "Technology" },
        { "modrinth.category.transportation", "Transportation" },
        { "modrinth.category.utility", "Utility" },
        { "modrinth.category.worldgen", "Worldgen" },

        #endregion
        
        {"iconSelector.window", "KonkordLauncher - Icon Selector"},
        {"iconSelector.title", "Select a Icon"},
        {"iconSelector.subtitle", "Select a icon for an instance."},
        {"alert.title", "KonkordLauncher - Alert"},
        {"input.title", "KonkordLauncher - Input"},
        
        {"install.title", "KonkordLauncher - Progress"},
        {"install.progress", "Progress"},
    };

    /// <summary>
    /// Gets the dictionary of default translations.
    /// </summary>
    public static Dictionary<string, string> Translations => _translations.ToDictionary();
}