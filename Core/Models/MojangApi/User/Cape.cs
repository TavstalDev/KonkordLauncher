
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.User;

/// <summary>
/// Represents a Mojang cape entry, including its identifier, current state, texture URL,
/// and optional alias.
/// </summary>
public class Cape
{
    /// <summary>
    /// Gets or sets the unique identifier of the cape.
    /// </summary>
    [JsonProperty("id")]
    public string Id {  get; set; }

    /// <summary>
    /// Gets or sets the current state of the cape.
    /// </summary>
    [JsonProperty("state")]
    public string State {  get; set; }

    /// <summary>
    /// Gets or sets the URL where the cape texture can be downloaded.
    /// </summary>
    [JsonProperty("url")]
    public string Url {  get; set; }

    /// <summary>
    /// Gets or sets the optional alias of the cape.
    /// </summary>
    [JsonProperty("alias")]
    public string Alias {  get; set; }

    /// <summary>
    /// Initializes a new, empty instance of the <see cref="Cape"/> class.
    /// </summary>
    public Cape() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Cape"/> class with the specified values.
    /// </summary>
    /// <param name="id">The cape identifier.</param>
    /// <param name="state">The current state of the cape.</param>
    /// <param name="url">The URL of the cape texture.</param>
    /// <param name="alias">The optional alias of the cape.</param>
    public Cape(string id, string state, string url, string alias)
    {
        Id = id;
        State = state;
        Url = url;
        Alias = alias;
    }
}