using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLibrary.Models.Minecraft.Meta
{
    public class MCLogging
    {
        [JsonPropertyName("client"), JsonProperty("client")]
        public MCLoggingClient Client { get; set; }

        public MCLogging() { }
        public MCLogging(MCLoggingClient client) {  Client = client; }
    }
}
