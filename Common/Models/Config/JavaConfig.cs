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
    
    [JsonProperty("defaultJavaPath"), JsonPropertyName("defaultJavaPath")]
    public string DefaultJavaPath { get; set; }
    
    [JsonProperty("jvmArguments"), JsonPropertyName("jvmArguments")]
    public string JvmArguments { get; set; }
    
    [JsonProperty("javaPaths"), JsonPropertyName("javaPaths")]
    public Dictionary<int, List<string>> JavaPaths { get; set; }

    public JavaConfig()
    {
        MinMemory = 1024;
        MaxMemory = 4096;
        PermaGen = 128;
        DefaultJavaPath = string.Empty;
        JvmArguments = string.Empty;
        JavaPaths = new Dictionary<int, List<string>>();
    }

    public JavaConfig(uint minMemory, uint maxMemory, uint permaGen, string defaultJavaPath, string jvmArguments, Dictionary<int, List<string>> javaPaths)
    {
        MinMemory = minMemory;
        MaxMemory = maxMemory;
        PermaGen = permaGen;
        DefaultJavaPath = defaultJavaPath;
        JvmArguments = jvmArguments;
        JavaPaths = javaPaths;
    }
}