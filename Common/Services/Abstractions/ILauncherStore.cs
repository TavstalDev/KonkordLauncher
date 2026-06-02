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
    /// Gets the launcher settings from persistent storage.
    /// </summary>
    /// <returns>The loaded <see cref="CoreConfig"/>, or <see langword="null"/> if the settings could not be retrieved.</returns>
    CoreConfig? GetSettings();

    /// <summary>
    /// Gets the launcher settings from persistent storage, creating defaults if necessary.
    /// </summary>
    /// <param name="screenResolution">
    /// Optional screen resolution used when creating a new settings file.
    /// </param>
    /// <param name="cancellationToken">Cancellation token observed during file IO.</param>
    /// <returns>A task that resolves to the loaded <see cref="CoreConfig"/>.</returns>
    Task<CoreConfig> GetSettingsAsync(Resolution? screenResolution = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves launcher settings to persistent storage.
    /// </summary>
    /// <param name="settings">The settings object to save.</param>
    /// <returns><see langword="true"/> if the save succeeded; otherwise, <see langword="false"/>.</returns>
    bool SaveSettings(CoreConfig settings);

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
    /// <returns>The loaded <see cref="AccountData"/>.</returns>
    AccountData? GetAccountData();

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
    /// <returns><see langword="true"/> if the save succeeded; otherwise, <see langword="false"/>.</returns>
    bool SaveAccountData(AccountData accountData);

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
    /// <returns>The list of saved <see cref="Instance"/> entries.</returns>
    List<Instance>? GetInstances();

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
    /// <returns><see langword="true"/> if the save succeeded; otherwise, <see langword="false"/>.</returns>
    bool SaveInstances(List<Instance> instances);

    /// <summary>
    /// Saves the configured launcher instances to persistent storage.
    /// </summary>
    /// <param name="instances">The instances collection to save.</param>
    /// <param name="cancellationToken">Cancellation token observed during file IO.</param>
    /// <returns>A task that resolves to <see langword="true"/> if the save succeeded; otherwise, <see langword="false"/>.</returns>
    Task<bool> SaveInstancesAsync(List<Instance> instances, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves cached instance resources for the specified launcher instance.
    /// </summary>
    /// <param name="instance">The <see cref="Instance"/> whose resources should be returned.</param>
    /// <returns>
    /// A list of <see cref="InstanceResource"/> objects associated with the instance,
    /// or <see langword="null"/> if no resources are available or the instance cannot be resolved.
    /// </returns>
    List<InstanceResource>? GetInstanceResources(Instance instance);

    /// <summary>
    /// Asynchronously retrieves cached instance resources for the specified launcher instance.
    /// </summary>
    /// <param name="instance">The <see cref="Instance"/> whose resources should be returned.</param>
    /// <param name="cancellationToken">Cancellation token observed during file IO and deserialization.</param>
    /// <returns>
    /// A task that resolves to a list of <see cref="InstanceResource"/> objects associated with the instance.
    /// The returned list will be empty if no resources are found.
    /// </returns>
    Task<List<InstanceResource>> GetInstanceResourcesAsync(Instance instance,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists the provided collection of instance resources for the instance.
    /// </summary>
    /// <param name="instance">The <see cref="Instance"/> to which the resources belong.</param>
    /// <param name="resources">The list of <see cref="InstanceResource"/> objects to save.</param>
    /// <returns>
    /// <see langword="true"/> if the resources were saved successfully; otherwise <see langword="false"/>.
    /// Implementations should handle IO exceptions and return <see langword="false"/> on failure rather than throwing.
    /// </returns>
    bool SaveInstanceResources(Instance instance, List<InstanceResource> resources);

    /// <summary>
    /// Asynchronously persists the provided collection of instance resources for the instance.
    /// </summary>
    /// <param name="instance">The <see cref="Instance"/> to which the resources belong.</param>
    /// <param name="resources">The list of <see cref="InstanceResource"/> objects to save.</param>
    /// <param name="cancellationToken">Cancellation token observed during file IO.</param>
    /// <returns>
    /// A task that resolves to <see langword="true"/> if the resources were saved successfully; otherwise <see langword="false"/>.
    /// Implementations should log and return <see langword="false"/> on failure instead of throwing exceptions to callers.
    /// </returns>
    Task<bool> SaveInstanceResourcesAsync(Instance instance, List<InstanceResource> resources,
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Gets cached patch notes from the specified cache directory.
    /// </summary>
    /// <param name="cacheDir">The directory containing cached patch note data.</param>
    /// <param name="cancellationToken">Cancellation token observed during file IO.</param>
    /// <returns>A task that resolves to the list of cached <see cref="PatchNote"/> entries.</returns>
    Task<List<PatchNote>> GetPatchNotesAsync(string cacheDir, CancellationToken cancellationToken = default);
}