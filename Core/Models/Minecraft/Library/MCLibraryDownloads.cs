using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.Minecraft.Library
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
