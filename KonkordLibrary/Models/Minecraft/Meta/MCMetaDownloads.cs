using System.Text.Json.Serialization;

namespace KonkordLibrary.Models.Minecraft.Meta
{
    public class MCMetaDownloads
    {
        [JsonPropertyName("client")]
        public MCMetaDownload Client { get; set; }
        [JsonPropertyName("client_mappings")]
        public MCMetaDownload ClientMappings { get; set; }
        [JsonPropertyName("server")]
        public MCMetaDownload Server { get; set; }
        [JsonPropertyName("server_mappings")]
        public MCMetaDownload ServerMappings { get; set;}

        public MCMetaDownloads() { }

        public MCMetaDownloads(MCMetaDownload client, MCMetaDownload clientMappings, MCMetaDownload server, MCMetaDownload serverMappings)
        {
            Client = client;
            ClientMappings = clientMappings;
            Server = server;
            ServerMappings = serverMappings;
        }
    }
}
