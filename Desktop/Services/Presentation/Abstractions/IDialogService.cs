using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;

namespace Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;

public interface IDialogService
{
    void SetMainWindow(Window mainWindow);

    Task CloseAsync();

    Task<bool> ShowAlertAsync(string title, string message, EAlertType type);

    Task<bool> ShowConfirmAsync(string title, string message, EAlertType type);

    Task ShowAccountsAsync();

    Task<JavaVersionModel?> ShowJavaSelectorAsync();

    Task<string?> ShowInputAsync(string title);

    Task<string?> ShowIconSelectorAsync();

    Task ShowInstanceCreationAsync();

    Task ShowInstanceEditorAsync(InstanceModel instance);

    Task ShowInstanceLogsAsync(string instanceId);

    Task CloseInstanceLogsAsync(string instanceId);

    Task ShowInstanceExportAsync(Instance instance, EInstanceProvider provider);

    Task<bool> ShowInstanceVersionSelectorAsync(Instance instance);

    Task<bool?> ShowResourceDownloaderAsync(Instance instance, EResourceType type);

    Task<bool> ShowResourceReviewAsync(Instance instance, EResourceType type, List<ResourceDownloadModel>  resources);
}
