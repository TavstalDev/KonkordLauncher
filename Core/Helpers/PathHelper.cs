namespace Tavstal.KonkordLauncher.Core.Helpers;

public class PathHelper
{
    public static readonly string ApplicationDir = Directory.GetCurrentDirectory();
    
    public static readonly string InstancesDir = Path.Combine(ApplicationDir, "instances");
    
    public static readonly string TranslationsDir = Path.Combine(ApplicationDir, "translations");
    
    public static readonly string VersionsDir = Path.Combine(ApplicationDir, "versions");
    
    public static readonly string ManifestDir = Path.Combine(ApplicationDir, "manifests");
    
    public static readonly string CacheDir = Path.Combine(ApplicationDir, "cache");
    
    public static readonly string LibrariesDir = Path.Combine(ApplicationDir, "libraries");
    
    public static readonly string AssetsDir = Path.Combine(ApplicationDir, "assets");
    
    
    public static readonly string VanillaManifestPath = Path.Combine(ManifestDir, "vanillaManifest.json");
    public static readonly string ForgeManifestPath = Path.Combine(ManifestDir, "forgeManifest.json");
    public static readonly string FabricManifestPath = Path.Combine(ManifestDir, "fabricManifest.json");
    public static readonly string QuiltManifestPath = Path.Combine(ManifestDir, "quiltManifest.json");
    
    
    public static readonly string LauncherConfigPath = Path.Combine(ApplicationDir, "config.json");
    public static readonly string LauncherAccountsPath = Path.Combine(ApplicationDir, "accounts.json");
}