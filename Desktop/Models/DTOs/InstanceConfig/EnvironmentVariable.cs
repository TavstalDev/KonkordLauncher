using System.Text.Json.Serialization;

namespace Tavstal.KonkordLauncher.Desktop.Models.DTOs.InstanceConfig;

/// <summary>
///     Holds an environment variable with a key and a value.
/// </summary>
public class EnvironmentVariable
{
    /// <summary>
    ///     Creates a new environment variable.
    /// </summary>
    /// <param name="key">The key of the variable.</param>
    /// <param name="value">The value of the variable.</param>
    public EnvironmentVariable(string key, string value)
    {
        Key = key;
        Value = value;
    }

    /// <summary>
    ///     Gets or sets the key of the variable.
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; }

    /// <summary>
    ///     Gets or sets the value of the variable.
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; }
}
