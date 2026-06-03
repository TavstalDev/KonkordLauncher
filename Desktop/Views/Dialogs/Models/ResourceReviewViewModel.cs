using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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

public partial class ResourceReviewViewModel : KonkordObservableObject
{
    private readonly Instance _instance;
    private readonly EResourceType _resourceType;
    private readonly IHttpService _httpService;
    private readonly ITranslationService _translationService;
    private readonly ILauncherStore _launcherStore;
    private readonly IManifestService _manifestService;
    private readonly IMetaCacheService _metaCacheService;
    private readonly IProgressReporter _progressReporter;
    public ObservableCollection<ResourceDownloadModel> Resources { get; set; }
    
    #region Interactions
    public Interaction<bool, Unit> CloseWindowInteraction { get; } = new();
    public Interaction<Alert, Unit> ShowAlertInteraction { get; } = new();
    #endregion

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
        _manifestService = services.GetRequiredService<IManifestService>();
        _metaCacheService = services.GetRequiredService<IMetaCacheService>();
    }
    
    #region Commands

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
        var tasks = Resources.Select(x =>
        {
            if (!x.ShouldDownload)
                return Task.CompletedTask;

            string targetPath = Path.Combine(targetDir, x.FileName);
            string relativePath = Path.Combine(dirName, x.FileName);

            newResources.Add(new InstanceResource
            {
                Name = x.Name,
                Type = _resourceType,
                Path = relativePath,
                ProjectId = x.ProjectId,
                Url = x.Url,
                Sha1 = x.Sha1,
                Sha512 = x.Sha512,
                Platform = x.Platform,
                Client = null,
                Server = null,
                IconPath = x.IconUrl
            });

            IProgress<double> progress = new Progress<double>(p =>
            {
                _progressReporter.ReportProgress(p);
                _progressReporter.UpdateStatusTranslated("instance.download.file", x.Name, p.ToString("0.00"));
            });

            return _httpService.DownloadFileAsync(x.Url, targetPath, progress);
        });

        await Task.WhenAll(tasks);
        string instanceConfigPath = _instance.GetResourceConfigPath();
        if (!File.Exists(instanceConfigPath))
        {
            await _launcherStore.SaveInstanceResourcesAsync(_instance, newResources);
        }
        else
        {
            var existingResources = await _launcherStore.GetInstanceResourcesAsync(_instance);
            existingResources!.AddRange(newResources);
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