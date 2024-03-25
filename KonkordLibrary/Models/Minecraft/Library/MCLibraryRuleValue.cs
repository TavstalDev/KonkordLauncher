using System.Text.Json.Serialization;

namespace KonkordLibrary.Models.Minecraft.Library
{
    public class MCLibraryRuleValue
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        public MCLibraryRuleValue() { }

        public MCLibraryRuleValue(string name)
        {
            Name = name;
        }
    }
}
