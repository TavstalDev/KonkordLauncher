using System.Collections.ObjectModel;

namespace Tavstal.KonkordLauncher.Desktop.Models.Domain;


/// <summary>
/// Groups a set of instances under a common display name.
/// Used to present instances grouped in the UI (for example by profile, source, or category).
/// </summary>
public class InstanceGroup
{
    /// <summary>
    /// Display name of the group (e.g. "Vanilla", "Modded", "Favorites").
    /// </summary>
    public string GroupName { get; set; }
    
    /// <summary>
    /// Collection of instances that belong to this group.
    /// The collection is created and owned by this object; it can be observed for changes by the UI.
    /// </summary>
    public ObservableCollection<InstanceModel> Instances { get; }
    
    /// <summary>
    /// Initializes a new <see cref="InstanceGroup"/> with the specified group name.
    /// The <see cref="Instances"/> collection is initialized empty.
    /// </summary>
    /// <param name="name">The display name for the group.</param>
    public InstanceGroup(string name)
    {
        GroupName = name;
        Instances = [];
    }
}