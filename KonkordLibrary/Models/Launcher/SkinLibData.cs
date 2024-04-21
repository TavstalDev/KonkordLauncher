using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace KonkordLibrary.Models.Launcher
{
    public class SkinLibData
    {
        [JsonProperty("selectedSkin"), JsonPropertyName("selectedSkin")]
        public string SelectedSkin { get; set; }
        [JsonProperty("skins"), JsonPropertyName("skins")]
        public List<SkinLib> Skins { get; set; }

        public SkinLibData() 
        {
            SelectedSkin = string.Empty;
            Skins = new List<SkinLib>();
        }
    }
}
