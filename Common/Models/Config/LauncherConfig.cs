using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Helpers;

namespace Tavstal.KonkordLauncher.Common.Models.Config;

public class LauncherConfig
{
    [JsonProperty("enableAutomaticUpdates"), JsonPropertyName("enableAutomaticUpdates")]
    public bool EnableAutomaticUpdates { get; set; }
    
    [JsonProperty("updateInterval"), JsonPropertyName("updateInterval")]
    public uint UpdateInterval { get; set; }

    [JsonProperty("nextUpdateCheck"), JsonPropertyName("nextUpdateCheck")]
    public DateTime NextUpdateCheck { get; set; }
    
    [JsonProperty("language"), JsonPropertyName("language")]
    public string Language { get; set; }
    
    [JsonProperty("theme"), JsonPropertyName("theme")]
    public EThemeType Theme { get; set; }
    
    [JsonProperty("assetsDirectoryPath"), JsonPropertyName("assetsDirectoryPath")]
    public string AssetsDirectoryPath { get; set; }
    
    [JsonProperty("cacheDirectoryPath"), JsonPropertyName("cacheDirectoryPath")]
    public string CacheDirectoryPath { get; set; }
    
    [JsonProperty("iconsDirectoryPath"), JsonPropertyName("iconsDirectoryPath")]
    public string IconsDirectoryPath { get; set; }
    
    [JsonProperty("instancesDirectoryPath"), JsonPropertyName("instancesDirectoryPath")]
    public string InstancesDirectoryPath { get; set; }
    
    [JsonProperty("librariesDirectoryPath"), JsonPropertyName("librariesDirectoryPath")]
    public string LibrariesDirectoryPath { get; set; }
    
    [JsonProperty("manifestsDirectoryPath"), JsonPropertyName("manifestsDirectoryPath")]
    public string ManifestsDirectoryPath { get; set; }
    
    [JsonProperty("translationsDirectoryPath"), JsonPropertyName("translationsDirectoryPath")]
    public string TranslationsDirectoryPath { get; set; }
    
    [JsonProperty("versionsDirectoryPath"), JsonPropertyName("versionsDirectoryPath")]
    public string VersionsDirectoryPath { get; set; }

    public LauncherConfig()
    {
        EnableAutomaticUpdates = true;
        UpdateInterval = 24; // Default to 24 hours
        NextUpdateCheck = DateTime.MinValue; // Default to never checked
        Language = "en"; // Default language
        Theme = EThemeType.Automatic; // Default theme type
        AssetsDirectoryPath = Path.Combine(PathHelper.ApplicationDir, "assets");
        CacheDirectoryPath = Path.Combine(PathHelper.ApplicationDir, "cache");
        IconsDirectoryPath = Path.Combine(PathHelper.ApplicationDir, "icons");
        InstancesDirectoryPath = Path.Combine(PathHelper.ApplicationDir, "instances");
        LibrariesDirectoryPath = Path.Combine(PathHelper.ApplicationDir, "libraries");
        ManifestsDirectoryPath = Path.Combine(PathHelper.ApplicationDir, "manifests");
        TranslationsDirectoryPath = Path.Combine(PathHelper.ApplicationDir, "translations");
        VersionsDirectoryPath = Path.Combine(PathHelper.ApplicationDir, "versions");
    }

    public LauncherConfig(bool enableAutomaticUpdates, uint updateInterval, DateTime nextUpdateCheck, string language, EThemeType theme, string assetsDirectoryPath, string cacheDirectoryPath, string iconsDirectoryPath, string instancesDirectoryPath, string librariesDirectoryPath, string manifestsDirectoryPath, string translationsDirectoryPath, string versionsDirectoryPath)
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
        LibrariesDirectoryPath = librariesDirectoryPath;
        ManifestsDirectoryPath = manifestsDirectoryPath;
        TranslationsDirectoryPath = translationsDirectoryPath;
        VersionsDirectoryPath = versionsDirectoryPath;
    }
    
    public string GetVanillaManifestPath()
    {
        return Path.Combine(ManifestsDirectoryPath, "vanillaManifest.json");
    }
    
    public string GetForgeManifestPath()
    {
        return Path.Combine(ManifestsDirectoryPath, "forgeManifest.json");
    }
    
    public string GetNeoForgeManifestPath()
    {
        return Path.Combine(ManifestsDirectoryPath, "neoForgeManifest.json");
    }
    
    public string GetFabricManifestPath()
    {
        return Path.Combine(ManifestsDirectoryPath, "fabricManifest.json");
    }
    
    public string GetQuiltManifestPath()
    {
        return Path.Combine(ManifestsDirectoryPath, "quiltManifest.json");
    }
}