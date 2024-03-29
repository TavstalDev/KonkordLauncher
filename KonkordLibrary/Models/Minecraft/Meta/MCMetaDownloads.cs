using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace KonkordLibrary.Models.Minecraft.Meta
{
    public class MCMetaDownloads
    {
        [JsonPropertyName("client")]
        public MCMetaDownload Client { get; set; }
        [JsonProperty("client_mappings")]
        public MCMetaDownload ClientMappings { get; set; }
        [JsonPropertyName("server")]
        public MCMetaDownload Server { get; set; }
        [JsonProperty("server_mappings")]
        public MCMetaDownload ServerMappings { get; set; }

        public MCMetaDownloads() { }

        public MCMetaDownloads(MCMetaDownload client, MCMetaDownload client_mappings, MCMetaDownload server, MCMetaDownload server_mappings)
        {
            Client = client;
            ClientMappings = client_mappings;
            Server = server;
            ServerMappings = server_mappings;
        }
    }
}
