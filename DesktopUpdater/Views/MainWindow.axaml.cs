using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reactive;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;
using Tavstal.KonkordLauncher.DesktopUpdater.Models;

namespace Tavstal.KonkordLauncher.DesktopUpdater.Views;

/// <summary>
/// Represents the main window of the desktop updater application.
/// Handles initialization, update process, and cleanup operations.
/// </summary>
public partial class MainWindow : KonkordWindow<MainViewModel>, IProgressReporter
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(MainWindow));
    private readonly string _tmpDir;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// Sets up the temporary directory and data context for the window.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif

        _tmpDir = Path.Combine(Path.GetTempPath(), "konkordupdater_" + Path.GetRandomFileName());
        if (!Directory.Exists(_tmpDir))
            Directory.CreateDirectory(_tmpDir);
        DataContext = new MainViewModel();
        this.WhenActivated(disposables =>
        {
            DataContext.MinimizeWindowInteraction.RegisterHandler(action =>
            {
                WindowState = WindowState.Minimized;
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.MaximizeWindowInteraction.RegisterHandler(action =>
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
        });
    }

    /// <summary>
    /// Called when the window is opened.
    /// Starts the update process asynchronously.
    /// </summary>
    /// <param name="e">Event arguments for the opened event.</param>
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        Dispatcher.UIThread.Invoke(async () => await StartUpdateProcessAsync());
    }

    /// <summary>
    /// Called when the window is closed.
    /// Cleans up the temporary directory used during the update process.
    /// </summary>
    /// <param name="e">Event arguments for the closed event.</param>
    protected override void OnClosed(EventArgs e)
    {
        if (Directory.Exists(_tmpDir))
            FileSystemHelper.DeleteDirectory(_tmpDir);
        base.OnClosed(e);
    }
    
    private void DragStart_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Start moving the window when left mouse button is pressed
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    /// <summary>
    /// Starts the update process asynchronously.
    /// Handles downloading, extracting, and applying the latest release of the application.
    /// </summary>
    private async Task StartUpdateProcessAsync()
    {
        // TODO: Test on all platforms
        string targetAssetName = string.Empty;
        bool isArm = OSHelper.IsArmBased();
        switch (OSHelper.GetOperatingSystem())
        {
            case EOperatingSystem.Windows:
            {
                targetAssetName = isArm ? "KonkordLauncher_{0}_windows_arm.zip" : "KonkordLauncher_{0}_windows_x64.zip";
                break;
            }
            case EOperatingSystem.Linux:
            {
                targetAssetName = isArm ? "KonkordLauncher_{0}_linux_arm.tar.gz" : "KonkordLauncher_{0}_linux_x64.tar.gz";
                break;
            }
            case EOperatingSystem.MacOS:
            {
                targetAssetName = isArm ? "KonkordLauncher_{0}_mac_arm.tar.gz" : "KonkordLauncher_{0}_mac_x64.tar.gz";
                break;
            }
        }

        // 0. Send http request to GitHub API to get the latest release info
        var response = await HttpHelper.GetStringAsync(KonkordEndpoints.LatestRelease);
        if (string.IsNullOrEmpty(response))
        {
            _logger.Error("Failed to fetch release info from GitHub API.");
            return;
        }

        JObject releaseObject = JObject.Parse(response);
        if (!releaseObject.TryGetValue("assets", out var assetsToken))
        {
            _logger.Error("No assets found in the latest release.");
            return;
        }

        string? version = releaseObject.Value<string>("tag_name")?.TrimStart('v');
        if (string.IsNullOrEmpty(version))
        {
            _logger.Error("Failed to determine the latest version from the release info.");
            return;
        }

        // Insert version into the target asset name
        targetAssetName = string.Format(targetAssetName, version);

        JArray assetsArray = (JArray)assetsToken;
        // Find the target asset
        string? downloadUrl = null;
        foreach (var asset in assetsArray)
        {
            if (asset["name"]?.ToString() == targetAssetName)
            {
                downloadUrl = asset["browser_download_url"]?.ToString() ?? string.Empty;
                break;
            }
        }

        if (string.IsNullOrEmpty(downloadUrl))
        {
            _logger.Error($"No suitable asset found for the current OS and architecture. Asset name: {targetAssetName}");
            return;
        }

        // 1. Download the asset
        var progress = new Progress<double>(p =>
        {
            SetProgress(p);
            var percent = (int)(p * 100);
            if (percent > 100)
                percent = 100; // Cap at 100%
            SetStatusTranslated("updater_downloading", percent);
        });
        string targetFilePath = Path.Combine(_tmpDir, targetAssetName);
        await HttpHelper.DownloadFileAsync(downloadUrl, targetFilePath, progress);

        // 2. Extract the downloaded file to the temporary directory
        SetStatusTranslated("updater_extracting", targetAssetName);
        string targetTempDir = Path.Combine(_tmpDir, "extracted");
        if (targetAssetName.EndsWith(".tar.gz"))
        {
            await using Stream inStream = File.OpenRead(targetFilePath);
            await using Stream gzipStream = new GZipInputStream(inStream);
            using TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.UTF8);
            tarArchive.ExtractContents(targetTempDir);
        }
        else
            ZipFile.ExtractToDirectory(targetFilePath, targetTempDir);

        // Remove Updater from the extracted files
        foreach (var file in Directory.GetFiles(targetTempDir))
        {
            if (file.Contains("Updater"))
                File.Delete(file);
        }

        string tempBinDir = Path.Combine(targetTempDir, "bin");
        if (Directory.Exists(tempBinDir))
        {
            foreach (var file in Directory.GetFiles(tempBinDir))
            {
                if (file.Contains("Updater"))
                    File.Delete(file);
            }
        }

        // 3. Move the extracted files to the application directory
        SetStatusTranslated("updater.applying");
        FileSystemHelper.MoveDirectory(targetTempDir, PathHelper.ApplicationDir, true);

        // 4. Delete the temporary directory
        SetStatusTranslated("updater.finalizing");
        if (Directory.Exists(_tmpDir))
            FileSystemHelper.DeleteDirectory(_tmpDir);

        // 5. Restart the application
        SetStatusTranslated("updater.completed");
        string fileName = "KonkordLauncher";
        if (OSHelper.GetOperatingSystem() == EOperatingSystem.Windows)
            fileName += ".exe";
        else if (OSHelper.GetOperatingSystem() == EOperatingSystem.MacOS)
            fileName += ".app";
        string appPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
        if (!File.Exists(appPath))
        {
            _logger.Error("Failed to restart the launcher.");
            return;
        }

        ProcessStartInfo processInfo = new ProcessStartInfo()
        {
            FileName = appPath,
            UseShellExecute = true,
        };
        Process.Start(processInfo);
        Close();
    }

    #region IProgressReporter Implementation

    /// <summary>
    /// Sets the progress value for the update process.
    /// </summary>
    /// <param name="progress">The progress value as a double.</param>
    public void SetProgress(double progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext is not { } vm)
                return;

            vm.Progress = progress;
        });
    }

    /// <summary>
    /// Sets the status message for the update process.
    /// </summary>
    /// <param name="status">The status message as a string.</param>
    public void SetStatus(string status)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext is not { } vm)
                return;

            vm.ProgressText = status;
        });
    }

    /// <summary>
    /// Sets the translated status message for the update process.
    /// </summary>
    /// <param name="statusKey">The translation key for the status message.</param>
    /// <param name="args">Optional arguments for the translation.</param>
    public void SetStatusTranslated(string statusKey, params object[]? args)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext is not { } vm)
                return;

            vm.ProgressText = TranslationManager.Translate(statusKey, args);
        });
    }

    #endregion
}