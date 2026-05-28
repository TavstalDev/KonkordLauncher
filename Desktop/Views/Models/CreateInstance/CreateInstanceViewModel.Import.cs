using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.CreateInstance;

public partial class CreateInstanceViewModel_Import : KonkordObservableObject
{
    private  readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(CreateInstanceViewModel_Import));
    private readonly CreateInstanceViewModel _parent;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSourceFromFile))]
    public partial int SelectedImportSourceIndex { get; set; } = 0;

    public bool IsSourceFromFile => SelectedImportSourceIndex == 0;

    [ObservableProperty]
    public partial string? ImportPath { get; set; }

    [ObservableProperty]
    public partial bool HasImportPath { get; set; }

    [ObservableProperty]
    public partial string ImportPreviewName { get; set; } = "---";

    [ObservableProperty]
    public partial string ImportPreviewVersion { get; set; } = "---";

    [ObservableProperty]
    public partial string ImportPreviewModLoader { get; set; } = "---";

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
            _logger.LogWarning("Invalid import path specified.");
            await _parent.ShowAlertDialogInteraction.Handle(new Alert(
                TranslationManager.Translate("instance.create.import.error.invalid_path.title"),
                TranslationManager.Translate("instance.create.import.error.invalid_path.message"),
                EAlertType.Error
            ));
            return;
        }
        
        // TODO: Use service
        // await InstanceHelper.ImportAsync(ImportPath, EInstanceProvider.MODRINTH, App.ScreenResolution, null, null, null, _parent, cancellationToken) != null
        if (true) 
        {
            _parent.CloseReporter();
            var instances = await LauncherHelper.GetInstancesAsync(cancellationToken);
            GlobalEvents.InvokeInstanceAdded(instances.Last().Id);
            await _parent.CloseWindowInteraction.Handle(Unit.Default);
        }
        else
        {
            _logger.LogWarning("Failed to import instance from file.");
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
            _logger.LogWarning("Invalid import path specified.");
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
            
            // TODO: Use service
            // await InstanceHelper.ImportAsync(tempPath, EInstanceProvider.MODRINTH, App.ScreenResolution, null, null, null, _parent,
            // cancellationToken) != null
            if (true)
            {
                _parent.CloseReporter();
                var instances = await LauncherHelper.GetInstancesAsync(cancellationToken);
                GlobalEvents.InvokeInstanceAdded(instances.Last().Id);
                await _parent.CloseWindowInteraction.Handle(Unit.Default);
            }
            else
            {
                _logger.LogWarning("Failed to import instance from url.");
                await _parent.ShowAlertDialogInteraction.Handle(new Alert(
                    TranslationManager.Translate("instance.create.import.error.import_failed.title"),
                    TranslationManager.Translate("instance.create.import.error.import_failed.message"),
                    EAlertType.Error
                ));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to import instance from URL: {ex}");
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