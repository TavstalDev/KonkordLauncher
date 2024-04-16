using Tavstal.KonkordLibrary.Models.Forge;
using System.IO;
using System.Text.RegularExpressions;

namespace Tavstal.KonkordLibrary.Helpers
{
    /// <summary>
    /// Provides methods for mapping and interpolation.
    /// Source: https://github.com/CmlLib/CmlLib.Core.Installer.Forge
    /// </summary>
    public static class Mapper
    {
        private static readonly Regex argBracket = new Regex("\\$?\\{(.*?)}");

        /// <summary>
        /// Maps the specified arguments using the provided dictionaries and prepends a directory path to them.
        /// </summary>
        /// <param name="arg">The arguments to map.</param>
        /// <param name="dicts">The dictionaries used for mapping.</param>
        /// <param name="prepath">The directory path to prepend to the arguments.</param>
        /// <returns>
        /// An array of mapped arguments.
        /// </returns>
        public static string[] Map(string[] arg, Dictionary<string, string?> dicts, string prepath)
        {
            bool flag = !string.IsNullOrEmpty(prepath);
            List<string> list = new List<string>(arg.Length);
            for (int i = 0; i < arg.Length; i++)
            {
                string text = Interpolation(arg[i], dicts, handleEmpty: false);
                if (flag)
                {
                    text = ToFullPath(text, prepath);
                }

                list.Add(HandleEmptyArg(text));
            }

            return list.ToArray();
        }

        /// <summary>
        /// Interpolates the specified string using the provided dictionaries.
        /// </summary>
        /// <param name="str">The string to interpolate.</param>
        /// <param name="dicts">The dictionaries used for interpolation.</param>
        /// <param name="handleEmpty">A flag indicating whether to handle empty values.</param>
        /// <returns>
        /// The interpolated string.
        /// </returns>
        public static string Interpolation(string str, Dictionary<string, string?> dicts, bool handleEmpty)
        {
            Dictionary<string, string?> dicts2 = dicts;
            str = argBracket.Replace(str, delegate (Match match)
            {
                if (match.Groups.Count < 2)
                {
                    return match.Value;
                }

                string value = match.Groups[1].Value;
                string value2;
                return dicts2.TryGetValue(value, out value2) ? ((value2 == null) ? "" : value2) : match.Value;
            });
            if (handleEmpty)
            {
                return HandleEmptyArg(str);
            }

            return str;
        }

        /// <summary>
        /// Converts the specified string to a full path by prepending the specified directory path.
        /// </summary>
        /// <param name="str">The string to convert to a full path.</param>
        /// <param name="prepath">The directory path to prepend.</param>
        /// <returns>
        /// The full path.
        /// </returns>
        public static string ToFullPath(string str, string prepath)
        {
            if (str.StartsWith("[") && str.EndsWith("]") && !string.IsNullOrEmpty(prepath))
            {
                string[] array = str.TrimStart('[').TrimEnd(']').Split('@');
                string name = array[0];
                string extension = "jar";
                if (array.Length > 1)
                {
                    extension = array[1];
                }

                return Path.Combine(prepath, PackageName.Parse(name).GetPath(null, extension));
            }

            if (str.StartsWith("'") && str.EndsWith("'"))
            {
                return str.Trim('\'');
            }

            return str;
        }

        /// <summary>
        /// Handles an empty argument by replacing it with a placeholder.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>
        /// The input string if it is not empty; otherwise, a placeholder string.
        /// </returns>
        public static string HandleEmptyArg(string input)
        {
            if (input.Contains("="))
            {
                string[] array = input.Split('=');
                if (array[1].Contains(" ") && !checkEmptyHandled(array[1]))
                {
                    return array[0] + "=\"" + array[1] + "\"";
                }

                return input;
            }

            if (input.Contains(" ") && !checkEmptyHandled(input))
            {
                return "\"" + input + "\"";
            }

            return input;
        }

        /// <summary>
        /// Checks if empty handling has been applied to the specified string.
        /// </summary>
        /// <param name="str">The string to check.</param>
        /// <returns>
        /// True if empty handling has been applied; otherwise, false.
        /// </returns>
        private static bool checkEmptyHandled(string str)
        {
            if (!str.StartsWith("\""))
            {
                return str.EndsWith("\"");
            }

            return true;
        }
    }
}
