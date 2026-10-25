using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using DiscordRPC;
using Microsoft.Extensions.DependencyInjection;
using Tavstal.BeaconSDK.Helpers.IO;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Views;

namespace Tavstal.KonkordLauncher.Desktop;

// ReSharper disable once PartialTypeWithSinglePart - Avalonia code generation
public partial class App : Application
{
    private static IBeaconLogger? _logger;
    // ReSharper disable once NotAccessedField.Local - Used to ensure the task is not garbage collected before completion
    private static Task? _initializeTask;
    private static DiscordRpcClient? _rpcClient;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        _initializeTask = InitAsync();
    }

    private Task InitAsync()
    {
        try
        {
            _logger = Program.Services.GetRequiredService<IBeaconLogger<App>>();
            var eventService = Program.Services.GetRequiredService<IEventService>();
            eventService.OnThemeChanged += ApplyTheme;

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

        return Task.CompletedTask;
    }

    /// <summary>
    /// Configures the application after the framework initialization is completed.
    /// Sets up the main window for the desktop-style application lifetime.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new StartupWindow();
#if DEBUG
            this.AttachDeveloperTools();
#endif

            var launcherStore = Program.Services.GetRequiredService<ILauncherStore>();
            desktop.ShutdownRequested += (_, _) =>
            {
                try
                {
                    FileSystemHelper.DeleteDirectory(launcherStore.GetTempDir());
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
            RequestedThemeVariant = theme switch
            {
                EThemeType.LIGHT => ThemeVariant.Light,
                EThemeType.DARK => ThemeVariant.Dark,
                _ => RequestedThemeVariant
            };
        }
        catch (Exception ex)
        {
            _logger?.LogCritical(ex, "Failed to apply theme");
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
                _logger?.LogError("Discord RPC client is not initialized or disposed.");
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
            _logger?.LogCritical(ex, "Failed to update Discord RPC");
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
                _logger?.LogError("Discord RPC client is not initialized or disposed.");
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
            _logger?.LogCritical(ex, "Failed to clear Discord RPC");
        }
    }
}
