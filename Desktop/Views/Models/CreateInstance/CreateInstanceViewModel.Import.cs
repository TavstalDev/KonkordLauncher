using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Common.Services.Implementations;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.CreateInstance;

public partial class CreateInstanceViewModel_Import : KonkordObservableObject
{
    private readonly ICustomLogger _logger;
    private readonly IHttpService _httpService;
    private readonly ITranslationService _translationService;
    private readonly ILauncherStore _launcherStore;
    private readonly ModrinthPackageService _modrinthPackageService;
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
        if (Design.IsDesignMode)
            return;
        
        var services = Program.ServiceProvider;
        _logger = services.GetRequiredService<ICustomLogger<CreateInstanceViewModel_Import>>();
        _httpService = services.GetRequiredService<IHttpService>();
        _translationService = services.GetRequiredService<ITranslationService>();
        _launcherStore = services.GetRequiredService<ILauncherStore>();
        _modrinthPackageService = services.GetRequiredService<ModrinthPackageService>();
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
                _translationService.Translate("instance.create.import.error.invalid_path.title"),
                _translationService.Translate("instance.create.import.error.invalid_path.message"),
                EAlertType.Error
            ));
            return;
        }
        
        if (await _modrinthPackageService.ImportAsync(ImportPath, App.ScreenResolution, null, null, null, _parent, cancellationToken) != null ) 
        {
            _parent.CloseReporter();
            var instances = await _launcherStore.GetInstancesAsync(cancellationToken);
            GlobalEvents.InvokeInstanceAdded(instances.Last().Id);
            await _parent.CloseWindowInteraction.Handle(Unit.Default);
        }
        else
        {
            _logger.LogWarning("Failed to import instance from file.");
            await _parent.ShowAlertDialogInteraction.Handle(new Alert(
                _translationService.Translate("instance.create.import.error.import_failed.title"),
                _translationService.Translate("instance.create.import.error.import_failed.message"),
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
                _translationService.Translate("instance.create.import.error.invalid_path.title"),
                _translationService.Translate("instance.create.import.error.invalid_path.message"),
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

            await _httpService.DownloadFileAsync(ImportPath, tempPath, progress, cancellationToken);
            _parent.CloseReporter();
            
            if (await _modrinthPackageService.ImportAsync(tempPath, App.ScreenResolution, null, null, null, _parent, cancellationToken) != null)
            {
                _parent.CloseReporter();
                var instances = await _launcherStore.GetInstancesAsync(cancellationToken);
                GlobalEvents.InvokeInstanceAdded(instances.Last().Id);
                await _parent.CloseWindowInteraction.Handle(Unit.Default);
            }
            else
            {
                _logger.LogWarning("Failed to import instance from url.");
                await _parent.ShowAlertDialogInteraction.Handle(new Alert(
                    _translationService.Translate("instance.create.import.error.import_failed.title"),
                    _translationService.Translate("instance.create.import.error.import_failed.message"),
                    EAlertType.Error
                ));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to import instance from URL:");
            await _parent.ShowAlertDialogInteraction.Handle(new Alert(
                _translationService.Translate("instance.create.import.error.import_failed.title"),
                _translationService.Translate("instance.create.import.error.import_failed.message"),
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