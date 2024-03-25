using System.Text.Json.Serialization;

namespace KonkordLibrary.Models.Minecraft.Library
{
    public class MCLibraryDownloads
    {
        [JsonPropertyName("artifact")]
        public MCLibraryArtifact Artifact { get; set; }

        public MCLibraryDownloads() { }

        public MCLibraryDownloads(MCLibraryArtifact artifact)
        {
            Artifact = artifact;
        }
    }
}
