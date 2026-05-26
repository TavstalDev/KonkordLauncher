using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Core.Models.Instance;

namespace Tavstal.KonkordLauncher.Common.Services.Abstractions;

/// <summary>
/// Defines persistence operations for launcher settings, accounts, instances, and patch notes.
/// </summary>
public interface ILauncherStore
{
    /// <summary>
    /// Gets the launcher settings from persistent storage, creating defaults if necessary.
    /// </summary>
    /// <param name="screenResolution">
    /// Optional screen resolution used when creating a new settings file.
    /// </param>
    /// <param name="cancellationToken">Cancellation token observed during file IO.</param>
    /// <returns>A task that resolves to the loaded <see cref="CoreConfig"/>.</returns>
    Task<CoreConfig> GetSettingsAsync(Resolution? screenResolution = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Saves launcher settings to persistent storage.
    /// </summary>
    /// <param name="settings">The settings object to save.</param>
    /// <param name="cancellationToken">Cancellation token observed during file IO.</param>
    /// <returns>A task that resolves to <see langword="true"/> if the save succeeded; otherwise, <see langword="false"/>.</returns>
    Task<bool> SaveSettingsAsync(CoreConfig settings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the saved account data from persistent storage.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token observed during file IO.</param>
    /// <returns>A task that resolves to the loaded <see cref="AccountData"/>.</returns>
    Task<AccountData> GetAccountDataAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Saves account data to persistent storage.
    /// </summary>
    /// <param name="accountData">The account data object to save.</param>
    /// <param name="cancellationToken">Cancellation token observed during file IO.</param>
    /// <returns>A task that resolves to <see langword="true"/> if the save succeeded; otherwise, <see langword="false"/>.</returns>
    Task<bool> SaveAccountDataAsync(AccountData accountData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the configured launcher instances from persistent storage.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token observed during file IO.</param>
    /// <returns>A task that resolves to the list of saved <see cref="Instance"/> entries.</returns>
    Task<List<Instance>> GetInstancesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Saves the configured launcher instances to persistent storage.
    /// </summary>
    /// <param name="instances">The instances collection to save.</param>
    /// <param name="cancellationToken">Cancellation token observed during file IO.</param>
    /// <returns>A task that resolves to <see langword="true"/> if the save succeeded; otherwise, <see langword="false"/>.</returns>
    Task<bool> SaveInstancesAsync(List<Instance> instances, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets cached patch notes from the specified cache directory.
    /// </summary>
    /// <param name="cacheDir">The directory containing cached patch note data.</param>
    /// <param name="cancellationToken">Cancellation token observed during file IO.</param>
    /// <returns>A task that resolves to the list of cached <see cref="PatchNote"/> entries.</returns>
    Task<List<PatchNote>> GetPatchNotesAsync(string cacheDir, CancellationToken cancellationToken = default);
}