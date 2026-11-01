using System.Collections.Generic;
using System.Threading;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Views;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

namespace Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;

/// <summary>
///     Creates the windows that the launcher uses.
/// </summary>
public interface IWindowFactory
{
    /// <summary>
    ///     Creates the main window.
    /// </summary>
    /// <returns>The main window.</returns>
    MainWindow CreateMainWindow();
    /// <summary>
    ///     Creates the window for a new instance.
    /// </summary>
    /// <returns>The instance creation window.</returns>
    CreateInstanceWindow CreateCreateInstanceWindow();
    /// <summary>
    ///     Creates the accounts window.
    /// </summary>
    /// <returns>The accounts window.</returns>
    AccountsWindow CreateAccountsWindow();
    /// <summary>
    ///     Creates the Java version selector window.
    /// </summary>
    /// <returns>The Java selector window.</returns>
    JavaSelectorWindow CreateJavaSelectorWindow();
    /// <summary>
    ///     Creates the icon selector window.
    /// </summary>
    /// <returns>The icon selector window.</returns>
    IconSelectorWindow CreateIconSelectorWindow();
    /// <summary>
    ///     Creates a progress window.
    /// </summary>
    /// <param name="cancellationTokenSource">An optional source that can cancel the running task.</param>
    /// <returns>The progress window.</returns>
    ProgressWindow CreateProgressWindow(CancellationTokenSource? cancellationTokenSource = null);
    /// <summary>
    ///     Creates the editor window for an instance.
    /// </summary>
    /// <param name="instance">The instance to edit.</param>
    /// <returns>The instance editor window.</returns>
    EditInstanceWindow CreateEditInstanceWindow(InstanceModel instance);
    /// <summary>
    ///     Creates the log window for an instance.
    /// </summary>
    /// <param name="instanceId">The ID of the instance.</param>
    /// <returns>The instance log window.</returns>
    InstanceLogsWindow CreateInstanceLogsWindow(string instanceId);
    /// <summary>
    ///     Creates the export window for an instance.
    /// </summary>
    /// <param name="instance">The instance to export.</param>
    /// <param name="provider">The package provider to export to.</param>
    /// <returns>The export window.</returns>
    ExportWindow CreateExportWindow(Instance instance, EInstanceProvider provider);
    /// <summary>
    ///     Creates the version selector window for an instance.
    /// </summary>
    /// <param name="instance">The instance whose version can change.</param>
    /// <returns>The version selector window.</returns>
    InstanceVersionSelectorWindow CreateInstanceVersionSelectorWindow(Instance instance);
    /// <summary>
    ///     Creates the resource download window for an instance.
    /// </summary>
    /// <param name="instance">The instance that receives the resources.</param>
    /// <param name="resourceType">The type of resource.</param>
    /// <returns>The resource download window.</returns>
    ResourceDownloadWindow CreateResourceDownloadWindow(Instance instance, EResourceType resourceType);

    /// <summary>
    ///     Creates the window that reviews the selected resources.
    /// </summary>
    /// <param name="instance">The instance that receives the resources.</param>
    /// <param name="resourceType">The type of resource.</param>
    /// <param name="resources">The resources to review.</param>
    /// <returns>The resource review window.</returns>
    ResourceReviewWindow CreateResourceReviewWindow(Instance instance, EResourceType resourceType,
        List<ResourceDownloadModel> resources);
}
