using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Desktop.Helpers;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

public partial class CreateInstanceViewModel : ObservableObject
{
    private readonly VersionManifest _vanillaManifest;
    private readonly ReverseMarkdown.Converter _converter = new();

    #region Custom
    
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))] private string _instanceName = string.Empty;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))] private string _instanceGroup = string.Empty;
    [ObservableProperty]  private string? _instanceIconPath;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))] private Bitmap? _instanceIcon;

    /// <summary>
    /// Gets a value indicating whether a custom instance can be created.
    /// Returns true if all required fields (instance name, icon, selected Minecraft version,
    /// and, if applicable, selected mod loader) are set; otherwise, false.
    /// </summary>
    public bool CanCreateCustomInstance
    {
        get
        {
            if (string.IsNullOrEmpty(InstanceName))
                return false;
            
            if (InstanceIcon == null)
                return false;
            
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (SelectedMinecraftVersion == null)
                return false;
            
            if (ModLoaderType != EMinecraftKind.VANILLA && SelectedModLoader == null)
                return false;
            
            return true;
        }
    }
    
    #region Vanilla
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(VersionResult))] private string _searchQuery = string.Empty;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(VersionResult))] private bool _showReleases = true;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(VersionResult))] private bool _showSnapshots;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(VersionResult))] private bool _showAlphas;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(VersionResult))] private bool _showBetas;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(VersionResult))] private bool _showExperiments;
    
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))] [NotifyPropertyChangedFor(nameof(ModLoaderVersionResult))] private MinecraftVersion? _selectedMinecraftVersion;

    /// <summary>
    /// Gets a filtered list of available Minecraft versions based on the current search query and selected version types.
    /// Filters include release, snapshot, old alpha, old beta, and experiment versions,
    /// depending on the corresponding boolean properties.
    /// </summary>
    public ObservableCollection<MinecraftVersion> VersionResult
    {
        get
        {
            var versions = _vanillaManifest.Versions.FindAll(x =>
                (string.IsNullOrEmpty(SearchQuery) || x.Id.StartsWith(SearchQuery)) &&
                (x.Type != "release" || ShowReleases) &&
                (x.Type != "snapshot" || ShowSnapshots) &&
                (x.Type != "old_alpha" || ShowAlphas) &&
                (x.Type != "old_beta" || ShowBetas) &&
                (x.Type != "experiment" || ShowExperiments));

            return new ObservableCollection<MinecraftVersion>(versions.Count == 0 ? [] : versions);
        }
    }
    #endregion
    #region  Mod Loader
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ModLoaderVersionResult))] private string _modLoaderSearchQuery = string.Empty;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ModLoaderVersionResult))] [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))] private EMinecraftKind _modLoaderType = EMinecraftKind.VANILLA;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))] private IModManifest? _selectedModLoader;
    
    /// <summary>
    /// Gets a filtered list of available mod loader versions based on the selected mod loader type,
    /// the currently selected Minecraft version, and the mod loader search query.
    /// For each mod loader type (NeoForge, Forge, Fabric, Quilt), retrieves the corresponding manifest,
    /// filters by the selected Minecraft version and search query, and returns the matching results.
    /// Returns an empty list for Vanilla or if no matching versions are found.
    /// </summary>
    public List<IModManifest> ModLoaderVersionResult
    {
        get
        {
            if (SelectedMinecraftVersion == null)
                return [];
            
            List<IModManifest> result = [];
            switch (ModLoaderType)
            {
                case EMinecraftKind.VANILLA:
                    break;
                case EMinecraftKind.NEOFORGE:
                {
                    var data = ManifestHelper.GetNeoForgeManifest();
                    if (data == null)
                        return [];
                    foreach (var version in data)
                    {
                        if (version.GameVersion != SelectedMinecraftVersion.Id)
                            continue;
                        
                        if (!string.IsNullOrEmpty(ModLoaderSearchQuery) || !version.Version.StartsWith(ModLoaderSearchQuery))
                            continue;
                        
                        result.Add(version);
                    }
                    break;
                }
                case EMinecraftKind.FORGE:
                {
                    var data = ManifestHelper.GetForgeManifest();
                    if (data == null)
                        return [];
                    foreach (var version in data)
                    {
                        if (version.GameVersion != SelectedMinecraftVersion.Id)
                            continue;
                        
                        if (!string.IsNullOrEmpty(ModLoaderSearchQuery) || !version.Version.StartsWith(ModLoaderSearchQuery))
                            continue;
                        
                        result.Add(version);
                    }
                    break;
                }
                case EMinecraftKind.FABRIC:
                {
                    result = ManifestHelper.GetFabricManifest()!;
                    if (result == null)
                        return [];
                    
                    if (!string.IsNullOrEmpty(ModLoaderSearchQuery))
                        result = result.FindAll(x => x.Version.StartsWith(ModLoaderSearchQuery));
                    break;
                }
                case EMinecraftKind.QUILT:
                {
                    result = ManifestHelper.GetQuiltManifest()!;
                    if (result == null)
                        return [];
                    
                    if (!string.IsNullOrEmpty(ModLoaderSearchQuery))
                        result = result.FindAll(x => x.Version.StartsWith(ModLoaderSearchQuery));
                    break;
                }
            }
            
            return result;
        }
    }
    #endregion
    #endregion
    
    #region Modpack

    [ObservableProperty] private ObservableCollection<ModPackModel> _modpacks;
    
    [ObservableProperty] private EPlatformType _selectedModpackPlatform = EPlatformType.Modrinth;
    
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ModpackPreview))] private ModPackModel? _selectedModpack;
    
    public List<EPlatformType> AvailableModpackPlatforms =>
    [
        EPlatformType.Modrinth,
        EPlatformType.CurseForge,
        EPlatformType.Technic,
        EPlatformType.FTB
    ];
    
    public string ModpackPreview => SelectedModpack == null ? _converter.Convert(@"<p>Select a modpack to see its preview.</p>") : _converter.Convert(SelectedModpack.RawPage);

    #endregion

    #region Import

    #endregion

    public CreateInstanceViewModel()
    {
        InstanceIcon = ImageHelper.Load("avares://Desktop/Assets/Icons/dirt.png").Result;
        if (Design.IsDesignMode)
            return;
        
        _vanillaManifest = ManifestHelper.GetMinecraftManifest()!;
        _selectedMinecraftVersion = VersionResult.First();
    }
}