
using System.Text.Json.Serialization;
using NbtLib;


namespace Tavstal.KonkordLauncher.Core.Models.MojangApi;

/// <summary>
/// Represents a Minecraft level, containing data and optional data packs.
/// </summary>
public class Level
{
    /// <summary>
    /// Gets or sets the level data, which includes various properties of the Minecraft level.
    /// </summary>
    [NbtProperty(PropertyName = "Data")]
    [JsonPropertyName("data")]
    public LevelData Data { get; set; }

    /// <summary>
    /// Gets or sets the data packs associated with the level.
    /// This property is currently unused but may be utilized in the future.
    /// </summary>
    [NbtProperty(PropertyName = "DataPacks")]
    [JsonPropertyName("dataPacks")]
    public object? DataPacks { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Level"/> class with default values.
    /// </summary>
    public Level()
    {
        Data = new LevelData();
        DataPacks = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Level"/> class with the specified data and data packs.
    /// </summary>
    /// <param name="data">The level data to initialize with.</param>
    /// <param name="dataPacks">The data packs to initialize with, or null if not provided.</param>
    public Level(LevelData data, object? dataPacks = null)
    {
        Data = data;
        DataPacks = dataPacks;
    }
}