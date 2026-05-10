using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Network;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;

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
        
        string extension = Path.GetExtension(path);
        if (!(extension == ".zip" || extension == ".mrpack"))
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
        if (IsSourceFromFile)
            await ImportFromFileAsync();
        else
            await ImportFromUrlAsync();
    }
    #endregion

    private async Task ImportFromFileAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(ImportPath) || !File.Exists(ImportPath))
        {
            _logger.Warn("Invalid import path specified.");
            await _parent.ShowAlertDialogInteraction.Handle(new Alert(
                TranslationManager.Translate("instance.create.import.error.invalid_path.title"),
                TranslationManager.Translate("instance.create.import.error.invalid_path.message"),
                EAlertType.Error
            ));
            return;
        }
        
        if (await InstanceHelper.ImportAsync(ImportPath, EInstanceProvider.Modrinth, App.ScreenResolution, null, null, _parent, cancellationToken) != null)
        {
            _parent.CloseReporter();
            GlobalEvents.InvokeInstancesChanged();
            await _parent.CloseWindowInteraction.Handle(Unit.Default);
        }
        else
        {
            _logger.Warn("Failed to import instance from file.");
            await _parent.ShowAlertDialogInteraction.Handle(new Alert(
                TranslationManager.Translate("instance.create.import.error.import_failed.title"),
                TranslationManager.Translate("instance.create.import.error.import_failed.message"),
                EAlertType.Error
            ));
        }
    }

    private async Task ImportFromUrlAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(ImportPath))
        {
            _logger.Warn("Invalid import path specified.");
            await _parent.ShowAlertDialogInteraction.Handle(new Alert(
                TranslationManager.Translate("instance.create.import.error.invalid_path.title"),
                TranslationManager.Translate("instance.create.import.error.invalid_path.message"),
                EAlertType.Error
            ));
            return;
        }

        if (!Uri.TryCreate(ImportPath, UriKind.Absolute, out var uri))
            return;
        
        string fileName = Path.GetFileName(uri.LocalPath);
        string tempPath = Path.Combine(PathHelper.TempDir, fileName);
        try
        {
            _parent.OpenReporter();
            IProgress<double> progress = new Progress<double>(p =>
            {
                _parent.ReportProgress(p);
                _parent.UpdateStatusTranslated("instance.download.file", "instance", p.ToString("0.00"));
            });

            await HttpHelper.DownloadFileAsync(ImportPath, tempPath, progress, cancellationToken);
            _parent.CloseReporter();
            
            if (await InstanceHelper.ImportAsync(tempPath, EInstanceProvider.Modrinth, App.ScreenResolution, null, null, _parent,
                    cancellationToken) != null)
            {
                _parent.CloseReporter();
                GlobalEvents.InvokeInstancesChanged();
                await _parent.CloseWindowInteraction.Handle(Unit.Default);
            }
            else
            {
                _logger.Warn("Failed to import instance from url.");
                await _parent.ShowAlertDialogInteraction.Handle(new Alert(
                    TranslationManager.Translate("instance.create.import.error.import_failed.title"),
                    TranslationManager.Translate("instance.create.import.error.import_failed.message"),
                    EAlertType.Error
                ));
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to import instance from URL: {ex}");
            await _parent.ShowAlertDialogInteraction.Handle(new Alert(
                TranslationManager.Translate("instance.create.import.error.import_failed.title"),
                TranslationManager.Translate("instance.create.import.error.import_failed.message"),
                EAlertType.Error
            ));
        }
        finally
        {
            if (File.Exists(tempPath))
                FileSystemHelper.DeleteFile(tempPath);
        }
    }
    
    private async Task FetchImportPreviewFromFile()
    {
        // TODO
    }
    
    private async Task FetchImportPreviewFromUrl()
    {
        // TODO
    }
}