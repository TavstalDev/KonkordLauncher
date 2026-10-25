using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;

namespace Tavstal.KonkordLauncher.Desktop.Services.Instances;

public interface IInstanceModelFactory
{
    InstanceModel Create(Instance instance);
}
