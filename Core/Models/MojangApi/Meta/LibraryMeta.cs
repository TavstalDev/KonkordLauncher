using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta.Library;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;

/// <summary>
/// Represents metadata for a library, including its name, downloads, rules, and native configurations.
/// </summary>
public class LibraryMeta
{
    /// <summary>
    /// Gets or sets the name of the library.
    /// </summary>
    [JsonPropertyName("name"), JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the download information for the library.
    /// </summary>
    [JsonPropertyName("downloads"), JsonProperty("downloads")]
    public LibraryDownloads Downloads { get; set; }

    /// <summary>
    /// Gets or sets the rules that determine whether the library is allowed or disallowed.
    /// </summary>
    [JsonPropertyName("rules"), JsonProperty("rules")]
    public List<Rule> Rules { get; set; }

    /// <summary>
    /// Gets or sets the native configurations for the library, if applicable.
    /// </summary>
    [JsonPropertyName("natives"), JsonProperty("natives")]
    public Natives? Natives { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryMeta"/> class.
    /// </summary>
    public LibraryMeta()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryMeta"/> class with specified name, downloads, and rules.
    /// </summary>
    /// <param name="name">The name of the library.</param>
    /// <param name="downloads">The download information for the library.</param>
    /// <param name="rules">The rules that determine whether the library is allowed or disallowed.</param>
    public LibraryMeta(string name, LibraryDownloads downloads, List<Rule> rules)
    {
        Name = name;
        Downloads = downloads;
        Rules = rules;
    }

    /// <summary>
    /// Evaluates the rules to determine if the library is allowed based on the current operating system.
    /// </summary>
    /// <returns><c>true</c> if the library is allowed; otherwise, <c>false</c>.</returns>
    public bool GetRulesResult()
    {
        bool localResult = false;

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Rules == null)
            return true;

        if (Rules.Count == 0)
            return true;

        var operatingSystem = OSHelper.GetOperatingSystem();
        foreach (Rule rule in Rules)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (rule.Os == null)
            {
                localResult = rule.Action == "allow";
                continue;
            }
            
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (rule.Os.Name == null)
            {
                localResult = rule.Action == "allow";
                continue;
            }
            
            if (rule.Os.Name == "x86" && !OSHelper.Is64BitOperatingSystem())
            {
                localResult = rule.Action == "allow";
                continue;
            }

            if (rule.Os.Name == "windows" && operatingSystem == EOperatingSystem.Windows)
            {
                localResult = rule.Action == "allow";
                continue;
            }

            if (rule.Os.Name == "linux" && operatingSystem == EOperatingSystem.Linux)
            {
                localResult = rule.Action == "allow";
                continue;
            }

            if (rule.Os.Name.StartsWith("osx") && operatingSystem == EOperatingSystem.MacOS)
                 localResult = rule.Action == "allow";
        }

        return localResult;
    }
}