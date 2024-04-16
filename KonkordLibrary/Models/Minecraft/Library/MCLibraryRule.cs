using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLibrary.Models.Minecraft.Library
{
    public class MCLibraryRule
    {
        [JsonPropertyName("action"), JsonProperty("action")]
        public string Action { get; set; }
        [JsonPropertyName("OS"), JsonProperty("OS")]
        public MCLibraryRuleValue OS { get; set; }

        public MCLibraryRule() { }
        public MCLibraryRule(string action, MCLibraryRuleValue oS)
        {
            Action = action;
            OS = oS;
        }
    }
}
