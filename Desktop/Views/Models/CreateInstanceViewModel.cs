using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    
    [ObservableProperty] private string _instanceName = string.Empty;
    [ObservableProperty] private string _instanceGroup = string.Empty;
    [ObservableProperty] private Bitmap? _instanceIcon;
    
    #region Vanilla
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(VersionResult))] private string _searchQuery = string.Empty;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(VersionResult))] private bool _showReleases = true;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(VersionResult))] private bool _showSnapshots;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(VersionResult))] private bool _showAlphas;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(VersionResult))] private bool _showBetas;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(VersionResult))] private bool _showExperiments;

    public List<MinecraftVersion> VersionResult => _vanillaManifest.Versions.FindAll(x =>
        (string.IsNullOrEmpty(SearchQuery) || x.Id.Contains(SearchQuery)) &&
        (x.Type != "release" || ShowReleases) &&
        (x.Type != "snapshot" || ShowSnapshots) &&
        (x.Type != "old_alpha" || ShowAlphas) &&
        (x.Type != "old_beta" || ShowBetas) &&
        (x.Type != "experiment" || ShowExperiments));
    
    [ObservableProperty] private MinecraftVersion _selectedMinecraftVersion;
    #endregion
    #region  Mod Loader
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ModVersionResult))] private string _modSearchQuery = string.Empty;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ModVersionResult))] private EMinecraftKind _modType = EMinecraftKind.VANILLA;

    public List<IModManifest> ModVersionResult
    {
        get
        {
            List<IModManifest> result = [];
            switch (ModType)
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
                        
                        if (!string.IsNullOrEmpty(ModSearchQuery) || !version.Version.Contains(ModSearchQuery))
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
                        
                        if (!string.IsNullOrEmpty(ModSearchQuery) || !version.Version.Contains(ModSearchQuery))
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
                    
                    if (!string.IsNullOrEmpty(ModSearchQuery))
                        result = result.FindAll(x => x.Version.Contains(ModSearchQuery));
                    break;
                }
                case EMinecraftKind.QUILT:
                {
                    result = ManifestHelper.GetQuiltManifest()!;
                    if (result == null)
                        return [];
                    
                    if (!string.IsNullOrEmpty(ModSearchQuery))
                        result = result.FindAll(x => x.Version.Contains(ModSearchQuery));
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
    
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ModpackPreview))] private ModPackModel? _selectedModpack;
    
    public string ModpackPreview => SelectedModpack == null ? _converter.Convert(@"<p>Select a modpack to see its preview.</p>") : _converter.Convert(SelectedModpack.RawPage);

    #endregion

    #region Import

    #endregion

    public CreateInstanceViewModel()
    {
        _vanillaManifest = ManifestHelper.GetMinecraftManifest()!;
        _selectedMinecraftVersion = VersionResult.First();
        InstanceIcon = ImageHelper.Load("avares://Desktop/Assets/Icons/dirt.png").Result;
    }
}