using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Core.Helpers.Domain;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Views.Models.CreateInstance;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

public partial class CreateInstanceViewModel : KonkordObservableObject
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(CreateInstanceViewModel));
    public CreateInstanceViewModel_Custom Custom { get;  }
    public CreateInstanceViewModel_Modpack Modpack { get;  }
    public CreateInstanceViewModel_Import Import { get;  }

    [ObservableProperty] private ECreateInstanceTab _selectedTab = ECreateInstanceTab.MODPACK;
    
    #region Interactions
    public Interaction<Unit, Unit> MinimizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> MaximizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();
    public Interaction<ECreateInstanceTab, Unit> SwitchTabInteraction { get; } = new();
    public Interaction<int, Unit> SwitchImportTabInteraction { get; } = new();
    public Interaction<Alert, Unit> ShowAlertDialogInteraction { get; } = new();
    public Interaction<Unit, string?> ShowIconSelectorInteraction { get; } = new();
    public Interaction<Unit, string?> ShowFileSelectorInteraction { get; } = new();
    #endregion

    public CreateInstanceViewModel()
    {
        Custom = new CreateInstanceViewModel_Custom(this);
        Modpack = new CreateInstanceViewModel_Modpack(this);
        Import = new CreateInstanceViewModel_Import(this);

        SetupPipeline();
        _ = InitAsync();
    }

    /// <summary>
    /// Releases the resources used by the CreateInstanceViewModel and performs cleanup operations.
    /// </summary>
    /// <param name="disposing">A boolean value indicating whether the method is being called directly or indirectly by a finalizer.</param>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Custom.Dispose();
        Modpack.Dispose();
        Import.Dispose();
    }

    /// <summary>
    /// Asynchronously initializes the CreateInstanceViewModel and its subcomponents.
    /// </summary>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> that can be used by callers to cancel the initialization flow.</param>
    /// <returns>A <see cref="Task"/> that completes when initialization has finished for the view-model and all child modules.</returns>
    private async Task InitAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        
        var settings = await Task.Run(() => LauncherHelper.GetLauncherSettingsAsync(cancellationToken), cancellationToken);
        var manifestPath = settings.Launcher.GetVanillaManifestPath();
        var versionManifest = await Task.Run(() => ManifestHelper.GetMinecraftManifestAsync(manifestPath, cancellationToken), cancellationToken);

        if (versionManifest == null)
            throw new Exception("Failed to load Minecraft version manifest.");
        
        await Custom.InitAsync(settings, versionManifest, cancellationToken);
        await Modpack.InitAsync(versionManifest, cancellationToken);
        await Import.InitAsync(cancellationToken);
    }

    /// <summary>
    /// Sets up reactive/filtering pipelines for all child sub-view-models.
    /// </summary>
    private void SetupPipeline()
    {
        Custom.SetupPipeline();
        Modpack.SetupPipeline();
        Import.SetupPipeline();
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
    
    /// <summary>
    /// Requests the parent view to change the selected create-instance tab.
    /// </summary>
    /// <param name="tab">The tab to switch to (one of <see cref="ECreateInstanceTab"/>).</param>
    [RelayCommand]
    private async Task HandleTabBtn(ECreateInstanceTab tab) => await SwitchTabInteraction.Handle(tab);
    
    #endregion
}