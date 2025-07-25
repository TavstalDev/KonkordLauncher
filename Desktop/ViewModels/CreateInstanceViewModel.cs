using System.Collections.ObjectModel;
using ReactiveUI;
using Tavstal.KonkordLauncher.Desktop.Models;

namespace Tavstal.KonkordLauncher.Desktop.ViewModels;

public class CreateInstanceViewModel : ViewModelBase
{
    private static readonly ReverseMarkdown.Converter _converter = new();
    
    #region Custom
    
    private string _searchQuery;
    public string SearchQuery
    {
        get => _searchQuery;
        set => this.RaiseAndSetIfChanged(ref _searchQuery, value);
    }
    
    private bool _showReleases;
    public bool ShowReleases
    {
        get => _showReleases;
        set => this.RaiseAndSetIfChanged(ref _showReleases, value);
    }
    private bool _showSnapshots;
    public bool ShowSnapshots
    {
        get => _showSnapshots;
        set => this.RaiseAndSetIfChanged(ref _showSnapshots, value);
    }
    
    private bool _showAlphas;
    public bool ShowAlphas
    {
        get => _showAlphas;
        set => this.RaiseAndSetIfChanged(ref _showAlphas, value);
    }
    
    private bool _showBetas;
    public bool ShowBetas
    {
        get => _showBetas;
        set => this.RaiseAndSetIfChanged(ref _showBetas, value);
    }
    
    private bool _showExperiments;
    public bool ShowExperiments
    {
        get => _showExperiments;
        set => this.RaiseAndSetIfChanged(ref _showExperiments, value);
    }
    
    public ObservableCollection<VersionModel> Versions { get; set; } =
    [
        new VersionModel
        {
            Version = "1.20.2",
            ReleaseDate = "2023-10-01",
            Type = "Release",
        }
    ];
    #endregion
    
    #region Modpack

    public ObservableCollection<ModPackModel> Modpacks { get; set; } = new();

    
    private ModPackModel? _selectedModpack;
    public ModPackModel? SelectedModpack
    {
        get => _selectedModpack;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedModpack, value);
            if (value != null)
            {
                ModpackPreview = _converter.Convert(value.RawPage);
            }
            else
            {
                ModpackPreview = _converter.Convert("<p>Select a modpack to see its preview.</p>");
            }
        }
    }
    
    private string _modpackPreview = _converter.Convert(@"<p>Select a modpack to see its preview.</p>");
    public string ModpackPreview 
    {
        get => _modpackPreview;
        set => this.RaiseAndSetIfChanged(ref _modpackPreview, value);
    }

    #endregion

    #region Import

    #endregion
}