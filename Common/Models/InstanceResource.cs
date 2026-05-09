using Tavstal.KonkordLauncher.Core.Enums;

namespace Tavstal.KonkordLauncher.Common.Models;

public class InstanceResource
{
    public string Name { get; set; }
    
    public string Url { get; set; }
    
    public string Path { get; set; }
    
    public string Client { get; set; }
    
    public string Server { get; set; }
    
    public string Sha1 { get; set; }
    
    public string Sha512 { get; set; }
    
    public EPlatformType Platform { get; set; }
    
    public EResourceType Type { get; set; }
    
    public int FileSize { get; set; }
}