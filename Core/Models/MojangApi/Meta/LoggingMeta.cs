using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;

/// <summary>
/// Represents metadata for logging configuration in Minecraft.
/// </summary>
public class LoggingMeta
{
    /// <summary>
    /// Gets or sets the logging client configuration.
    /// </summary>
    [JsonPropertyName("client"), JsonProperty("client")]
    public LoggingClient Client { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingMeta"/> class.
    /// </summary>
    public LoggingMeta()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingMeta"/> class with a specified logging client configuration.
    /// </summary>
    /// <param name="client">The logging client configuration.</param>
    public LoggingMeta(LoggingClient client)
    {
        Client = client;
    }
}