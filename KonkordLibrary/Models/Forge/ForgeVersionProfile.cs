﻿using KonkordLibrary.Models.Minecraft.Library;
using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;

namespace KonkordLibrary.Models.Forge
{
    public class ForgeVersionProfile
    {
        [JsonPropertyName("spec")]
        public int Spec {  get; set; }
        [JsonPropertyName("profile")]
        public string Profile { get; set; }
        [JsonPropertyName("version")]
        public string Version { get; set; }
        [JsonPropertyName("path")]
        public string? Path { get; set; }
        [JsonPropertyName("minecraft")]
        public string Minecraft { get; set; }
        [JsonPropertyName("serverJarPath")]
        public string ServerJarPath { get; set; }
        [JsonPropertyName("data")]
        public JObject Data { get; set; }
        [JsonPropertyName("processors")]
        public JArray Processors { get; set; }
        [JsonPropertyName("libraries")]
        public List<MCLibrary> Libraries { get; set; }
    }
}