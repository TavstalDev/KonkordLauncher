using System.Text.Json.Serialization;

namespace KonkordLibrary.Models.Minecraft.Meta
{
    public class MCLogging
    {
        [JsonPropertyName("client")]
        public MCLoggingClient Client { get; set; }

        public MCLogging() { }
        public MCLogging(MCLoggingClient client) {  Client = client; }
    }
}
