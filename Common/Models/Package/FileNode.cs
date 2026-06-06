using System.Text.Json.Serialization;


namespace Tavstal.KonkordLauncher.Common.Models.Package;


/// <summary>
/// Represents a file or directory entry inside a package tree.
/// </summary>
public class FileNode
{
    /// <summary>
    /// Gets or sets the display name of the file or directory.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the full or relative path of the file or directory within the package.
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether this node represents a directory.
    /// </summary>
    [JsonPropertyName("isDirectory")]
    public bool IsDirectory { get; set; }
    
    /// <summary>
    /// Gets or sets the child entries of this node.
    /// </summary>
    [JsonPropertyName("children")]
    public List<FileNode> Children { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FileNode"/> class.
    /// </summary>
    /// <param name="name">The file or directory name.</param>
    /// <param name="path">The file or directory path.</param>
    /// <param name="isDirectory">A value indicating whether the node is a directory.</param>
    public FileNode(string name, string path, bool isDirectory)
    {
        Name = name;
        Path = path;
        IsDirectory = isDirectory;
        Children = [];
    }
}