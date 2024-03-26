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
    }
}
