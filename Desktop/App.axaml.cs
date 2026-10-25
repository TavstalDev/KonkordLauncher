using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using Tavstal.BeaconSDK.Helpers.IO;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Services;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;
using Tavstal.KonkordLauncher.Desktop.Views;

namespace Tavstal.KonkordLauncher.Desktop;

// ReSharper disable once PartialTypeWithSinglePart - Avalonia code generation
public partial class App : Application
{
    private static IBeaconLogger? _logger;
    // ReSharper disable once NotAccessedField.Local - Used to ensure the task is not garbage collected before completion
    private static Task? _initializeTask;

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
            desktop.MainWindow = Program.Services.GetRequiredService<StartupWindow>();
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
        Avalonia.Threading.Dispatcher.UIThread.Invoke(() =>
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
        });
    }
}
