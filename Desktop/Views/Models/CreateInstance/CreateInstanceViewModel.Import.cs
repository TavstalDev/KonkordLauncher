using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.CreateInstance;

public partial class CreateInstanceViewModel_Import : KonkordObservableObject
{
    private  readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(CreateInstanceViewModel_Import));
    private readonly CreateInstanceViewModel _parent;
    
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsSourceFromFile))] private int _selectedImportSourceIndex = 0;
    public bool IsSourceFromFile => SelectedImportSourceIndex == 0;

    [ObservableProperty] private string? _importPath;
    [ObservableProperty] private bool _hasImportPath;
    [ObservableProperty] private string _importPreviewName = "---";
    [ObservableProperty] private string _importPreviewVersion = "---";
    [ObservableProperty] private string _importPreviewModLoader = "---";
    
    public CreateInstanceViewModel_Import(CreateInstanceViewModel parent)
    {
        _parent = parent;
    }
    
    public void SetupPipeline()
    {
        
    }
    
    public async Task InitAsync(CancellationToken cancellationToken = default)
    {
        
    }
    
    #region Commands

    partial void OnImportPathChanged(string? value)
    {
        if (IsSourceFromFile)
            return;

        if (Uri.TryCreate(value, UriKind.Absolute, out _))
        {
            HasImportPath = true;
            if (ImportUrlTextChangedCommand.CanExecute(value))
                ImportUrlTextChangedCommand.Execute(value);
        }
        else
        {
            HasImportPath = false;
        }
    }
    
    [RelayCommand]
    private async Task ChangeImportType(int index) => await _parent.SwitchImportTabInteraction.Handle(index);

    [RelayCommand]
    private async Task SelectFileToImport()
    {
        string? path = await _parent.ShowFileSelectorInteraction.Handle(Unit.Default);
        if (string.IsNullOrEmpty(path))
        {
            HasImportPath = false;
            return;
        }

        if (!File.Exists(path))
        {
            HasImportPath = false;
            return;
        }

        HasImportPath = true;
        ImportPath = path;
        await FetchImportPreviewFromFile();
    }
    
    [RelayCommand]
    private async Task ImportUrlTextChanged(string path) => await FetchImportPreviewFromUrl();

    [RelayCommand]
    private async Task CreateInstanceFromImport()
    {
        // TODO
    }
    #endregion
    
    private async Task FetchImportPreviewFromFile()
    {
        // TODO
    }
    
    private async Task FetchImportPreviewFromUrl()
    {
        // TODO
    }
}