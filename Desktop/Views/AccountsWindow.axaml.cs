using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Primitives;
using Tavstal.BeaconSDK.Enums;
using Tavstal.BeaconSDK.Models;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.BeaconSDK.Services.Abstractions.Auth;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Translation;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views;

/// <summary>
/// Represents the AccountsWindow, a partial class that serves as the main window for managing accounts.
/// Implements the IProgressReporter interface to handle progress updates.
/// </summary>
public partial class AccountsWindow : KonkordWindow<AccountsViewModel>, IKonkordProgress
{
    private readonly ITranslationService _translationService = null!;
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IMicrosoftAuthService _microsoftAuthService = null!;
    private readonly IMicrosoftDeviceAuthService _microsoftDeviceAuthService = null!;
    private readonly IMicrosoftHttpAuthService _microsoftHttpAuthService = null!;
    private readonly ISkinService _skinService = null!;
    private readonly IEncryptionService _encryptionService = null!;
    private readonly IEventService _eventService = null!;

    /// <summary>
    /// Initializes a new instance of the AccountsWindow class.
    /// Sets up the DataContext and attaches developer tools in debug mode.
    /// </summary>
    [RequiresUnreferencedCode( "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public AccountsWindow()
    {
        InitializeComponent();
        DataContext = new AccountsViewModel(Program.Services.GetRequiredService<IBeaconLogger<AccountsViewModel>>(), _launcherStore,
            _translationService,  _microsoftAuthService, _microsoftDeviceAuthService, _microsoftHttpAuthService,
            _skinService, _encryptionService, _eventService);
        OfflineUsernameInput.TextChanged += OfflineUsername_OnTextChanged;

        if (Design.IsDesignMode)
            return;

        this.WhenActivated(disposables =>
        {
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.ShowAlertDialog.RegisterHandler(async action =>
            {
                AlertWindow alertWindow = new(action.Input.Title, action.Input.Message, action.Input.Type);
                await alertWindow.ShowDialog(this);
                action.SetOutput(Unit.Default);
            }).DisposeWith(disposables);
            DataContext.SetClipboardText.RegisterHandler(async action =>
            {
                await SetClipboardTextAsync(action.Input);
                action.SetOutput(Unit.Default);
            }).DisposeWith(disposables);
        });
    }

    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public AccountsWindow(ITranslationService translationService, ILauncherStore launcherStore, IMicrosoftAuthService microsoftAuthService,
        IMicrosoftDeviceAuthService microsoftDeviceAuthService, IMicrosoftHttpAuthService microsoftHttpAuthService,
        ISkinService skinService, IEncryptionService encryptionService, IEventService eventService) : this()
    {
        _translationService = translationService;
        _launcherStore = launcherStore;
        _microsoftAuthService = microsoftAuthService;
        _microsoftDeviceAuthService = microsoftDeviceAuthService;
        _microsoftHttpAuthService = microsoftHttpAuthService;
        _skinService = skinService;
        _encryptionService = encryptionService;
        _eventService = eventService;
        _microsoftAuthService.OnAuthStatusChanged += OnAuthStatusChanged;
    }

    #region Events

    /// <summary>
    /// Handles the cleanup and resource deallocation when the window is closing.
    /// Unsubscribes from events and stops any active listeners to ensure proper disposal.
    /// </summary>
    /// <param name="e">Provides data for the window closing event.</param>
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
    /// Handles the text changed event for the offline username input field.
    /// Ensures that the input contains only alphanumeric characters and underscores.
    /// If invalid characters are detected, they are removed, and the caret position is adjusted accordingly.
    /// </summary>
    /// <param name="sender">The source of the event, expected to be a TextBox.</param>
    /// <param name="e">The event data.</param>
    private void OfflineUsername_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox)
            return;

        if (textBox.Text == null)
            return;

        string allowed = Regex.Replace(textBox.Text, @"[^A-Za-z0-9_]", "");
        if (textBox.Text != allowed)
        {
            int caret = textBox.CaretIndex;
            textBox.Text = allowed;
            textBox.CaretIndex = Math.Min(caret - 1, allowed.Length);
        }
    }
    #endregion

    /// <summary>
    /// Handles changes in the authentication status for Microsoft accounts.
    /// Updates the UI and performs necessary actions based on the new status.
    /// </summary>
    /// <param name="status">The new authentication status.</param>
    private void OnAuthStatusChanged(EAuthStatus status)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            if (DataContext == null)
                return;
            DataContext.OnAuthStatusChange(status);
        });
    }

    #region Progress Reporter

    public void Report(double progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.Progress = progress;
        });
    }

    public void Report(double progress, string fileName, bool downloading = true)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.Progress = progress;
            // TODO
        });
    }

    public void Report(EProgressKind kind, double progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.Progress = progress;
            DataContext.ProgressText = _translationService.Translate(TranslationHelper.GetProgressKey(kind), progress.ToString("0.00"));
        });
    }

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

    public void ReportExpectedTask(int? count) { /* ignored */ }

    public void ReportTaskCompleted() { /* ignored */ }

    #endregion
}
