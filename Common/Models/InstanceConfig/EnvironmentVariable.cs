
using System.Text.Json.Serialization;


namespace Tavstal.KonkordLauncher.Common.Models.InstanceConfig;

/// <summary>
/// Represents an environment variable with a key-value pair.
/// </summary>
public class EnvironmentVariable
{
    /// <summary>
    /// Gets or sets the key of the environment variable.
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; }

    /// <summary>
    /// Gets or sets the value of the environment variable.
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentVariable"/> class
    /// with the specified key and value.
    /// </summary>
    /// <param name="key">The key of the environment variable.</param>
    /// <param name="value">The value of the environment variable.</param>
    public EnvironmentVariable(string key, string value)
    {
        Key = key;
        Value = value;
    }
}