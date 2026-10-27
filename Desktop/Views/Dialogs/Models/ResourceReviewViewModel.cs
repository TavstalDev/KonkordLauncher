using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Tavstal.BeaconSDK.Models;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Services.Content;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

public partial class ResourceReviewViewModel : KonkordObservableObject
{
    private readonly IHttpService _httpService = null!;
    private readonly Instance _instance = null!;
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IMetaCacheService _metaCacheService = null!;
    private readonly IKonkordProgress _progressReporter = null!;
    private readonly EResourceType _resourceType;
    private readonly ITranslationService _translationService = null!;
    private readonly IDialogService _dialogService = null!;

    public ResourceReviewViewModel()
    {
        Resources = [];
    }

    public ResourceReviewViewModel(Instance instance, EResourceType type, List<ResourceDownloadModel> resources,
        IKonkordProgress progressReporter, IHttpService httpService, ITranslationService translationService,
        ILauncherStore launcherStore, IMetaCacheService metaCacheService, IDialogService dialogService)
    {
        _instance = instance;
        _resourceType = type;
        _progressReporter = progressReporter;
        _httpService = httpService;
        _translationService = translationService;
        _launcherStore = launcherStore;
        _metaCacheService = metaCacheService;
        _dialogService = dialogService;

        if (Design.IsDesignMode)
        {
            Resources =
            [
                new ResourceDownloadModel
                {
                    Name = "Test",
                    FileName = "test.jar",
                    Version = "1.0.0",
                    VersionId = "",
                    Url = "",
                    ProjectId = "",
                    Sha1 = "",
                    Sha512 = "",
                    Platform = EPlatformType.MODRINTH,
                    ShouldDownload = true
                },
                new ResourceDownloadModel
                {
                    Name = "Test2",
                    FileName = "test2.jar",
                    Version = "1.0.0",
                    VersionId = "",
                    Url = "",
                    ProjectId = "",
                    Sha1 = "",
                    Sha512 = "",
                    Platform = EPlatformType.MODRINTH,
                    ShouldDownload = true
                },
                new ResourceDownloadModel
                {
                    Name = "Test3",
                    FileName = "test3.jar",
                    Version = "1.0.0",
                    Url = "",
                    ProjectId = "",
                    VersionId = "",
                    Sha1 = "",
                    Sha512 = "",
                    Platform = EPlatformType.MODRINTH,
                    ShouldDownload = true
                }
            ];
            return;
        }

        Resources = new ObservableCollection<ResourceDownloadModel>(resources);
    }

    public ObservableCollection<ResourceDownloadModel> Resources { get; set; }

    #region Interactions

    public Interaction<bool, Unit> CloseWindowInteraction { get; } = new();
    #endregion

    #region Commands

    /// <summary>
    ///     Downloads all selected resources, saves the instance resource configuration,
    ///     and closes the window upon success. Shows an alert when complete.
    /// </summary>
    [RelayCommand]
    private async Task Install()
    {
        var gameDir = _instance.GameDirectory!;
        var targetDir = string.Empty;
        var dirName = string.Empty;
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

            var targetPath = Path.Combine(targetDir, resource.FileName);
            var relativePath = Path.Combine(dirName, resource.FileName);

            var newResource = new InstanceResource
            {
                Name = resource.Name,
                Type = _resourceType,
                Path = relativePath,
                ProjectId = resource.ProjectId,
                VersionId = resource.VersionId,
                Url = resource.Url,
                Sha1 = resource.Sha1,
                Sha512 = resource.Sha512,
                Platform = resource.Platform,
                FileSize = resource.FileSize,
                Client = null,
                Server = null,
                IconPath = null
            };

            if (resource.IconUrl != null)
            {
                var iconPath = _metaCacheService.GetImagePath(resource.IconUrl);
                if (iconPath != null)
                {
                    downloadEntries.Add(new DownloadEntry(resource.IconUrl, iconPath, null, _progressReporter));
                    newResource.IconPath = iconPath;
                }
            }

            downloadEntries.Add(new DownloadEntry(resource.Url, targetPath, null, _progressReporter));
            newResources.Add(newResource);
        }

        await _httpService.ParallelDownloadFilesAsync(downloadEntries);
        var instanceConfigPath = _instance.GetResourceConfigPath();
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
        await _dialogService.ShowAlertAsync(
            _translationService.Translate("instance.resources.download.complete.title"),
            _translationService.Translate("instance.resources.download.complete.description"),
            EAlertType.Success);
        await CloseWindowInteraction.Handle(true);
    }

    /// <summary>
    ///     Requests the window to close by invoking the <see cref="CloseWindowInteraction" /> interaction.
    /// </summary>
    [RelayCommand]
    private async Task CloseWindow()
    {
        await CloseWindowInteraction.Handle(false);
    }

    #endregion
}
