using KonkordLibrary.Models.Minecraft.Library;
using System.Text.Json.Serialization;

namespace KonkordLibrary.Models.Minecraft.Meta
{
    public class MCMetaArgument
    {
        [JsonPropertyName("game")]
        public List<object> Game {  get; set; }
        [JsonPropertyName("jvm")]
        public List<object> JVM { get; set; }

        public MCMetaArgument() { }

        public MCMetaArgument(List<object> game, List<object> jVM)
        {
            Game = game;
            JVM = jVM;
        }

        public List<string> GetGameArgs()
        {
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

        public string GetGameArgString()
        {
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

        public List<string> GetJVMArgs()
        {
            List<string> local = new List<string>();
            foreach (var item in JVM)
            {
                if (item is string s)
                {
                    if (s.StartsWith("-cp"))
                        continue;
                    if (s == "${classpath}")
                        continue;
                    local.Add(s);
                }
            }

            return local;
        }

        public string GetJVMArgString()
        {
            string local = string.Empty;
            foreach (var item in JVM)
            {
                if (item is string s)
                {
                    if (s.StartsWith("-cp"))
                        continue;
                    if (s == "${classpath}")
                        continue;
                    local += $"{s} ";
                }
            }

            return local;
        }
    }
}
