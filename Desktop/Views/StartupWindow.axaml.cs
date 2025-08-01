using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.ViewModels;

namespace Tavstal.KonkordLauncher.Desktop.Views;

public partial class StartupWindow : Window, IProgressReporter
{
    private readonly IClassicDesktopStyleApplicationLifetime _desktopLifetime;
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(StartupWindow));
    private readonly int _stepDelay = 100; // Delay in milliseconds for each validation step

    public StartupWindow(IClassicDesktopStyleApplicationLifetime desktopLifetime)
    {
        InitializeComponent();
        _desktopLifetime = desktopLifetime;

#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif

        this.DataContext = new StartupViewModel();
    }

    private void Window_OnLoaded(object? sender, RoutedEventArgs e)
    {
        Task.Run(ValidateAsync);
    }

    private async Task ValidateAsync()
    {
        // 0. Set initial status
        SetStatusTranslated("startup.progress.initializing");
        
        // 1. Validate Directory Structure
        SetStatusTranslated("startup.validation.dataFolder");
        await Task.Delay(_stepDelay);
        if (!ValidationHelper.ValidateDataFolder())
        {
            SetStatusTranslated("startup.validation.dataFolderFailed");
            return;
        }
        
        // 2. Validate Settings
        SetStatusTranslated("startup.validation.settings");
        await Task.Delay(_stepDelay);
        if (!await ValidationHelper.ValidateSettings())
        {
            SetStatusTranslated("startup.validation.settingsFailed");
            return;
        }
        
        // 3. Validate Translations
        SetStatusTranslated("startup.validation.translations");
        await Task.Delay(_stepDelay);
        await TranslationManager.InitializeTranslations();
        
        // 5. Validate Accounts
        SetStatusTranslated("startup.validation.accounts");
        await Task.Delay(_stepDelay);
        if (!await ValidationHelper.ValidateAccounts())
        {
            SetStatusTranslated("startup.validation.accountsFailed");
            return;
        }
        
        // 7. Validate Manifests
        SetStatusTranslated("startup.validation.manifests");
        await Task.Delay(_stepDelay);
        if (!await ValidationHelper.ValidateManifests())
        {
            SetStatusTranslated("startup.validation.manifestsFailed");
            return;
        }
        
        // 8. Validate Java
        SetStatusTranslated("startup.validation.java");
        await Task.Delay(_stepDelay);
        if (!JavaHelper.IsJavaInstalled())
        {
            SetStatusTranslated("startup.validation.javaFailed");
            return;
        }

        
        // 9. Check for Updates
        SetStatusTranslated("startup.progress.checking");
        await Task.Delay(_stepDelay);
        // TODO:
        // Check if the launcher is up to date.
        // If not, download the update
        // then start the console app to replace the current executable.
        // After that, restart the launcher

        Dispatcher.UIThread.Post(() =>
        {
            _desktopLifetime.MainWindow = new MainWindow();
            _desktopLifetime.MainWindow.Show();
            this.Close();
        });
    }

    public void SetProgress(double progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (this.DataContext is not StartupViewModel viewModel)
                return;

            viewModel.Progress = progress;
        });
    }

    public void SetStatus(string status)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (this.DataContext is not StartupViewModel viewModel)
                return;

            viewModel.ProgressText = status;
        });
    }

    public void SetStatusTranslated(string statusKey, params object[]? args)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (this.DataContext is not StartupViewModel viewModel)
                return;

            viewModel.ProgressText = TranslationManager.Translate(statusKey, args);
        });
    }
}