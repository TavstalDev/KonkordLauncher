﻿using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Tavstal.KonkordLibrary.Models.Instances.CurseForge
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
