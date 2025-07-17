using System.Text.RegularExpressions;
using Tavstal.KonkordLauncher.Core.Models.Forge;

namespace Tavstal.KonkordLauncher.Core.Helpers;

/// <summary>
/// Provides methods for mapping and interpolation.
/// Source: https://github.com/CmlLib/CmlLib.Core.Installer.Forge
/// </summary>
public static class Mapper
{
    /// <summary>
    /// A regular expression used to match arguments enclosed in curly brackets.
    /// </summary>
    private static readonly Regex argBracket = new("\\$?\\{(.*?)}");

    /// <summary>
    /// Maps an array of arguments by interpolating values from a dictionary and optionally converting paths to full paths.
    /// </summary>
    /// <param name="args">The array of arguments to map.</param>
    /// <param name="dictionaries">A dictionary containing key-value pairs for interpolation.</param>
    /// <param name="prePath">The base path to prepend to arguments if applicable.</param>
    /// <returns>An array of mapped arguments.</returns>
    public static string[] Map(string[] args, Dictionary<string, string?> dictionaries, string prePath)
    {
        bool flag = !string.IsNullOrEmpty(prePath);
        List<string> list = new List<string>(args.Length);
        foreach (var arg in args)
        {
            string text = Interpolation(arg, dictionaries, handleEmpty: false);
            if (flag)
                text = ToFullPath(text, prePath);
            list.Add(HandleEmptyArg(text));
        }

        return list.ToArray();
    }

    /// <summary>
    /// Interpolates placeholders in a string using values from a dictionary.
    /// </summary>
    /// <param name="str">The string containing placeholders to interpolate.</param>
    /// <param name="dictionaries">A dictionary containing key-value pairs for interpolation.</param>
    /// <param name="handleEmpty">Indicates whether to handle empty arguments.</param>
    /// <returns>The interpolated string.</returns>
    public static string Interpolation(string str, Dictionary<string, string?> dictionaries, bool handleEmpty)
    {
        Dictionary<string, string?> dicts2 = dictionaries;
        str = argBracket.Replace(str, delegate(Match match)
        {
            if (match.Groups.Count < 2)
                return match.Value;

            string value = match.Groups[1].Value;
            return dicts2.TryGetValue(value, out var value2) ? value2 ?? "" : match.Value;
        });
        return handleEmpty ? HandleEmptyArg(str) : str;
    }

    /// <summary>
    /// Converts a string to a full path based on a base path if applicable.
    /// </summary>
    /// <param name="str">The string to convert to a full path.</param>
    /// <param name="prePath">The base path to prepend to the string.</param>
    /// <returns>The full path string.</returns>
    public static string ToFullPath(string str, string prePath)
    {
        if (str.StartsWith("[") && str.EndsWith("]") && !string.IsNullOrEmpty(prePath))
        {
            string[] array = str.TrimStart('[').TrimEnd(']').Split('@');
            string name = array[0];
            string extension = "jar";
            if (array.Length > 1)
                extension = array[1];

            return Path.Combine(prePath, PackageName.Parse(name).GetPath(null, extension));
        }

        if (str.StartsWith("'") && str.EndsWith("'"))
            return str.Trim('\'');

        return str;
    }

    /// <summary>
    /// Handles empty arguments by enclosing them in quotes if necessary.
    /// </summary>
    /// <param name="input">The input string to process.</param>
    /// <returns>The processed string with empty arguments handled.</returns>
    public static string HandleEmptyArg(string input)
    {
        if (input.Contains('='))
        {
            string[] array = input.Split('=');
            if (array[1].Contains(' ') && !checkEmptyHandled(array[1]))
                return array[0] + "=\"" + array[1] + "\"";

            return input;
        }

        if (input.Contains(' ') && !checkEmptyHandled(input))
            return "\"" + input + "\"";

        return input;
    }

    /// <summary>
    /// Checks whether a string is already enclosed in quotes.
    /// </summary>
    /// <param name="str">The string to check.</param>
    /// <returns>True if the string is enclosed in quotes, otherwise false.</returns>
    private static bool checkEmptyHandled(string str)
    {
        return str.StartsWith("\"") || str.EndsWith("\"");
    }
}