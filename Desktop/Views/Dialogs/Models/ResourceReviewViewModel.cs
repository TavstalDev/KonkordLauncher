using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

/// <summary>
/// ViewModel for reviewing and installing selected resources (mods, resource packs, shader packs).
/// Handles downloading files, saving instance resource configuration, and dependency resolution.
/// </summary>
public partial class ResourceReviewViewModel : KonkordObservableObject
{
    private readonly Instance _instance;
    private readonly EResourceType _resourceType;
    private readonly IHttpService _httpService = null!;
    private readonly ITranslationService _translationService = null!;
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IMetaCacheService _metaCacheService = null!;
    private readonly IProgressReporter _progressReporter;
    public ObservableCollection<ResourceDownloadModel> Resources { get; set; }
    
    #region Interactions
    public Interaction<bool, Unit> CloseWindowInteraction { get; } = new();
    public Interaction<Alert, Unit> ShowAlertInteraction { get; } = new();
    #endregion

    /// <summary>
    /// Initializes a new instance of the ResourceReviewViewModel class.
    /// </summary>
    /// <param name="instance">The target instance for resource installation.</param>
    /// <param name="type">The type of resources being installed.</param>
    /// <param name="resources">The list of resources to review and install.</param>
    /// <param name="progressReporter">Reporter for download progress.</param>
    public ResourceReviewViewModel(Instance instance, EResourceType type, List<ResourceDownloadModel> resources, IProgressReporter progressReporter)
    {
        _instance = instance;
        _resourceType = type;
        _progressReporter = progressReporter;
        if (Design.IsDesignMode)
        {
            Resources =
            [
                new ResourceDownloadModel
                {
                    Name = "Test",
                    FileName = "test.jar",
                    Version = "1.0.0",
                    Url = "",
                    ProjectId = "",
                    Sha1 = "",
                    Sha512 = "",
                    Platform = EPlatformType.MODRINTH,
                    ShouldDownload = true,
                },
                new ResourceDownloadModel
                {
                    Name = "Test2",
                    FileName = "test2.jar",
                    Version = "1.0.0",
                    Url = "",
                    ProjectId = "",
                    Sha1 = "",
                    Sha512 = "",
                    Platform = EPlatformType.MODRINTH,
                    ShouldDownload = true,
                },
                new ResourceDownloadModel
                {
                    Name = "Test3",
                    FileName = "test3.jar",
                    Version = "1.0.0",
                    Url = "",
                    ProjectId = "",
                    Sha1 = "",
                    Sha512 = "",
                    Platform = EPlatformType.MODRINTH,
                    ShouldDownload = true,
                },
            ];
            return;
        }
        
        Resources = new ObservableCollection<ResourceDownloadModel>(resources);

        var services = Program.ServiceProvider;
        _httpService = services.GetRequiredService<IHttpService>();
        _translationService = services.GetRequiredService<ITranslationService>();
        _launcherStore = services.GetRequiredService<ILauncherStore>();
        _metaCacheService = services.GetRequiredService<IMetaCacheService>();
    }
    
    #region Commands

    /// <summary>
    /// Downloads all selected resources, saves the instance resource configuration,
    /// and closes the window upon success. Shows an alert when complete.
    /// </summary>
    [RelayCommand]
    public async Task Install()
    {
        string gameDir = _instance.GameDirectory!;
        string targetDir = string.Empty;
        string dirName = string.Empty;
        switch (_resourceType)
        {
            case EResourceType.RESOURCE_PACK:
            {
                dirName = "resourcepacks";
                targetDir = Path.Combine(gameDir, dirName);
                break;
            }
            case EResourceType.MOD:
            {
                dirName = "mods";
                targetDir = Path.Combine(gameDir, dirName);
                break;
            }
            case EResourceType.SHADER_PACK:
            {
                dirName = "shaderpacks";
                targetDir = Path.Combine(gameDir, dirName);
                break;
            }
        }
        Directory.CreateDirectory(targetDir);
        
        var newResources = new List<InstanceResource>();
        
        _progressReporter.OpenReporter();
        List<DownloadEntry> downloadEntries = [];
        foreach (var resource in Resources)
        {
            if (!resource.ShouldDownload)
                continue;
            
            string targetPath = Path.Combine(targetDir, resource.FileName);
            string relativePath = Path.Combine(dirName, resource.FileName);

            var newResource = new InstanceResource
            {
                Name = resource.Name,
                Type = _resourceType,
                Path = relativePath,
                ProjectId = resource.ProjectId,
                Url = resource.Url,
                Sha1 = resource.Sha1,
                Sha512 = resource.Sha512,
                Platform = resource.Platform,
                Client = null,
                Server = null,
                IconPath = null
            };

            if (resource.IconUrl != null)
            {
                string? iconPath = _metaCacheService.GetImagePath(resource.IconUrl);
                if (iconPath != null)
                {
                    downloadEntries.Add(new DownloadEntry(resource.IconUrl, iconPath, _progressReporter));
                    newResource.IconPath = iconPath;
                }
            }
            downloadEntries.Add(new DownloadEntry(resource.Url, targetPath, _progressReporter));
            newResources.Add(newResource);
        }
        
        await _httpService.ParallelDownloadFilesAsync(downloadEntries);
        string instanceConfigPath = _instance.GetResourceConfigPath();
        if (!File.Exists(instanceConfigPath))
        {
            await _launcherStore.SaveInstanceResourcesAsync(_instance, newResources);
        }
        else
        {
            var existingResources = await _launcherStore.GetInstanceResourcesAsync(_instance);
            existingResources.AddRange(newResources);
            await _launcherStore.SaveInstanceResourcesAsync(_instance, existingResources);
        }
        
        _progressReporter.CloseReporter();
        await ShowAlertInteraction.Handle(new Alert(
            _translationService.Translate("instance.resources.download.complete"),
            _translationService.Translate("instance.resources.download.complete.description"),
            EAlertType.Success));
        await CloseWindowInteraction.Handle(true);
    }
    
    /// <summary>
    /// Requests the window to close by invoking the <see cref="CloseWindowInteraction"/> interaction.
    /// </summary>
    [RelayCommand]
    public async Task CloseWindow() => await CloseWindowInteraction.Handle(false);
    
    #endregion
}