using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLibrary.Models.Launcher
{
    [Serializable]
    public class Resolution
    {
        [JsonPropertyName("x"), JsonProperty("x")]
        public int X { get; set; }
        [JsonPropertyName("y"), JsonProperty("y")]
        public int Y { get; set; }
        [JsonPropertyName("isFullScreen"), JsonProperty("isFullScreen")]
        public bool IsFullScreen { get; set; }

        public Resolution() { }
        public Resolution(int x, int y, bool isFullScreen)
        {
            X = x;
            Y = y;
            IsFullScreen = isFullScreen;
        }
    }
}
