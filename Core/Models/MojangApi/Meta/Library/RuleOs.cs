using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta.Library;

/// <summary>
/// Represents the operating system specification for a rule.
/// </summary>
public class RuleOs
{
    /// <summary>
    /// Gets or sets the name of the operating system.
    /// </summary>
    [JsonPropertyName("name"), JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RuleOs"/> class.
    /// </summary>
    public RuleOs() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RuleOs"/> class with a specified operating system name.
    /// </summary>
    /// <param name="name">The name of the operating system.</param>
    public RuleOs(string name)
    {
        Name = name;
    }
}