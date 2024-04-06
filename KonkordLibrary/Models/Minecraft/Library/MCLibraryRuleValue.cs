using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace KonkordLibrary.Models.Minecraft.Library
{
    public class MCLibraryRuleValue
    {
        [JsonPropertyName("name"), JsonProperty("name")]
        public string Name { get; set; }

        public MCLibraryRuleValue() { }

        public MCLibraryRuleValue(string name)
        {
            Name = name;
        }
    }
}
