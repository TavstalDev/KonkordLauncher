using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Tavstal.BeaconSDK.Helpers.IO;
using Tavstal.BeaconSDK.Models.Instance;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Services;
using Tavstal.KonkordLauncher.Desktop.Services.Instances;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.CreateInstance;

public partial class CreateInstanceViewModel_Import : KonkordObservableObject
{
    private readonly IKonkordProgress _progress = null!;
    private readonly IEventService _eventService = null!;
    private readonly IHttpService _httpService = null!;
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IBeaconLogger _logger = null!;
    private readonly ModrinthPackageService _modrinthPackageService = null!;
    private readonly IScreenService _screenService = null!;
    private readonly ITranslationService _translationService = null!;
    private readonly IDialogService _dialogService = null!;
    private readonly IFilePickerService _filePickerService = null!;

    public event EventHandler? OnCloseRequested;

    public CreateInstanceViewModel_Import()
    {
        _screenService = null!;
    }

    public CreateInstanceViewModel_Import(IKonkordProgress progress, IScreenService screenService,
        ILauncherStore launcherStore, ITranslationService translationService,
        IHttpService httpService, IEventService eventService, ModrinthPackageService modrinthPackageService,
        IDialogService dialogService, IFilePickerService filePickerService)
    {
        if (Design.IsDesignMode)
            return;

        _progress = progress;
        _screenService = screenService;
        _logger = Program.Services.GetRequiredService<IBeaconLogger<CreateInstanceViewModel_Import>>();
        _httpService = httpService;
        _translationService = translationService;
        _launcherStore = launcherStore;
        _modrinthPackageService = modrinthPackageService;
        _eventService = eventService;
        _dialogService = dialogService;
        _filePickerService = filePickerService;
    }

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

    private async Task ImportFromFileAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(ImportPath) || !File.Exists(ImportPath))
        {
            _logger.LogWarning("Invalid import path specified.");
            await _dialogService.ShowAlertAsync(
                _translationService.Translate("instance.create.import.error.invalid_path.title"),
                _translationService.Translate("instance.create.import.error.invalid_path.message"),
                EAlertType.Error
            );
            return;
        }

        if (await _modrinthPackageService.ImportAsync(ImportPath,
                new Resolution
                {
                    X = (uint)_screenService.ScreenSize.Width, Y = (uint)_screenService.ScreenSize.Height
                },
                null, null, null, _progress, cancellationToken) != null)
        {
            _progress.CloseReporter();
            var instances = await _launcherStore.GetInstancesAsync(cancellationToken);
            _eventService.RaiseInstanceAdded(instances.Last().Id);
            OnCloseRequested?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            _logger.LogWarning("Failed to import instance from file.");
            await _dialogService.ShowAlertAsync(
                _translationService.Translate("instance.create.import.error.import_failed.title"),
                _translationService.Translate("instance.create.import.error.import_failed.message"),
                EAlertType.Error
            );
        }
    }

    private async Task ImportFromUrlAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(ImportPath))
        {
            _logger.LogWarning("Invalid import path specified.");
            await _dialogService.ShowAlertAsync(
                _translationService.Translate("instance.create.import.error.invalid_path.title"),
                _translationService.Translate("instance.create.import.error.invalid_path.message"),
                EAlertType.Error
            );
            return;
        }

        if (!Uri.TryCreate(ImportPath, UriKind.Absolute, out var uri))
            return;

        var fileName = Path.GetFileName(uri.LocalPath);
        var tempPath = Path.Combine(_launcherStore.GetTempDir(), fileName);
        try
        {
            _progress.OpenReporter();
            IProgress<double> progress = new Progress<double>(p =>
            {
                _progress.Report(p, "instance.download.file", "instance", p.ToString("0.00"));
            });

            await _httpService.DownloadFileAsync(ImportPath, tempPath, null, progress, cancellationToken);
            _progress.CloseReporter();

            if (await _modrinthPackageService.ImportAsync(tempPath,
                    new Resolution
                    {
                        X = (uint)_screenService.ScreenSize.Width, Y = (uint)_screenService.ScreenSize.Height
                    },
                    null, null, null, _progress, cancellationToken) != null)
            {
                _progress.CloseReporter();
                var instances = await _launcherStore.GetInstancesAsync(cancellationToken);
                _eventService.RaiseInstanceAdded(instances.Last().Id);
                OnCloseRequested?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _logger.LogWarning("Failed to import instance from url.");
                await _dialogService.ShowAlertAsync(
                    _translationService.Translate("instance.create.import.error.import_failed.title"),
                    _translationService.Translate("instance.create.import.error.import_failed.message"),
                    EAlertType.Error
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import instance from URL:");
            await _dialogService.ShowAlertAsync(
                _translationService.Translate("instance.create.import.error.import_failed.title"),
                _translationService.Translate("instance.create.import.error.import_failed.message"),
                EAlertType.Error
            );
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
    private async Task SelectFileToImport()
    {
        var path = await _filePickerService.OpenFilePickerAsync(_translationService.Translate("common.select.file"),
            ".zip, .mrpack, .json", ["*.zip", "*.mrpack", "*.json"]);
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

        var extension = Path.GetExtension(path);
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
    private async Task ImportUrlTextChanged(string path)
    {
        await FetchImportPreviewFromUrl();
    }

    [RelayCommand]
    private async Task CreateInstanceFromImport()
    {
        if (IsSourceFromFile)
            await ImportFromFileAsync();
        else
            await ImportFromUrlAsync();
    }

    #endregion
}
