using System.Collections.ObjectModel;

namespace Tavstal.KonkordLauncher.Desktop.Models.Domain;

/// <summary>
///     A named group of instances, used to group them in the UI.
/// </summary>
public class InstanceGroup
{
    /// <summary>
    ///     Creates a group with the given name and an empty instance list.
    /// </summary>
    /// <param name="name">The group name.</param>
    public InstanceGroup(string name)
    {
        GroupName = name;
        Instances = [];
    }

    /// <summary>
    ///     Gets or sets the group name.
    /// </summary>
    public string GroupName { get; set; }

    /// <summary>
    ///     Gets the instances in this group.
    /// </summary>
    public ObservableCollection<InstanceModel> Instances { get; }
}
