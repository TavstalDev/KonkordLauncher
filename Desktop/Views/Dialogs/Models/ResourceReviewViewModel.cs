using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

public partial class ResourceReviewViewModel : KonkordObservableObject
{
    private readonly ILauncherStore _launcherStore;
    private readonly IManifestService _manifestService;
    private readonly IMetaCacheService _metaCacheService;
    public readonly Instance Instance;
    public ObservableCollection<InstanceResource> InstanceResources { get; set; } = [];
    
    #region Interactions
    public Interaction<Unit, Unit> MinimizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> MaximizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();
    #endregion

    public ResourceReviewViewModel()
    {
        if (Design.IsDesignMode)
        {
            InstanceResources =
            [
                new InstanceResource
                {
                    Name = "Test",
                    ProjectId = "1",
                    Url = "",
                    Path = "mods/test.jar"
                },
                new InstanceResource
                {
                    Name = "Test2",
                    ProjectId = "2",
                    Url = "",
                    Path = "mods/test2.jar"
                },
                new InstanceResource
                {
                    Name = "Test3",
                    ProjectId = "3",
                    Url = "",
                    Path = "mods/test3.jar"
                }
            ];
            return;
        }
    }
    
    #region Commands

    /// <summary>
    /// Requests the window to minimize by invoking the <see cref="MinimizeWindowInteraction"/> interaction.
    /// </summary>
    [RelayCommand]
    public async Task MinimizeWindow() => await MinimizeWindowInteraction.Handle(Unit.Default);

    /// <summary>
    /// Requests the window to toggle maximize/restore by invoking the <see cref="MaximizeWindowInteraction"/> interaction.
    /// </summary>
    [RelayCommand]
    public async Task MaximizeWindow() => await MaximizeWindowInteraction.Handle(Unit.Default);

    /// <summary>
    /// Requests the window to close by invoking the <see cref="CloseWindowInteraction"/> interaction.
    /// </summary>
    [RelayCommand]
    public async Task CloseWindow() => await CloseWindowInteraction.Handle(Unit.Default);

    
    #endregion
}