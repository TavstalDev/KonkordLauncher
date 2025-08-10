using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Helpers;

namespace Tavstal.KonkordLauncher.Common.Models.Config;

/// <summary>
/// Represents the configuration settings for the launcher, including update settings, 
/// language, theme, and directory paths.
/// </summary>
public class LauncherConfig
{
    /// <summary>
    /// Gets or sets a value indicating whether automatic updates are enabled.
    /// </summary>
    [JsonProperty("enableAutomaticUpdates"), JsonPropertyName("enableAutomaticUpdates")]
    public bool EnableAutomaticUpdates { get; set; }
    
    /// <summary>
    /// Gets or sets the update interval in hours.
    /// </summary>
    [JsonProperty("updateInterval"), JsonPropertyName("updateInterval")]
    public uint UpdateInterval { get; set; }

    /// <summary>
    /// Gets or sets the date and time for the next update check.
    /// </summary>
    [JsonProperty("nextUpdateCheck"), JsonPropertyName("nextUpdateCheck")]
    public DateTime NextUpdateCheck { get; set; }
    
    /// <summary>
    /// Gets or sets the language of the launcher.
    /// </summary>
    [JsonProperty("language"), JsonPropertyName("language")]
    public string Language { get; set; }
    
    /// <summary>
    /// Gets or sets the theme of the launcher.
    /// </summary>
    [JsonProperty("theme"), JsonPropertyName("theme")]
    public EThemeType Theme { get; set; }
    
    /// <summary>
    /// Gets or sets the file system path to the assets directory.
    /// </summary>
    [JsonProperty("assetsDirectoryPath"), JsonPropertyName("assetsDirectoryPath")]
    public string AssetsDirectoryPath { get; set; }
    
    /// <summary>
    /// Gets or sets the file system path to the cache directory.
    /// </summary>
    [JsonProperty("cacheDirectoryPath"), JsonPropertyName("cacheDirectoryPath")]
    public string CacheDirectoryPath { get; set; }
    
    /// <summary>
    /// Gets or sets the file system path to the icons directory.
    /// </summary>
    [JsonProperty("iconsDirectoryPath"), JsonPropertyName("iconsDirectoryPath")]
    public string IconsDirectoryPath { get; set; }
    
    /// <summary>
    /// Gets or sets the file system path to the instances directory.
    /// </summary>
    [JsonProperty("instancesDirectoryPath"), JsonPropertyName("instancesDirectoryPath")]
    public string InstancesDirectoryPath { get; set; }
    
    /// <summary>
    /// Gets or sets the file system path to the Java directory.
    /// </summary>
    [JsonProperty("javaDirectoryPath"), JsonPropertyName("javaDirectoryPath")]
    public string JavaDirectoryPath { get; set; }
    
    /// <summary>
    /// Gets or sets the file system path to the libraries directory.
    /// </summary>
    [JsonProperty("librariesDirectoryPath"), JsonPropertyName("librariesDirectoryPath")]
    public string LibrariesDirectoryPath { get; set; }
    
    /// <summary>
    /// Gets or sets the file system path to the manifests directory.
    /// </summary>
    [JsonProperty("manifestsDirectoryPath"), JsonPropertyName("manifestsDirectoryPath")]
    public string ManifestsDirectoryPath { get; set; }
    
    /// <summary>
    /// Gets or sets the file system path to the translations directory.
    /// </summary>
    [JsonProperty("translationsDirectoryPath"), JsonPropertyName("translationsDirectoryPath")]
    public string TranslationsDirectoryPath { get; set; }
    
