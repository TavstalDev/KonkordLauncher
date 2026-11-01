using System;
using System.IO;
using System.Text.Json.Serialization;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;

namespace Tavstal.KonkordLauncher.Desktop.Models.DTOs.Config;

/// <summary>
///     Holds the launcher settings, such as updates, language, theme, and folder paths.
/// </summary>
public class LauncherConfig
{
    /// <summary>
    ///     Creates a new instance with default values.
    /// </summary>
    public LauncherConfig()
    {
        EnableAutomaticUpdates = true;
        UpdateInterval = 24; // Default to 24 hours
        NextUpdateCheck = DateTime.MinValue; // Default to never checked
        Language = "en"; // Default language
        Theme = EThemeType.DARK; // Default theme type
        var workingDir = Directory.GetCurrentDirectory();
        AssetsDirectoryPath = Path.Combine(workingDir, "assets");
        CacheDirectoryPath = Path.Combine(workingDir, "cache");
        IconsDirectoryPath = Path.Combine(workingDir, "icons");
        InstancesDirectoryPath = Path.Combine(workingDir, "instances");
        JavaDirectoryPath = Path.Combine(workingDir, "java");
        LibrariesDirectoryPath = Path.Combine(workingDir, "libraries");
        ManifestsDirectoryPath = Path.Combine(workingDir, "manifests");
        TranslationsDirectoryPath = Path.Combine(workingDir, "translations");
        VersionsDirectoryPath = Path.Combine(workingDir, "versions");
    }

    /// <summary>
    ///     Creates a new instance with the given values.
    /// </summary>
    /// <param name="enableAutomaticUpdates">Whether automatic updates are enabled.</param>
    /// <param name="updateInterval">The update interval in hours.</param>
    /// <param name="nextUpdateCheck">The date and time of the next update check.</param>
    /// <param name="language">The language code.</param>
    /// <param name="theme">The theme.</param>
    /// <param name="assetsDirectoryPath">The path to the assets folder.</param>
    /// <param name="cacheDirectoryPath">The path to the cache folder.</param>
    /// <param name="iconsDirectoryPath">The path to the icons folder.</param>
    /// <param name="instancesDirectoryPath">The path to the instances folder.</param>
    /// <param name="javaDirectoryPath">The path to the Java folder.</param>
    /// <param name="librariesDirectoryPath">The path to the libraries folder.</param>
    /// <param name="manifestsDirectoryPath">The path to the manifests folder.</param>
    /// <param name="translationsDirectoryPath">The path to the translations folder.</param>
    /// <param name="versionsDirectoryPath">The path to the versions folder.</param>
    public LauncherConfig(bool enableAutomaticUpdates, uint updateInterval, DateTime nextUpdateCheck, string language,
        EThemeType theme, string assetsDirectoryPath, string cacheDirectoryPath, string iconsDirectoryPath,
        string instancesDirectoryPath, string javaDirectoryPath, string librariesDirectoryPath,
        string manifestsDirectoryPath, string translationsDirectoryPath, string versionsDirectoryPath)
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
    ///     Gets or sets a value that indicates whether automatic updates are enabled.
    /// </summary>
    [JsonPropertyName("enableAutomaticUpdates")]
    public bool EnableAutomaticUpdates { get; set; }

    /// <summary>
    ///     Gets or sets the update interval in hours.
    /// </summary>
    [JsonPropertyName("updateInterval")]
    public uint UpdateInterval { get; set; }

    /// <summary>
    ///     Gets or sets the date and time of the next update check.
    /// </summary>
    [JsonPropertyName("nextUpdateCheck")]
    public DateTime NextUpdateCheck { get; set; }

    /// <summary>
    ///     Gets or sets the language code.
    /// </summary>
    [JsonPropertyName("language")]
    public string Language { get; set; }

    /// <summary>
    ///     Gets or sets the theme.
    /// </summary>
    [JsonPropertyName("theme")]
    public EThemeType Theme { get; set; }

    /// <summary>
    ///     Gets or sets the path to the assets folder.
    /// </summary>
    [JsonPropertyName("assetsDirectoryPath")]
    public string AssetsDirectoryPath { get; set; }

    /// <summary>
    ///     Gets or sets the path to the cache folder.
    /// </summary>
    [JsonPropertyName("cacheDirectoryPath")]
    public string CacheDirectoryPath { get; set; }

    /// <summary>
    ///     Gets or sets the path to the icons folder.
    /// </summary>
    [JsonPropertyName("iconsDirectoryPath")]
    public string IconsDirectoryPath { get; set; }

    /// <summary>
    ///     Gets or sets the path to the instances folder.
    /// </summary>
    [JsonPropertyName("instancesDirectoryPath")]
    public string InstancesDirectoryPath { get; set; }

    /// <summary>
    ///     Gets or sets the path to the Java folder.
    /// </summary>
    [JsonPropertyName("javaDirectoryPath")]
    public string JavaDirectoryPath { get; set; }

    /// <summary>
    ///     Gets or sets the path to the libraries folder.
    /// </summary>
    [JsonPropertyName("librariesDirectoryPath")]
    public string LibrariesDirectoryPath { get; set; }

    /// <summary>
    ///     Gets or sets the path to the manifests folder.
    /// </summary>
    [JsonPropertyName("manifestsDirectoryPath")]
    public string ManifestsDirectoryPath { get; set; }

    /// <summary>
    ///     Gets or sets the path to the translations folder.
    /// </summary>
    [JsonPropertyName("translationsDirectoryPath")]
    public string TranslationsDirectoryPath { get; set; }

    /// <summary>
    ///     Gets or sets the path to the versions folder.
    /// </summary>
    [JsonPropertyName("versionsDirectoryPath")]
    public string VersionsDirectoryPath { get; set; }

    /// <summary>
    ///     Gets the path to the vanilla manifest file.
    /// </summary>
    /// <returns>The path to the vanilla manifest file.</returns>
    public string GetVanillaManifestPath()
    {
        return Path.Combine(ManifestsDirectoryPath, "vanillaManifest.json");
    }

    /// <summary>
    ///     Gets the path to the Forge manifest file.
    /// </summary>
    /// <returns>The path to the Forge manifest file.</returns>
    public string GetForgeManifestPath()
    {
        return Path.Combine(ManifestsDirectoryPath, "forgeManifest.json");
    }

    /// <summary>
    ///     Gets the path to the NeoForge manifest file.
    /// </summary>
    /// <returns>The path to the NeoForge manifest file.</returns>
    public string GetNeoForgeManifestPath()
    {
        return Path.Combine(ManifestsDirectoryPath, "neoForgeManifest.json");
    }

    /// <summary>
    ///     Gets the path to the Fabric manifest file.
    /// </summary>
    /// <returns>The path to the Fabric manifest file.</returns>
    public string GetFabricManifestPath()
    {
        return Path.Combine(ManifestsDirectoryPath, "fabricManifest.json");
    }

    /// <summary>
    ///     Gets the path to the Quilt manifest file.
    /// </summary>
    /// <returns>The path to the Quilt manifest file.</returns>
    public string GetQuiltManifestPath()
    {
        return Path.Combine(ManifestsDirectoryPath, "quiltManifest.json");
    }
}
