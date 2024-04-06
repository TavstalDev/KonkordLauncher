using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace KonkordLibrary.Models.Minecraft.Library
{
    public class MCLibraryDownloads
    {
        [JsonPropertyName("artifact"), JsonProperty("artifact")]
        public MCLibraryArtifact Artifact { get; set; }
        [JsonPropertyName("classifiers"), JsonProperty("classifiers")]
        public MCLibraryClassifier? Classifiers { get; set; }

        public MCLibraryDownloads() { }

        public MCLibraryDownloads(MCLibraryArtifact artifact, MCLibraryClassifier? classifiers)
        {
            Artifact = artifact;
            Classifiers = classifiers;
        }
    }
}