    /// <summary>
    /// Gets or sets the file system path to the versions directory.
    /// </summary>
    [JsonProperty("versionsDirectoryPath"), JsonPropertyName("versionsDirectoryPath")]
    public string VersionsDirectoryPath { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LauncherConfig"/> class with default values.
    /// </summary>
    public LauncherConfig()
    {
        EnableAutomaticUpdates = true;
        UpdateInterval = 24; // Default to 24 hours
        NextUpdateCheck = DateTime.MinValue; // Default to never checked
        Language = "en"; // Default language
        Theme = EThemeType.Dark; // Default theme type
        AssetsDirectoryPath = Path.Combine(PathHelper.ApplicationDir, "assets");
        CacheDirectoryPath = Path.Combine(PathHelper.ApplicationDir, "cache");
        IconsDirectoryPath = Path.Combine(PathHelper.ApplicationDir, "icons");
        InstancesDirectoryPath = Path.Combine(PathHelper.ApplicationDir, "instances");
        JavaDirectoryPath = Path.Combine(PathHelper.ApplicationDir, "java");
        LibrariesDirectoryPath = Path.Combine(PathHelper.ApplicationDir, "libraries");
        ManifestsDirectoryPath = Path.Combine(PathHelper.ApplicationDir, "manifests");
        TranslationsDirectoryPath = Path.Combine(PathHelper.ApplicationDir, "translations");
        VersionsDirectoryPath = Path.Combine(PathHelper.ApplicationDir, "versions");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LauncherConfig"/> class with specified values.
    /// </summary>
    /// <param name="enableAutomaticUpdates">Indicates whether automatic updates are enabled.</param>
    /// <param name="updateInterval">The update interval in hours.</param>
    /// <param name="nextUpdateCheck">The date and time for the next update check.</param>
    /// <param name="language">The language of the launcher.</param>
    /// <param name="theme">The theme of the launcher.</param>
    /// <param name="assetsDirectoryPath">The file system path to the assets directory.</param>
    /// <param name="cacheDirectoryPath">The file system path to the cache directory.</param>
    /// <param name="iconsDirectoryPath">The file system path to the icons directory.</param>
    /// <param name="instancesDirectoryPath">The file system path to the instances directory.</param>
    /// <param name="javaDirectoryPath">The file system path to the Java directory.</param>
    /// <param name="librariesDirectoryPath">The file system path to the libraries directory.</param>
    /// <param name="manifestsDirectoryPath">The file system path to the manifests directory.</param>
    /// <param name="translationsDirectoryPath">The file system path to the translations directory.</param>
    /// <param name="versionsDirectoryPath">The file system path to the versions directory.</param>
    public LauncherConfig(bool enableAutomaticUpdates, uint updateInterval, DateTime nextUpdateCheck, string language, EThemeType theme, string assetsDirectoryPath, string cacheDirectoryPath, string iconsDirectoryPath, string instancesDirectoryPath, string javaDirectoryPath, string librariesDirectoryPath, string manifestsDirectoryPath, string translationsDirectoryPath, string versionsDirectoryPath)
    {
        EnableAutomaticUpdates = enableAutomaticUpdates;
        UpdateInterval = updateInterval;
        NextUpdateCheck = nextUpdateCheck;
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
    /// Gets the file system path to the vanilla manifest file.
    /// </summary>
    /// <returns>The path to the vanilla manifest file.</returns>
    public string GetVanillaManifestPath()
    {
        return Path.Combine(ManifestsDirectoryPath, "vanillaManifest.json");
    }
    
    /// <summary>
    /// Gets the file system path to the Forge manifest file.
    /// </summary>
    /// <returns>The path to the Forge manifest file.</returns>
    public string GetForgeManifestPath()
    {
        return Path.Combine(ManifestsDirectoryPath, "forgeManifest.json");
    }
    
    /// <summary>
    /// Gets the file system path to the NeoForge manifest file.
    /// </summary>
    /// <returns>The path to the NeoForge manifest file.</returns>
    public string GetNeoForgeManifestPath()
    {
        return Path.Combine(ManifestsDirectoryPath, "neoForgeManifest.json");
    }
    
    /// <summary>
    /// Gets the file system path to the Fabric manifest file.
    /// </summary>
    /// <returns>The path to the Fabric manifest file.</returns>
    public string GetFabricManifestPath()
    {
        return Path.Combine(ManifestsDirectoryPath, "fabricManifest.json");
    }
    
    /// <summary>
    /// Gets the file system path to the Quilt manifest file.
    /// </summary>
    /// <returns>The path to the Quilt manifest file.</returns>
    public string GetQuiltManifestPath()
    {
        return Path.Combine(ManifestsDirectoryPath, "quiltManifest.json");
    }
}