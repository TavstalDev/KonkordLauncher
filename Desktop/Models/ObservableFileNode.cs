using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Common.Models.Package;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;

namespace Tavstal.KonkordLauncher.Desktop.Models;

/// <summary>
/// View-model representation of a file or directory node used by the export UI.
/// </summary>
public partial class ObservableFileNode : KonkordObservableObject
{
    /// <summary>
    /// Display name of the file or directory (the final path segment).
    /// Example: "mods", "options.txt".
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Full filesystem path to the file or directory represented by this node.
    /// Example: "/home/user/.minecraft/mods/optifine.jar".
    /// </summary>
    public string Path { get; set; }
    
    /// <summary>
    /// True when this node represents a directory; false when it represents a file.
    /// Directories typically have children, files do not.
    /// </summary>
    public bool IsDirectory { get; set; }
    
    /// <summary>
    /// Whether the node is checked/selected in the UI.
    /// </summary>
    [ObservableProperty]
    public partial bool IsChecked { get; set; }
    
    /// <summary>
    /// Child nodes (only populated for directories).
    /// </summary>
    public ObservableCollection<ObservableFileNode> Children { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="ObservableFileNode"/>.
    /// </summary>
    /// <param name="name">Node display name (file/directory name).</param>
    /// <param name="path">Full path for the node.</param>
    /// <param name="isDirectory">Whether the node is a directory.</param>
    public ObservableFileNode(string name, string path, bool isDirectory)
    {
        Name = name;
        Path = path;
        IsDirectory = isDirectory;
        Children = [];
    }

    /// <summary>
    /// Called when a property changes on this view-model.
    /// Propagates the <see cref="IsChecked"/> value to all children so checking/unchecking a directory
    /// cascades to its contained items.
    /// </summary>
    /// <param name="e">Property changed event arguments.</param>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (Children.Count == 0)
            return;
        
        foreach (var child in Children)
            child.IsChecked = IsChecked;
    }

    /// <summary>
    /// Builds an <see cref="ObservableFileNode"/> tree from a filesystem directory.
    /// </summary>
    /// <param name="directoryPath">Full path to the directory to convert.</param>
    /// <returns>A populated <see cref="ObservableFileNode"/> representing the directory and its immediate children.</returns>
    public static ObservableFileNode FromDirectory(string directoryPath)
    {
        var directoryInfo = new DirectoryInfo(directoryPath);
        var node = new ObservableFileNode(directoryInfo.Name, directoryInfo.FullName, true);
        foreach (var dir in directoryInfo.GetDirectories("*",  SearchOption.TopDirectoryOnly))
            node.Children.Add(FromDirectory(dir.FullName));
        foreach (var file in directoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly))
            node.Children.Add(new ObservableFileNode(file.Name, file.FullName, false));
        return node;
    }
    
    /// <summary>
    /// Converts this <see cref="ObservableFileNode"/> (and its children) into a <see cref="FileNode"/>
    /// suitable for packaging/export. By default, unchecked files are skipped.
    /// </summary>
    /// <param name="observableNode">The node to convert.</param>
    /// <param name="skipUncheckedFiles">If true, files that are not checked will be omitted from the result.</param>
    /// <returns>
    /// The converted <see cref="FileNode"/> or null if the node should be skipped (for example,
    /// an unchecked file when <paramref name="skipUncheckedFiles"/> is true).
    /// </returns>
    public static FileNode? ToFileNode(ObservableFileNode observableNode, bool skipUncheckedFiles = true)
    {
        if (skipUncheckedFiles)
        {
            if (!observableNode.IsDirectory && !observableNode.IsChecked)
                return null;
        }
        
        var fileNode = new FileNode(observableNode.Name, observableNode.Path, observableNode.IsDirectory);
        foreach (var child in observableNode.Children)
        {
            var node = ToFileNode(child, skipUncheckedFiles);
            if (node == null)
                continue;
            fileNode.Children.Add(node);
        }

        return fileNode;
    }

    /// <summary>
    /// Converts a list of <see cref="ObservableFileNode"/> roots into a list of <see cref="FileNode"/> roots.
    /// Files that are unchecked are omitted (unless they are directories containing checked children).
    /// </summary>
    /// <param name="observableNodes">List of observable nodes (top-level items displayed in the UI).</param>
    /// <returns>List of converted <see cref="FileNode"/> objects ready for export processing.</returns>
    public static List<FileNode> ToFileNodes(List<ObservableFileNode> observableNodes)
    {
        List<FileNode> result = [];

        foreach (var observableNode in observableNodes)
        {
            if (!observableNode.IsDirectory && !observableNode.IsChecked)
                continue;
            var node = ToFileNode(observableNode);
            if (node == null)
                continue;
            result.Add(node);
        }
        
        return result;
    }
}