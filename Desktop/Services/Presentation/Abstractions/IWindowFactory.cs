using System.Collections.Generic;
using System.Threading;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Views;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

namespace Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;

public interface IWindowFactory
{
    MainWindow CreateMainWindow();
    CreateInstanceWindow CreateCreateInstanceWindow();
    AccountsWindow CreateAccountsWindow();
    JavaSelectorWindow CreateJavaSelectorWindow();
    IconSelectorWindow CreateIconSelectorWindow();
    ProgressWindow CreateProgressWindow(CancellationTokenSource? cancellationTokenSource = null);
    EditInstanceWindow CreateEditInstanceWindow(InstanceModel instance);
    InstanceLogsWindow CreateInstanceLogsWindow(string instanceId);
    ExportWindow CreateExportWindow(Instance instance, EInstanceProvider provider);
    InstanceVersionSelectorWindow CreateInstanceVersionSelectorWindow(Instance instance);
    ResourceDownloadWindow CreateResourceDownloadWindow(Instance instance, EResourceType resourceType);

    ResourceReviewWindow CreateResourceReviewWindow(Instance instance, EResourceType resourceType,
        List<ResourceDownloadModel> resources);
}
