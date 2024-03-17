using System.Text.Json.Serialization;

namespace KonkordLauncher.API.Models
{
    [Serializable]
    public class Resolution
    {
        [JsonPropertyName("x")]
        public int X {  get; set; }
        [JsonPropertyName("y")]
        public int Y { get; set; }
        [JsonPropertyName("isFullScreen")]
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
