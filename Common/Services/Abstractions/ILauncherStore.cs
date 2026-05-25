using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Core.Models.Instance;

namespace Tavstal.KonkordLauncher.Common.Services.Abstractions;

public interface ILauncherStore
{
    Task<CoreConfig> GetSettingsAsync(Resolution? screenResolution = null, CancellationToken cancellationToken = default);
    
    Task<bool> SaveSettingsAsync(CoreConfig settings, CancellationToken cancellationToken = default);

    Task<AccountData> GetAccountDataAsync(CancellationToken cancellationToken = default);
    
    Task<bool> SaveAccountDataAsync(AccountData accountData, CancellationToken cancellationToken = default);

    Task<List<Instance>> GetInstancesAsync(CancellationToken cancellationToken = default);
    
    Task<bool> SaveInstancesAsync(List<Instance> instances, CancellationToken cancellationToken = default);

    Task<List<PatchNote>> GetPatchNotesAsync(string cacheDir, CancellationToken cancellationToken = default);
}