using System.Text.Json.Serialization;

namespace KonkordLibrary.Models.Minecraft.Library
{
    public class MCLibraryRule
    {
        [JsonPropertyName("action")]
        public string Action { get; set; }
        [JsonPropertyName("OS")]
        public MCLibraryRuleValue OS { get; set; }

        public MCLibraryRule() { }
        public MCLibraryRule(string action, MCLibraryRuleValue oS)
        {
            Action = action;
            OS = oS;
        }
    }
}
