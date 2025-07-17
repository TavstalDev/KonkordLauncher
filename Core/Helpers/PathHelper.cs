namespace Tavstal.KonkordLauncher.Core.Helpers;

public static class PathHelper
{
    #region Directories

    private static readonly string _appDataRoamingDir =
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    public static string AppDataRoamingDir
    {
        get { return _appDataRoamingDir; }
    }

    private static readonly string _mainDir = Path.Combine(_appDataRoamingDir, ".konkordlauncher");

    public static string MainDirectory
    {
        get { return _mainDir; }
    }

    private static readonly string _instancesDir = Path.Combine(_mainDir, "instances");

    public static string InstancesDir
    {
        get { return _instancesDir; }
    }

    private static readonly string _translationsDir = Path.Combine(_mainDir, "translations");

    public static string TranslationsDir
    {
        get { return _translationsDir; }
    }

    private static readonly string _versionsDir = Path.Combine(_mainDir, "versions");

    public static string VersionsDir
    {
        get { return _versionsDir; }
    }

    private static readonly string _manifestDir = Path.Combine(_mainDir, "manifests");

    public static string ManifestDir
    {
        get { return _manifestDir; }
    }

    private static readonly string _cacheDir = Path.Combine(_mainDir, "cache");

    public static string CacheDir
    {
        get { return _cacheDir; }
    }

    private static readonly string _librariesDir = Path.Combine(_mainDir, "libraries");

    public static string LibrariesDir
    {
        get { return _librariesDir; }
    }

    private static readonly string _assetsDir = Path.Combine(_mainDir, "assets");

    public static string AssetsDir
    {
        get { return _assetsDir; }
    }

    private static readonly string _tempDir = Path.Combine(_mainDir, "temp");

    public static string TempDir
    {
        get { return _tempDir; }
    }

    #endregion

    #region Files

    private static readonly string _launcherJsonFile = Path.Combine(_mainDir, "launcher.json");

    public static string LauncherJsonFile
    {
        get { return _launcherJsonFile; }
    }

    private static readonly string _accountsJsonFile = Path.Combine(_mainDir, "accounts.json");

    public static string AccountsJsonFile
    {
        get { return _accountsJsonFile; }
    }

    private static readonly string _vanillaManifestJsonFile = Path.Combine(_manifestDir, "vanillaManifest.json");

    public static string VanillaManifesJsonFile
    {
        get { return _vanillaManifestJsonFile; }
    }

    private static readonly string _forgeManifestJsonFile = Path.Combine(_manifestDir, "forgeManifest.json");

    public static string ForgeManifestJsonFile
    {
        get { return _forgeManifestJsonFile; }
    }

    private static readonly string _fabricManifestJsonFile = Path.Combine(_manifestDir, "fabricManifest.json");

    public static string FabricManifestJsonFile
    {
        get { return _fabricManifestJsonFile; }
    }

    private static readonly string _quiltManifestJsonFile = Path.Combine(_manifestDir, "quiltManifest.json");

    public static string QuiltManifestJsonFile
    {
        get { return _quiltManifestJsonFile; }
    }

    private static readonly string _skinLibJsonFile = Path.Combine(_mainDir, "skinLibrary.json");

    public static string SkinLibraryJsonFile
    {
        get { return _skinLibJsonFile; }
    }

    #endregion
}