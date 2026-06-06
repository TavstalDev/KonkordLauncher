
using System.Text.Json.Serialization;


namespace Tavstal.KonkordLauncher.Core.Models.MojangApi;

/// <summary>
/// Represents a Minecraft version with its associated metadata.
/// </summary>
public class MinecraftVersion
{
    /// <summary>
    /// Gets or sets the unique identifier of the Minecraft version.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the type of the Minecraft version (e.g., "release", "snapshot").
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the URL for the version's metadata or resources.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets the time the version was created or last updated.
    /// </summary>
    [JsonPropertyName("time")]
    public DateTime Time { get; set; }

    /// <summary>
    /// Gets or sets the release time of the Minecraft version.
    /// </summary>
    [JsonPropertyName("releaseTime")]
    public DateTime ReleaseTime { get; set; }

    /// <summary>
    /// Gets or sets the .NET version associated with the Minecraft version.
    /// This property is ignored during JSON serialization and deserialization.
    /// </summary>
    [JsonIgnore]
    private Version? _version { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MinecraftVersion"/> class.
    /// </summary>
    public MinecraftVersion() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinecraftVersion"/> class with the specified parameters.
    /// </summary>
    /// <param name="id">The unique identifier of the Minecraft version.</param>
    /// <param name="type">The type of the Minecraft version.</param>
    /// <param name="url">The URL for the version's metadata or resources.</param>
    /// <param name="time">The time the version was created or last updated.</param>
    /// <param name="releaseTime">The release time of the Minecraft version.</param>
    public MinecraftVersion(string id, string type, string url, DateTime time, DateTime releaseTime)
    {
        Id = id;
        _version = new Version(id);
        Type = type;
        Url = url;
        Time = time;
        ReleaseTime = releaseTime;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified object is equal to the current instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is MinecraftVersion other)
        {
            return Id == other.Id;
        }
        return false;
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return Id.GetHashCode();
    }
}