using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.Platforms.CurseForge
{
    public class CurseFile
    {
        [JsonProperty("projectID"), JsonPropertyName("projectID")]
        public int ProjectId { get; set; }
        [JsonProperty("fileID"), JsonPropertyName("fileID")]
        public int FileId { get; set; }
        [JsonProperty("required"), JsonPropertyName("required")]
        public bool Required { get; set; }

        public CurseFile() { }

        public CurseFile(int projectId, int fileId, bool required)
        {
            ProjectId = projectId;
            FileId = fileId;
            Required = required;
        }
    }
}
