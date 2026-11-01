using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;

namespace Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;

/// <summary>
///     Shows the launcher windows and dialogs.
/// </summary>
public interface IDialogService
{
    /// <summary>
    ///     Sets the main window that owns the dialogs.
    /// </summary>
    /// <param name="mainWindow">The main window.</param>
    void SetMainWindow(Window mainWindow);

    /// <summary>
    ///     Closes the main window.
    /// </summary>
    /// <returns>A task that completes when the window is closed.</returns>
    Task CloseAsync();

    /// <summary>
    ///     Shows an alert dialog.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The message to show.</param>
    /// <param name="type">The type of alert.</param>
    /// <returns>The dialog result.</returns>
    Task<bool> ShowAlertAsync(string title, string message, EAlertType type);

    /// <summary>
    ///     Shows a confirmation dialog.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The message to show.</param>
    /// <param name="type">The type of alert.</param>
    /// <returns>The dialog result.</returns>
    Task<bool> ShowConfirmAsync(string title, string message, EAlertType type);

    /// <summary>
    ///     Shows the accounts window.
    /// </summary>
    /// <returns>A task that completes when the window is closed.</returns>
    Task ShowAccountsAsync();

    /// <summary>
    ///     Shows the Java version selector.
    /// </summary>
    /// <returns>The selected Java version, or <see langword="null" /> if nothing was selected.</returns>
    Task<JavaVersionModel?> ShowJavaSelectorAsync();

    /// <summary>
    ///     Asks the user for text.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <returns>The entered text, or <see langword="null" /> if nothing was entered.</returns>
    Task<string?> ShowInputAsync(string title);

    /// <summary>
    ///     Shows the icon selector.
    /// </summary>
    /// <returns>The selected icon path, or <see langword="null" /> if nothing was selected.</returns>
    Task<string?> ShowIconSelectorAsync();

    /// <summary>
    ///     Shows the window for creating an instance.
    /// </summary>
    /// <returns>A task that completes when the window is closed.</returns>
    Task ShowInstanceCreationAsync();

    /// <summary>
    ///     Shows the editor for an instance. If the editor is already open, it is brought to the front.
    /// </summary>
    /// <param name="instance">The instance to edit.</param>
    /// <returns>A task that completes when the window is shown.</returns>
    Task ShowInstanceEditorAsync(InstanceModel instance);

    /// <summary>
    ///     Shows the log window for an instance.
    /// </summary>
    /// <param name="instanceId">The ID of the instance.</param>
    /// <returns>A task that completes when the window is shown.</returns>
    Task ShowInstanceLogsAsync(string instanceId);

    /// <summary>
    ///     Closes the log window of an instance.
    /// </summary>
    /// <param name="instanceId">The ID of the instance.</param>
    /// <returns>A task that completes when the window is closed.</returns>
    Task CloseInstanceLogsAsync(string instanceId);

    /// <summary>
    ///     Shows the export window for an instance.
    /// </summary>
    /// <param name="instance">The instance to export.</param>
    /// <param name="provider">The package provider to export to.</param>
    /// <returns>A task that completes when the window is closed.</returns>
    Task ShowInstanceExportAsync(Instance instance, EInstanceProvider provider);

    /// <summary>
    ///     Shows the version selector for an instance.
    /// </summary>
    /// <param name="instance">The instance whose version can change.</param>
    /// <returns>The dialog result.</returns>
    Task<bool> ShowInstanceVersionSelectorAsync(Instance instance);

    /// <summary>
    ///     Shows the resource download window for an instance.
    /// </summary>
    /// <param name="instance">The instance that receives the resources.</param>
    /// <param name="type">The type of resource to download.</param>
    /// <returns>The dialog result, or <see langword="null" /> if there is none.</returns>
    Task<bool?> ShowResourceDownloaderAsync(Instance instance, EResourceType type);

    /// <summary>
    ///     Shows the window that reviews the selected resources.
    /// </summary>
    /// <param name="instance">The instance that receives the resources.</param>
    /// <param name="type">The type of resource.</param>
    /// <param name="resources">The resources to review.</param>
    /// <returns>The dialog result.</returns>
    Task<bool> ShowResourceReviewAsync(Instance instance, EResourceType type, List<ResourceDownloadModel> resources);
}
