using KonkordLibrary.Models.Forge;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace KonkordLibrary.Helpers
{
    public static class Mapper
    {
        private static readonly Regex argBracket = new Regex("\\$?\\{(.*?)}");

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
