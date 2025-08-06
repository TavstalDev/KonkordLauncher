using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Common.Models.Config;

public class JavaConfig
{
    [JsonProperty("minMemory"), JsonPropertyName("minMemory")]
    public uint MinMemory { get; set; }
    
    [JsonProperty("maxMemory"), JsonPropertyName("maxMemory")]
    public uint MaxMemory { get; set; }
    
    [JsonProperty("permaGen"), JsonPropertyName("permaGen")]
    public uint PermaGen { get; set; }
    
    [JsonProperty("javaPath"), JsonPropertyName("javaPath")]
    public string JavaPath { get; set; }
    
    [JsonProperty("jvmArguments"), JsonPropertyName("jvmArguments")]
    public string JvmArguments { get; set; }

    public JavaConfig()
    {
        MinMemory = 1024;
        MaxMemory = 4096;
        PermaGen = 128;
        JavaPath = string.Empty;
        JvmArguments = string.Empty;
    }

    public JavaConfig(uint minMemory, uint maxMemory, uint permaGen, string defaultJavaPath, string jvmArguments)
    {
        MinMemory = minMemory;
        MaxMemory = maxMemory;
        PermaGen = permaGen;
        JavaPath = defaultJavaPath;
        JvmArguments = jvmArguments;
    }
}