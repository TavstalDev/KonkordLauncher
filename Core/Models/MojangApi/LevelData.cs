
using System.Text.Json.Serialization;
using NbtLib;


namespace Tavstal.KonkordLauncher.Core.Models.MojangApi;

/// <summary>
/// Represents the data associated with a Minecraft level, including game settings and metadata.
/// </summary>
public class LevelData
{
    /// <summary>
    /// Gets or sets the game mode of the level.
    /// </summary>
    [NbtProperty(PropertyName = "GameMode")]
    [JsonPropertyName("gameMode")]
    public int? GameMode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the level is in hardcore mode.
    /// </summary>
    [NbtProperty(PropertyName = "Hardcore")]
    [JsonPropertyName("hardcore")]
    public bool? Hardcore { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the last time the level was played.
    /// </summary>
    [NbtProperty(PropertyName = "LastPlayed")]
    [JsonPropertyName("lastPlayed")]
    public long LastPlayed { get; set; }

    /// <summary>
    /// Gets or sets the name of the level.
    /// </summary>
    [NbtProperty(PropertyName = "LevelName")]
    [JsonPropertyName("levelName")]
    public string? LevelName { get; set; }

    /// <summary>
    /// Gets or sets the random seed used to generate the level.
    /// </summary>
    [NbtProperty(PropertyName = "RandomSeed")]
    [JsonPropertyName("randomSeed")]
    public long? RandomSeed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the level's size on disk is tracked.
    /// </summary>
    [NbtProperty(PropertyName = "SizeOnDisk")]
    [JsonPropertyName("sizeOnDisk")]
    public bool SizeOnDisk { get; set; }

    /// <summary>
    /// Gets or sets the difficulty level of the game.
    /// </summary>
    [NbtProperty(PropertyName = "Difficulty")]
    [JsonPropertyName("difficulty")]
    public byte Difficulty { get; set; }

    /// <summary>
    /// Gets or sets the game type of the level.
    /// </summary>
    [NbtProperty(PropertyName = "GameType")]
    [JsonPropertyName("gameType")]
    public int? GameType { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LevelData"/> class with default values.
    /// </summary>
    public LevelData()
    {
        Difficulty = 0;
        GameType = null;
        GameMode = null;
        Hardcore = null;
        LastPlayed = 0;
        LevelName = null;
        RandomSeed = null;
        SizeOnDisk = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LevelData"/> class with the specified values.
    /// </summary>
    /// <param name="difficulty">The difficulty level of the game.</param>
    /// <param name="gameType">The game type of the level.</param>
    /// <param name="gameMode">The game mode of the level.</param>
    /// <param name="hardcore">Indicates whether the level is in hardcore mode.</param>
    /// <param name="lastPlayed">The timestamp of the last time the level was played.</param>
    /// <param name="levelName">The name of the level.</param>
    /// <param name="randomSeed">The random seed used to generate the level.</param>
    /// <param name="sizeOnDisk">Indicates whether the level's size on disk is tracked.</param>
    public LevelData(byte difficulty, int? gameType, int? gameMode, bool? hardcore, long lastPlayed, string? levelName, long? randomSeed, bool sizeOnDisk)
    {
        Difficulty = difficulty;
        GameType = gameType;
        GameMode = gameMode;
        Hardcore = hardcore;
        LastPlayed = lastPlayed;
        LevelName = levelName;
        RandomSeed = randomSeed;
        SizeOnDisk = sizeOnDisk;
    }
}