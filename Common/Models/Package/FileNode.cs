namespace Tavstal.KonkordLauncher.Common.Models.Package;

public class FileNode
{
    public string Name { get; set; }
    
    public string Path { get; set; }
    
    public bool IsDirectory { get; set; }
    
    public List<FileNode> Children { get; set; }
    
    public FileNode(string name, string path, bool isDirectory)
    {
        Name = name;
        Path = path;
        IsDirectory = isDirectory;
        Children = [];
    }
}