using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;

namespace Tavstal.KonkordLauncher.Desktop.Services.Instances;

/// <summary>
///     Creates <see cref="InstanceModel" /> objects from stored instance data.
/// </summary>
public interface IInstanceModelFactory
{
    /// <summary>
    ///     Creates a model for the given instance.
    /// </summary>
    /// <param name="instance">The instance data.</param>
    /// <returns>The new <see cref="InstanceModel" />.</returns>
    InstanceModel Create(Instance instance);
}
