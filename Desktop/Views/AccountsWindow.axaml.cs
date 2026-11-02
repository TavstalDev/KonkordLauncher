using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Primitives;
using Tavstal.BeaconSDK.Enums;
using Tavstal.BeaconSDK.Models;
using Tavstal.BeaconSDK.Services.Abstractions.Auth;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Translation;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views;

/// <summary>
///     The window used to sign in to and manage launcher accounts.
/// </summary>
public partial class AccountsWindow : KonkordWindow<AccountsViewModel>, IKonkordProgress
{
    private readonly IMicrosoftAuthService _microsoftAuthService = null!;
    private readonly IMicrosoftDeviceAuthService _microsoftDeviceAuthService = null!;
    private readonly IMicrosoftHttpAuthService _microsoftHttpAuthService = null!;
    private readonly ITranslationService _translationService = null!;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AccountsWindow" /> class.
    /// </summary>
    public AccountsWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AccountsWindow" /> class with the
    ///     given view model and services.
    /// </summary>
    /// <param name="viewModel">The view model that controls the window.</param>
    /// <param name="translationService">The service used to translate text.</param>
    /// <param name="microsoftAuthService">The service used to sign in with a Microsoft account.</param>
    /// <param name="microsoftDeviceAuthService">The service used for Microsoft device sign-in.</param>
    /// <param name="microsoftHttpAuthService">The service used to listen for Microsoft sign-in responses.</param>
    /// <param name="bitmapService">The service used to load and release bitmaps.</param>
    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public AccountsWindow(AccountsViewModel viewModel, ITranslationService translationService,
        IMicrosoftAuthService microsoftAuthService, IMicrosoftDeviceAuthService microsoftDeviceAuthService,
        IMicrosoftHttpAuthService microsoftHttpAuthService, IBitmapService bitmapService) : base(bitmapService)
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;

        _translationService = translationService;
        _microsoftAuthService = microsoftAuthService;
        _microsoftDeviceAuthService = microsoftDeviceAuthService;
        _microsoftHttpAuthService = microsoftHttpAuthService;
        DataContext = viewModel;
        OfflineUsernameInput.TextChanged += OfflineUsername_OnTextChanged;
        _microsoftAuthService.OnAuthStatusChanged += OnAuthStatusChanged;

        this.WhenActivated(disposables =>
        {
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
        });
    }

    /// <summary>
    ///     Handles a change in the Microsoft sign-in status and updates the view model on the UI thread.
    /// </summary>
    /// <param name="status">The new sign-in status.</param>
    private void OnAuthStatusChanged(EAuthStatus status)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            if (DataContext == null)
                return;
            DataContext.OnAuthStatusChange(status);
        });
    }

    #region Events

    /// <summary>
    ///     Stops account listeners and removes event handlers when the window closes.
    /// </summary>
    /// <param name="e">The data for the closing event.</param>
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        OfflineUsernameInput.TextChanged -= OfflineUsername_OnTextChanged;
        _microsoftAuthService.OnAuthStatusChanged -= OnAuthStatusChanged;
        Task.Run(async () =>
        {
            await _microsoftHttpAuthService.StopListeningAsync();
            await _microsoftDeviceAuthService.StopListeningAsync();
        });
        base.OnClosing(e);
    }

    /// <summary>
    ///     Keeps only letters, digits, and underscores in the offline username box.
    /// </summary>
    /// <param name="sender">The source of the event, which is expected to be a <see cref="TextBox" />.</param>
    /// <param name="e">The event data.</param>
    private void OfflineUsername_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox)
            return;

        if (textBox.Text == null)
            return;

        var allowed = Regex.Replace(textBox.Text, @"[^A-Za-z0-9_]", "");
        if (textBox.Text != allowed)
        {
            var caret = textBox.CaretIndex;
            textBox.Text = allowed;
            textBox.CaretIndex = Math.Min(caret - 1, allowed.Length);
        }
    }

    #endregion

    #region Progress Reporter

    /// <summary>
    ///     Sets the progress value shown in the window.
    /// </summary>
    /// <param name="progress">The progress value from 0 to 100.</param>
    public void Report(double progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.Progress = progress;
        });
    }

    /// <summary>
    ///     Reports progress for a single file operation.
    /// </summary>
    /// <param name="progress">The progress value from 0 to 100.</param>
    /// <param name="fileName">The name of the file being processed.</param>
    /// <param name="downloading">
    ///     <see langword="true" /> if the file is being downloaded; otherwise, <see langword="false" />.
    /// </param>
    public void Report(double progress, string fileName, bool downloading = true)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.Progress = progress;
            DataContext.ProgressText = _translationService.Translate(
                downloading ? "instance.downloading.file" : "instance.reading.file",
                fileName, progress.ToString("0.00"));
        });
    }

    /// <summary>
    ///     Sets the progress value and a translated status text for an operation step.
    /// </summary>
    /// <param name="kind">The type of the operation step.</param>
    /// <param name="progress">The progress value from 0 to 100.</param>
    public void Report(EProgressKind kind, double progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.Progress = progress;
            DataContext.ProgressText =
                _translationService.Translate(TranslationHelper.GetProgressKey(kind), progress.ToString("0.00"));
        });
    }

    /// <summary>
    ///     Sets the status text from a translation key.
    /// </summary>
    /// <param name="key">The translation key of the status text.</param>
    public void Report(string key)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.ProgressText = _translationService.Translate(key);
        });
    }

    /// <summary>
    ///     Sets the progress value and a translated status text.
    /// </summary>
    /// <param name="progress">The progress value from 0 to 100.</param>
    /// <param name="key">The translation key of the status text.</param>
    /// <param name="args">The values used to fill placeholders in the translation.</param>
    public void Report(double progress, string key, params object[]? args)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.Progress = progress;
            DataContext.ProgressText = _translationService.Translate(key, args);
        });
    }

    /// <summary>
    ///     Does nothing in this window, because it does not show a task count.
    /// </summary>
    /// <param name="count">The expected number of tasks. The value is ignored.</param>
    public void ReportExpectedTask(int? count)
    {
        /* ignored */
    }

    /// <summary>
    ///     Does nothing in this window, because it does not show a task count.
    /// </summary>
    public void ReportTaskCompleted()
    {
        /* ignored */
    }

    #endregion
}
