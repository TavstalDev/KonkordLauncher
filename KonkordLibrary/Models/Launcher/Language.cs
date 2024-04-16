using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Tavstal.KonkordLibrary.Models.Launcher
{
    public class Language
    {
        [JsonProperty("name"), JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonProperty("twoLetterCode"), JsonPropertyName("twoLetterCode")]
        public string TwoLetterCode { get; set; }
        [JsonProperty("threeLetterCode"), JsonPropertyName("threeLetterCode")]
        public string ThreeLetterCode { get; set; }
        [JsonProperty("url"), JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonProperty("isDefault"), JsonPropertyName("isDefault")]
        public bool IsDefault { get; set; }

        public Language() { }

        public Language(string name, string twoLetterCode, string threeLetterCode, string url)
        {
            Name = name;
            TwoLetterCode = twoLetterCode;
            ThreeLetterCode = threeLetterCode;
            Url = url;
            IsDefault = false;
        }

        public Language(string name, string twoLetterCode, string threeLetterCode, string url, bool isDefault)
        {
            Name = name;
            TwoLetterCode = twoLetterCode;
            ThreeLetterCode = threeLetterCode;
            Url = url;
            IsDefault = isDefault;
        }
    }
}
