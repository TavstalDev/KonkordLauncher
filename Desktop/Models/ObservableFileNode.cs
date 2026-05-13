using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Common.Models.Package;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;

namespace Tavstal.KonkordLauncher.Desktop.Models;

public partial class ObservableFileNode : KonkordObservableObject
{
    public string Name { get; set; }
    public string Path { get; set; }
    public bool IsDirectory { get; set; }
    [ObservableProperty]
    public partial bool IsChecked { get; set; }
    public ObservableCollection<ObservableFileNode> Children { get; set; }

    public ObservableFileNode(string name, string path, bool isDirectory)
    {
        Name = name;
        Path = path;
        IsDirectory = isDirectory;
        Children = [];
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (Children.Count == 0)
            return;
        
        foreach (var child in Children)
            child.IsChecked = IsChecked;
    }

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