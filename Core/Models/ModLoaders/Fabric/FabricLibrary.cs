
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.ModLoaders.Fabric;

public class FabricLibrary
{
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("url")]
    public string Url { get; set; }
    [JsonProperty("md5")]
    public string Md5 { get; set; }
    [JsonProperty("sha1")]
    public string Sha1 { get; set; }
    [JsonProperty("sha256")]
    public string Sha256 { get; set; }
    [JsonProperty("sha512")]
    public string Sha512 { get; set; }
    [JsonProperty("size")]
    public int Size { get; set; }

    public FabricLibrary() { }


    public string GetURL()
    {
        string path;
        string[] parts = Name.Split(":", 3);
        path = parts[0].Replace(".", "/") + "/" + parts[1] + "/" + parts[2] + "/" + parts[1] + "-" + parts[2] + ".jar";

        return Url + path;
    }

    public string GetPath()
    {
        string[] parts = Name.Split(":", 3);
        char separator = '/';
        string path = parts[0].Replace('.', separator) + separator + parts[1] + separator + parts[2] + separator + parts[1] + "-" + parts[2] + ".jar";
        return path.Replace(" ", "_");
    }
}