using System.Collections.ObjectModel;

namespace Tavstal.KonkordLauncher.Desktop.Models;

public class InstanceGroup
{
    public string GroupName { get; set; }
    public ObservableCollection<InstanceModel> Instances { get; }
    
    public InstanceGroup(string name)
    {
        GroupName = name;
        Instances = new ObservableCollection<InstanceModel>();
    }
}