using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Tavstal.KonkordLauncher.Core.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

public partial class ResourceReviewViewModel : KonkordObservableObject
{
    private readonly ILauncherStore _launcherStore;
    private readonly IManifestService _manifestService;
    private readonly IMetaCacheService _metaCacheService;
    public readonly Instance Instance;
    public ObservableCollection<ResourceDownloadModel> Resources { get; set; }
    
    #region Interactions
    
    
    public Interaction<bool, Unit> CloseWindowInteraction { get; } = new();
    #endregion

    public ResourceReviewViewModel(Instance instance, List<ResourceDownloadModel> resources)
    {
        Instance = instance;
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
                    Platform = EPlatformType.MODRINTH,
                    ShouldDownload = true,
                },
                new ResourceDownloadModel
                {
                    Name = "Test2",
                    FileName = "test2.jar",
                    Version = "1.0.0",
                    Url = "",
                    Platform = EPlatformType.MODRINTH,
                    ShouldDownload = true,
                },
                new ResourceDownloadModel
                {
                    Name = "Test3",
                    FileName = "test3.jar",
                    Version = "1.0.0",
                    Url = "",
                    Platform = EPlatformType.MODRINTH,
                    ShouldDownload = true,
                },
            ];
            return;
        }
        
        Resources = new ObservableCollection<ResourceDownloadModel>(resources);

        var services = Program.ServiceProvider;
        _launcherStore = services.GetRequiredService<ILauncherStore>();
        _manifestService = services.GetRequiredService<IManifestService>();
        _metaCacheService = services.GetRequiredService<IMetaCacheService>();
    }
    
    #region Commands

    [RelayCommand]
    public async Task Install()
    {
        
    }
    
    /// <summary>
    /// Requests the window to close by invoking the <see cref="CloseWindowInteraction"/> interaction.
    /// </summary>
    [RelayCommand]
    public async Task CloseWindow() => await CloseWindowInteraction.Handle(false);
    
    #endregion
}