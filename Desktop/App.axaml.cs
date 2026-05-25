using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using DiscordRPC;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Views;

namespace Tavstal.KonkordLauncher.Desktop;

/// <summary>
/// Represents the main application class for the Konkord Launcher desktop application.
/// </summary>
// ReSharper disable once PartialTypeWithSinglePart - Avalonia code generation
public partial class App : Application
{
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(App));
    // ReSharper disable once NotAccessedField.Local - Used to ensure the task is not garbage collected before completion
    private static Task? _initializeTask;
    private static DiscordRpcClient? _rpcClient;

    #region Screen Size
    private static PixelSize _screenSize = new(1920, 1080);
    public static PixelSize ScreenSize => _screenSize;
    private static Resolution _screenResolution = new(1920, 1080);
    public static Resolution ScreenResolution => _screenResolution;
    
    public static decimal ScreenWidth => _screenSize.Width;
    public static decimal ScreenHeight => _screenSize.Height;
    public static void SetScreenSize(PixelSize screenSize)
    {
        _screenSize = screenSize;
        _screenResolution.X = (uint)(0.40 * screenSize.Width); 
        _screenResolution.Y = (uint)(0.45 * screenSize.Height);
    }
    #endregion

    #region Versioning
    private static string _version = string.Empty;
    public static string Version
    {
        get
        {
            if (!string.IsNullOrEmpty(_version))
                return _version;
            
            Version? currentVersion;
            object[] versionAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
            if (versionAttributes.Length > 0)
            {
                AssemblyInformationalVersionAttribute informationalVersionAttribute = (AssemblyInformationalVersionAttribute)versionAttributes[0];
                currentVersion = new Version(informationalVersionAttribute.InformationalVersion);
            }
            else
                currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            
            _version = currentVersion?.ToString() ?? "2.0.0";
            return _version;
        }
    }
    public static string Branch
    {
        get
        {
#if  DEBUG
            return "dev";
#else 
            return "stable";
#endif
        }   
    }
    private static string _buidDate = string.Empty;
    public static string BuildDate
    {
        get
        {
            if (!string.IsNullOrEmpty(_buidDate))
                return _buidDate;
            
            object[] buildDateAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyMetadataAttribute), false);
            foreach (var attribute in buildDateAttributes)
            {
                if (attribute is AssemblyMetadataAttribute { Key: "BuildDate" } metadata)
                {
                    _buidDate = metadata.Value ?? DateTime.UtcNow.ToString("yyyy-MM-dd");
                    return _buidDate;
                }
            }

            _buidDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
            return _buidDate;
        }
    }
    #endregion
    
    /// <summary>
    /// Initializes the application by loading XAML resources.
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        GlobalEvents.OnThemeChanged += ApplyTheme;
        _initializeTask = InitAsync();
    }

    /// <summary>
    /// Initializes launcher runtime state by loading persisted settings, applying the selected UI theme,
    /// and configuring Discord Rich Presence with an initial "Starting..." status.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the asynchronous settings load operation.</param>
    private async Task InitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = await LauncherHelper.GetLauncherSettingsAsync(ScreenResolution, cancellationToken);
            ApplyTheme(settings.Launcher.Theme);
            
            Directory.CreateDirectory(PathHelper.TempDir);

            if (!Design.IsDesignMode)
            {
                _rpcClient = new DiscordRpcClient("1178002101561995416");
                _rpcClient.Initialize();
                _rpcClient.SetPresence(new RichPresence
                {
                    Details = "Starting...",
                    Timestamps = Timestamps.Now,
                    Assets = new Assets
                    {
                        LargeImageKey = "logo",
                        LargeImageText = "Konkord Launcher",
                    }
                });
            }
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    /// Configures the application after the framework initialization is completed.
    /// Sets up the main window for the desktop-style application lifetime.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        // Archive existing log before starting a new session
        try
        {
            string logPath = Path.Combine(PathHelper.LauncherLogsDir, PathHelper.LatestLog);
            if (File.Exists(logPath))
            {
                var lastModified = File.GetLastWriteTime(logPath);
                string archivePath = Path.Combine(PathHelper.LauncherLogsDir, string.Format(PathHelper.LogsFileFormat, lastModified) + ".gz");
                FileSystemHelper.CompressFile(logPath, archivePath);
            }
        }
        catch (Exception)
        {
            // ignored
        }
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Sets the main window to the StartupWindow, passing the application lifetime.
            desktop.MainWindow = new StartupWindow();
#if DEBUG
            // This is the new alternative of attaching developer tools
            this.AttachDeveloperTools(); 
#endif
            
            desktop.ShutdownRequested += (_, _) =>
            {
                try
                {
                    FileSystemHelper.DeleteDirectory(PathHelper.TempDir);
                }
                catch (Exception)
                {
                    // ignored
                }
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    /// <summary>
    /// Applies the specified theme to the application by setting the requested theme variant.
    /// </summary>
    /// <param name="theme">The theme to apply, either Light or Dark.</param>
    private void ApplyTheme(EThemeType theme)
    {
        try
        {
            if (Current == null)
                return;
            switch (theme)
            {
                case EThemeType.LIGHT:
                {
                    RequestedThemeVariant = ThemeVariant.Light;
                    break;
                }
                case EThemeType.DARK:
                {
                    RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Exc("Failed to apply theme");
            _logger.Error(ex);
        }
    }
    
    /// <summary>
    /// Updates the Discord Rich Presence (RPC) with the specified details.
    /// </summary>
    /// <param name="details">The details to display in the Discord Rich Presence.</param>
    public static void UpdateRPC(string details)
    {
        try
        {
            if (Design.IsDesignMode)
                return;
            
            // Check if the Discord RPC client is initialized and not disposed.
            if (_rpcClient == null || _rpcClient.IsDisposed)
            {
                _logger.Error("Discord RPC client is not initialized or disposed.");
                return;
            }
        
            // Set the presence with the provided details and current timestamps.
            _rpcClient.SetPresence(new RichPresence
            {
                Details = details,
                Timestamps = _rpcClient.CurrentPresence?.Timestamps ?? Timestamps.Now
            });
        }
        catch (Exception ex)
        {
            // Log any exceptions that occur during the update process.
            _logger.Exc("Failed to update Discord RPC");
            _logger.Error(ex);
        }
    }

    /// <summary>
    /// Clears the Discord Rich Presence (RPC) by removing the current presence.
    /// </summary>
    public static void ClearRPC()
    {
        try
        {
            if (Design.IsDesignMode)
                return;
            
            // Check if the Discord RPC client is initialized and not disposed.
            if (_rpcClient == null || _rpcClient.IsDisposed)
            {
                _logger.Error("Discord RPC client is not initialized or disposed.");
                return;
            }

            // Clear the presence asynchronously.
            Task.Run(() =>
            {
                _rpcClient.SetPresence(null);
                _rpcClient.Invoke();
            });
        }
        catch (Exception ex)
        {
            // Log any exceptions that occur during the clearing process.
            _logger.Exc("Failed to clear Discord RPC");
            _logger.Error(ex);
        }
    }
}