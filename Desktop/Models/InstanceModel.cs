using System;
using System.Diagnostics;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Common.Models.InstanceConfig;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Desktop.Helpers;

namespace Tavstal.KonkordLauncher.Desktop.Models;

public partial class InstanceModel : ObservableObject
{
    [ObservableProperty] private string _id;

    [ObservableProperty] private string _name;

    [ObservableProperty] private string? _group;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Icon))] private string _iconPath;

    [ObservableProperty] private string _minecraftVersion;

    [ObservableProperty] private string _customVersion;

    [ObservableProperty] private EProfileType _type;

    [ObservableProperty] private EMinecraftKind _kind;

    [ObservableProperty] private string? _gameDirectory;

    [ObservableProperty] private InstanceConfig _config;
    
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsGameRunning))] private Process? _gameProcess;
    
    [ObservableProperty] private bool _isGameRunning;
    
    public Bitmap? Icon => string.IsNullOrEmpty(IconPath) ? ImageHelper.LoadFromResource(new Uri("avares://Desktop/Assets/Icons/dirt.png")) : new Bitmap(IconPath);
    
    public InstanceModel() {}
    
    public InstanceModel(Common.Models.Instance instance)
    {
        this.Id = instance.Id;
        this.Name = instance.Name;
        this.Group = instance.Group;
        this.IconPath = instance.IconPath;
        this.MinecraftVersion = instance.MinecraftVersion;
        this.CustomVersion = instance.CustomVersion;
        this.Type = instance.Type;
        this.Kind = instance.Kind;
        this.GameDirectory = instance.GameDirectory;
        this.Config = instance.Config;
    }

    public void AttachProcessEvent()
    {
        if (GameProcess == null)
            return;
        
        GameProcess.Exited += (sender, args) =>
        {
            IsGameRunning = false;
            GameProcess = null;
        };
        
        GameProcess.Disposed += (sender, args) =>
        {
            IsGameRunning = false;
            GameProcess = null;
        };
    }
}