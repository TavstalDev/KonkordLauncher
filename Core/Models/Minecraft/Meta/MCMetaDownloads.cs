using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.Minecraft.Meta
{
    public class MCMetaDownloads
    {
        [JsonPropertyName("client"), JsonProperty("client")]
        public MCMetaDownload Client { get; set; }
        [JsonPropertyName("client_mappings"), JsonProperty("client_mappings")]
        public MCMetaDownload ClientMappings { get; set; }
        [JsonPropertyName("server"), JsonProperty("server")]
        public MCMetaDownload Server { get; set; }
        [JsonPropertyName("server_mappings"), JsonProperty("server_mappings")]
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
