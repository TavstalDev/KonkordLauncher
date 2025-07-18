using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;

/// <summary>
/// Represents metadata for game and JVM arguments used in Minecraft.
/// </summary>
public class ArgumentMeta
{
    /// <summary>
    /// Gets or sets the list of game arguments.
    /// </summary>
    [JsonPropertyName("game"), JsonProperty("game")]
    public List<object> Game { get; set; }

    /// <summary>
    /// Gets or sets the list of JVM arguments.
    /// </summary>
    [JsonPropertyName("jvm"), JsonProperty("jvm")]
    public List<object> Jvm { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentMeta"/> class.
    /// </summary>
    public ArgumentMeta() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentMeta"/> class with specified game and JVM arguments.
    /// </summary>
    /// <param name="game">The list of game arguments.</param>
    /// <param name="jvm">The list of JVM arguments.</param>
    public ArgumentMeta(List<object> game, List<object> jvm)
    {
        Game = game;
        Jvm = jvm;
    }

    /// <summary>
    /// Retrieves the game arguments as a list of strings.
    /// </summary>
    /// <returns>A list of game arguments.</returns>
    public List<string> GetGameArgs()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Game == null)
            return new List<string>();

        List<string> local = new List<string>();
        foreach (var item in Game)
        {
            if (item is string s)
                local.Add(s);
        }

        List<string> result = new List<string>();

        for (int i = 0; i < local.Count; i += 2)
        {
            result.Add(local[i] + " " + local[i + 1]);
        }

        return result;
    }

    /// <summary>
    /// Retrieves the game arguments as a single concatenated string.
    /// </summary>
    /// <returns>A string containing the game arguments.</returns>
    public string GetGameArgString()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Game == null)
            return string.Empty;

        List<string> local = new List<string>();
        foreach (var item in Game)
        {
            if (item is string s)
                local.Add(s);
        }

        string result = string.Empty;
        for (int i = 0; i < local.Count; i += 2)
        {
            result += $"{local[i]} {local[i + 1]} ";
        }

        return result;
    }

    /// <summary>
    /// Retrieves the JVM arguments as a list of strings.
    /// </summary>
    /// <returns>A list of JVM arguments.</returns>
    public List<string> GetJvmArgs()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Jvm == null)
            return new List<string>();

        List<string> local = new List<string>();
        foreach (var item in Jvm)
        {
            if (item is string s)
            {
                /*if (s.StartsWith("-cp"))
                    continue;
                if (s == "${classpath}")
                    continue;*/
                local.Add(s);
            }
        }

        return local;
    }

    /// <summary>
    /// Retrieves the JVM arguments as a single concatenated string.
    /// </summary>
    /// <returns>A string containing the JVM arguments.</returns>
    public string GetJvmArgString()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Jvm == null)
            return string.Empty;

        string local = string.Empty;
        foreach (var item in Jvm)
        {
            if (item is string s)
            {
                /*if (s.StartsWith("-cp"))
                    continue;
                if (s == "${classpath}")
                    continue;*/
                local += $"{s} ";
            }
        }

        return local;
    }
}