using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tavstal.BeaconSDK.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs.Config;

namespace Tavstal.KonkordLauncher.Desktop.Services.Abstractions;

public interface ILauncherStore
{
    string GetTempDir();
    
    CoreConfig? GetSettings();
    
    Task<CoreConfig> GetSettingsAsync(Resolution? screenResolution = null,
        CancellationToken cancellationToken = default);
    
    bool SaveSettings(CoreConfig settings);
    
    Task<bool> SaveSettingsAsync(CoreConfig settings, CancellationToken cancellationToken = default);
    
    AccountData? GetAccountData();
    
    Task<AccountData> GetAccountDataAsync(CancellationToken cancellationToken = default);
    
    bool SaveAccountData(AccountData accountData);
    
    Task<bool> SaveAccountDataAsync(AccountData accountData, CancellationToken cancellationToken = default);
    
    List<Instance>? GetInstances();
    
    Task<List<Instance>> GetInstancesAsync(CancellationToken cancellationToken = default);
    
    bool SaveInstances(List<Instance> instances);
    
    Task<bool> SaveInstancesAsync(List<Instance> instances, CancellationToken cancellationToken = default);
    
    List<InstanceResource>? GetInstanceResources(Instance instance);
    
    Task<List<InstanceResource>> GetInstanceResourcesAsync(Instance instance,
        CancellationToken cancellationToken = default);
    
    bool SaveInstanceResources(Instance instance, List<InstanceResource> resources);
    
    Task<bool> SaveInstanceResourcesAsync(Instance instance, List<InstanceResource> resources,
        CancellationToken cancellationToken = default);
    
    Task<List<PatchNote>> GetPatchNotesAsync(string cacheDir, CancellationToken cancellationToken = default);
}
