﻿using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Tavstal.KonkordLibrary.Models.Instances.CurseForge
{
    public class CurseModLoader
    {
        [JsonProperty("id"), JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonProperty("primary"), JsonPropertyName("primary")]
        public bool IsPrimary { get; set; }

        public CurseModLoader() { }

        public CurseModLoader(string id, bool isPrimary)
        {
            Id = id;
            IsPrimary = isPrimary;
        }
    }
}
