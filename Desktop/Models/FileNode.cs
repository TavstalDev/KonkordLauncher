using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;

namespace Tavstal.KonkordLauncher.Desktop.Models;

public partial class FileNode : KonkordObservableObject
{
    public string Name { get; set; }
    public string Path { get; set; }
    public bool IsDirectory { get; set; }
    [ObservableProperty]
    public partial bool IsChecked { get; set; }
    public ObservableCollection<FileNode> Children { get; set; }

    public FileNode(string name, string path, bool isDirectory)
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

    public static FileNode FromDirectory(string directoryPath)
    {
        var directoryInfo = new DirectoryInfo(directoryPath);
        var node = new FileNode(directoryInfo.Name, directoryInfo.FullName, true);
        foreach (var dir in directoryInfo.GetDirectories("*",  SearchOption.TopDirectoryOnly))
            node.Children.Add(FromDirectory(dir.FullName));
        foreach (var file in directoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly))
            node.Children.Add(new FileNode(file.Name, file.FullName, false));
        return node;
    }
}